using System.Threading.Tasks;

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Unity.Cloud.Assets;
using Unity.Cloud.Identity;
#endif

namespace AssetInventory
{
    public sealed class AssetManagerImporter : AssetImporter
    {
        public async Task Index(Asset forAsset = null)
        {
            ResetState(false);

            int progressId = MetaProgress.Start("Updating Asset Manager index");

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            await cam.GetOrganizationsAsync();

            if (cam.AvailableOrganizations != null)
            {
                foreach (IOrganization organization in cam.AvailableOrganizations)
                {
                    if (CancellationRequested) break;

                    cam.SetSelectedOrganization(organization);
                    await cam.GetProjectsAsync();

                    for (int i = 0; i < cam.AvailableProjects.Count; i++)
                    {
                        if (CancellationRequested) break;

                        IAssetProject project = cam.AvailableProjects[i];
                        IProject projectAlt = cam.AvailableProjectsAlt.FirstOrDefault(p => p.Descriptor.ProjectId == project.Descriptor.ProjectId);

                        CurrentMain = $"Processing project {project.Name} ({organization.Name})...";
                        MainCount = cam.AvailableProjects.Count;
                        MainProgress = i + 1;
                        MetaProgress.Report(progressId, i + 1, MainCount, project.Name);

                        Asset asset = new Asset();
                        asset.AssetSource = Asset.Source.AssetManager;
                        asset.SafeName = project.Descriptor.ProjectId.ToString();
                        if (forAsset != null && asset.SafeName != forAsset.SafeName) continue;

                        Asset existing = Fetch(asset);
                        if (existing != null)
                        {
                            if (existing.Exclude) continue;
                            asset = existing;
                        }

                        asset.DisplayName = project.Name;
                        asset.OriginalLocation = organization.Name;
                        asset.OriginalLocationKey = organization.Id.ToString();
                        asset.CurrentState = Asset.State.InProcess;

                        if (project.Metadata != null)
                        {
                            ProjectMetadata pmd = JsonConvert.DeserializeObject<ProjectMetadata>(project.Metadata.GetAsString());
                            asset.LastRelease = pmd.UpdatedAt;
                        }
                        Persist(asset);

                        // icon is only available through alternative project descriptor
                        if (projectAlt != null)
                        {
                            if (!string.IsNullOrWhiteSpace(projectAlt.IconUrl))
                            {
                                string previewFile = asset.GetPreviewFile(AI.GetPreviewFolder(), false);
                                if (!File.Exists(previewFile))
                                {
                                    await AssetUtils.LoadImageAsync(projectAlt.IconUrl, previewFile);
                                }
                            }
                        }

                        cam.SetSelectedProject(project);
                        await cam.GetProjectAssetsAsync();

                        await PersistAssetFiles(cam, cam.AvailableAssets, asset);

                        asset.CurrentState = Asset.State.Done;
                        asset.LastOnlineRefresh = DateTime.Now;

                        Persist(asset);

                        await IndexCollections(cam, asset);
                    }
                }
            }
#else
            await Task.Yield();
#endif
            MetaProgress.Remove(progressId);
            ResetState(true);
        }

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
        public static async Task PersistAssetFiles(CloudAssetManagement cam, List<IAsset> assets, Asset asset, bool checkOrphans = true)
        {
            bool tagsChanged = false;

            List<AssetFile> orphanedFiles = new List<AssetFile>();
            if (checkOrphans) orphanedFiles = DBAdapter.DB.Query<AssetFile>("select * from AssetFile where AssetId = ?", asset.Id);
            for (int i2 = 0; i2 < assets.Count; i2++)
            {
                if (CancellationRequested) break;

                IAsset cloudAsset = assets[i2];

                CurrentSub = IOUtils.RemoveInvalidChars(cloudAsset.Name);
                SubCount = assets.Count;
                SubProgress = i2;

                AssetFile af = new AssetFile();
                af.AssetId = asset.Id;
                af.Guid = cloudAsset.Descriptor.AssetId.ToString();

                string version = cloudAsset.Descriptor.AssetVersion.ToString();
                string status = cloudAsset.StatusName;

                AssetFile existingAf = Fetch(af);
                if (existingAf != null)
                {
                    orphanedFiles.RemoveAll(f => f.Id == existingAf.Id);

                    if (existingAf.FileVersion == version && existingAf.FileStatus == status) continue;
                    af = existingAf;

                    af.PreviewState = AssetFile.PreviewOptions.Redo;
                    af.Hue = -1f;
                }
                af.FileVersion = version;
                af.FileStatus = status;
                af.FileName = cloudAsset.Name;
                af.Path = cloudAsset.Name;
                af.SourcePath = af.Guid;
                af.Type = cloudAsset.Type.ToString();

                // load attached files
                cam.SetSelectedAsset(cloudAsset);
                await cam.GetDatasetsAsync();

                List<IFile> files = await cam.GetAllFilesAsync();
                int totalFiles = files.Count;
                long totalSize = files.Sum(f => f.SizeBytes);

                // if there is only one file expose it directly so all downstream features like audio preview work nicely
                if (totalFiles == 1)
                {
                    IFile file = files.First();
                    af.Path = file.Descriptor.Path;
                    af.FileName = Path.GetFileName(af.Path);
                    af.Type = IOUtils.GetExtensionWithoutDot(af.FileName).ToLowerInvariant();
                }
                af.Size = totalSize;
                Persist(af);

                // load preview
                FetchPreview(cloudAsset, af);

                // add tags
                foreach (string tag in cloudAsset.Tags)
                {
                    if (Tagging.AddTagAssignment(af.Id, tag, TagAssignment.Target.Asset, true)) tagsChanged = true;
                }
            }
            if (checkOrphans && orphanedFiles.Count > 0)
            {
                orphanedFiles.ForEach(AI.ForgetAssetFile);
            }
            if (tagsChanged)
            {
                Tagging.LoadTags();
                Tagging.LoadTagAssignments();
            }
            SubCount = 0;
        }

        private async Task IndexCollections(CloudAssetManagement cam, Asset parent)
        {
            await cam.ListProjectAssetCollectionsAsync();
            if (cam.AssetCollections.Count == 0) return;

            Dictionary<string, int> assetMapping = new Dictionary<string, int>();

            for (int i = 0; i < cam.AssetCollections.Count; i++)
            {
                if (CancellationRequested) break;

                IAssetCollection collection = cam.AssetCollections[i];

                CurrentMain = $"Processing collection {collection.Descriptor.Path.ToString().Replace("/", "-")}...";
                MainCount = cam.AssetCollections.Count;
                MainProgress = i + 1;

                string parentPath = collection.ParentPath.ToString();

                Asset asset = new Asset();
                asset.AssetSource = Asset.Source.AssetManager;
                asset.Location = collection.Descriptor.Path.ToString();
                asset.SafeName = AssetUtils.GuessSafeName(asset.Location);
                asset.ParentId = string.IsNullOrWhiteSpace(parentPath) ? parent.Id : assetMapping[parentPath];

                Asset existing = Fetch(asset);
                if (existing != null)
                {
                    if (existing.Exclude) continue;
                    asset = existing;

                    // set partially again to update existing
                    asset.Location = collection.Descriptor.Path.ToString();
                    asset.SafeName = AssetUtils.GuessSafeName(asset.Location);
                }
                asset.DisplayName = collection.Name;
                asset.OriginalLocation = parent.OriginalLocation;
                asset.OriginalLocationKey = parent.OriginalLocationKey;
                asset.CurrentState = Asset.State.InProcess;
                Persist(asset);
                assetMapping.Add(asset.Location, asset.Id);

                cam.SetCurrentCollection(collection);
                await cam.ListCollectionAssetsAsync();

                await PersistAssetFiles(cam, cam.CurrentCollectionAssets, asset);

                asset.CurrentState = Asset.State.Done;
                asset.LastOnlineRefresh = DateTime.Now;

                Persist(asset);
            }

            // check for deleted collections
            List<Asset> children = parent.GetChildren();
            List<Asset> orphaned = children.Where(c => !assetMapping.ContainsValue(c.Id)).ToList();
            orphaned.ForEach(a => AI.RemovePackage(new AssetInfo(a), false));
        }

        private static async void FetchPreview(IAsset cloudAsset, AssetFile af)
        {
            if (af.PreviewState == AssetFile.PreviewOptions.Error) return;
            string targetFile = af.GetPreviewFile(AI.GetPreviewFolder());
            if (File.Exists(targetFile) && af.PreviewState != AssetFile.PreviewOptions.Redo && af.PreviewState != AssetFile.PreviewOptions.None) return;

            Uri url = await cloudAsset.GetPreviewUrlAsync(CancellationToken.None);
            if (url == null)
            {
                af.PreviewState = AssetFile.PreviewOptions.Error;
                Persist(af);
                return;
            }
            await AssetUtils.LoadImageAsync(url.ToString(), targetFile);

            af.PreviewState = AssetFile.PreviewOptions.Provided;
            Persist(af);
        }
#endif
    }
}
