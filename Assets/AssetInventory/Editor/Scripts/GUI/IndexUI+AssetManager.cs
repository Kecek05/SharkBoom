using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
#else
using UnityEditor.PackageManager;
#endif

namespace AssetInventory
{
    public partial class IndexUI
    {
        private Rect _amUploadButtonRect;

        private void DrawAssetManager()
        {
            EditorGUI.BeginChangeCheck();
            AI.Config.indexAssetManager = GUILayout.Toggle(AI.Config.indexAssetManager, "Unity Asset Manager", GUILayout.MaxWidth(250));
            if (EditorGUI.EndChangeCheck()) AI.SaveConfig();

            if (AI.Config.indexAssetManager)
            {
                BeginIndentBlock(16);

#if !UNITY_2022_3_OR_NEWER
                EditorGUILayout.HelpBox("In order to connect to Asset Manager, Unity 2022.3 or higher is required.", MessageType.Error);
#elif !USE_ASSET_MANAGER || !USE_CLOUD_IDENTITY
                EditorGUILayout.HelpBox("In order to connect to Asset Manager, the following packages need to be installed: Unity Cloud Assets 1.4.0+, Unity Cloud Identity 1.3.0+", MessageType.Error);
                if (GUILayout.Button("Install Packages"))
                {
                    Client.AddAndRemove(new[] {"com.unity.cloud.assets@1.4.0", "com.unity.cloud.identity@1.3.0"});
                }
#else
                if (string.IsNullOrWhiteSpace(CloudProjectSettings.accessToken))
                {
                    EditorGUILayout.HelpBox("Please log in to Unity Cloud Identity to use Asset Manager.", MessageType.Info);
                    if (GUILayout.Button("Log In"))
                    {
                        CloudProjectSettings.ShowLogin();
                    }
                }
                else
                {
                    GUILabelWithText("Current User", CloudProjectSettings.userName);
                }
                GUILabelWithText("Organizations", "-All-");
                GUILabelWithText("Projects", "-All-");
#endif

                EndIndentBlock(false);
            }
        }

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
        private async void CreateCollection(AssetInfo parent, string colName)
        {
            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            await cam.SelectProjectAsync(parent.ToAsset());

            // create remote
            string path = parent.ParentInfo != null ? parent.SafeName + "/" + colName : colName;
            IAssetCollection collection = await cam.CreateAssetCollectionAsync(new CollectionPath(path));
            if (collection == null) return;

            // create local equivalent 
            Asset asset = new Asset();
            asset.AssetSource = Asset.Source.AssetManager;
            asset.SafeName = AssetUtils.GuessSafeName(collection.Descriptor.Path);
            asset.ParentId = parent.AssetId;
            asset.DisplayName = collection.Name;
            asset.Location = collection.Descriptor.Path.ToString();
            asset.OriginalLocation = parent.OriginalLocation;
            asset.OriginalLocationKey = parent.OriginalLocationKey;
            asset.CurrentState = Asset.State.Done;
            AssetImporter.Persist(asset);

            AI.TriggerPackageRefresh();
        }

        private async void DeleteCollection(AssetInfo info)
        {
            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            IAssetProject project = await cam.SelectProjectAsync(info.ToAsset());
            if (project == null) return;
            await cam.DeleteAssetCollectionAsync(info.Location);

            AI.RemovePackage(info, true);
            AI.TriggerPackageRefresh();
        }

        private async void AddAssetsToProject(AssetInfo project, List<AssetInfo> assets)
        {
            CloudAssetManagement.IncBusyCount();

            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            await cam.SelectProjectAsync(project.ToAsset());

            IAssetCollection collection = null;
            if (!string.IsNullOrWhiteSpace(project.Location))
            {
                collection = await cam.SelectProjectAssetCollectionAsync(project.ToAsset());
            }

            List<IAsset> newAssets = new List<IAsset>();
            foreach (AssetInfo info in assets)
            {
                string path;
                if (info.DependencyState == AssetInfo.DependencyStateOptions.Unknown) await CalculateDependencies(info);
                bool folderMode = info.Dependencies.Count > 0;
                if (folderMode)
                {
                    path = IOUtils.CreateTempFolder();
                    await AI.CopyTo(info, path, true, true, false, true);

                    // use asset root and not temp folder name as root
                    string[] dirs = Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
                    if (dirs.Length == 1 && Directory.Exists(dirs[0])) path = dirs[0];
                }
                else
                {
                    path = await AI.EnsureMaterializedAsset(info.ToAsset(), info);
                }
                if (path == null)
                {
                    Debug.LogError($"Could not materialize '{info}' for upload.");
                    continue;
                }

                // Generate new asset
                AssetType type = info.GetAMAssetType();
                List<string> tags = info.AssetTags.Select(at => at.Name).ToList();
                Dictionary<string, MetadataValue> metadata = new Dictionary<string, MetadataValue>();
                if (type == AssetType.Asset_2D)
                {
                    // TODO: default resolution field seems to be a one value pre-defined list for some reason
                }
                IAsset cloudAsset = await cam.CreateAssetAsync(type, info.FileName, null, tags, metadata);
                cam.SetSelectedAsset(cloudAsset);

                IDataset dataset = await cloudAsset.GetSourceDatasetAsync(CancellationToken.None);
                if (dataset == null) continue;

                // start in parallel
                Task previewUpload = UploadPreview(info, cloudAsset, cam);

                // for files with dependencies upload complete folder
                if (folderMode)
                {
                    if (!await cam.UploadFolderAsync(dataset, path)) continue;
                    await IOUtils.DeleteFileOrDirectory(path);
                }
                else
                {
                    IFile cloudFile = await cam.UploadFile(dataset, path);
                    if (cloudFile == null) continue;
                }
                await previewUpload;

                newAssets.Add(cloudAsset);

                // link to collection if selected
                if (collection == null) continue;
                await cam.LinkAssetToCollectionAsync(cloudAsset);
            }

            // add to project
            await AssetManagerImporter.PersistAssetFiles(cam, newAssets, project.ToAsset().GetRootAsset(), false);

            // add to collection
            if (collection != null)
            {
                await AssetManagerImporter.PersistAssetFiles(cam, newAssets, project.ToAsset(), false);
            }
            AI.TriggerPackageRefresh();

            CloudAssetManagement.DecBusyCount();
        }

        private static async Task UploadPreview(AssetInfo info, IAsset cloudAsset, CloudAssetManagement cam)
        {
            // Upload preview if existent
            string previewFile = info.GetPreviewFile(AI.GetPreviewFolder());
            if (File.Exists(previewFile))
            {
                IDataset previewDataset = await cloudAsset.GetPreviewDatasetAsync(CancellationToken.None);
                if (previewDataset == null) return;

                await cam.UploadFile(previewDataset, previewFile);
            }
        }

        private async void RemoveAssetsFromCollection(List<AssetInfo> assets)
        {
            CloudAssetManagement.IncBusyCount();

            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            foreach (AssetInfo info in assets)
            {
                if (info.AssetSource != Asset.Source.AssetManager) continue;

                IAssetCollection collection = await cam.SelectProjectAssetCollectionAsync(info.ToAsset());
                if (collection == null) continue;

                await cam.ListCollectionAssetsAsync();
                IAsset cloudAsset = cam.CurrentCollectionAssets.FirstOrDefault(a => a.Descriptor.AssetId.ToString() == info.Guid);
                if (cloudAsset == null) continue;

                cam.SetSelectedAsset(cloudAsset);
                await cam.UnlinkAssetFromCollectionAsync(cloudAsset);

                AI.ForgetAssetFile(info);
            }
            AI.TriggerPackageRefresh();

            CloudAssetManagement.DecBusyCount();
        }

        private async void DeleteAssetsFromProject(List<AssetInfo> assets)
        {
            CloudAssetManagement.IncBusyCount();

            CloudAssetManagement cam = await AI.GetCloudAssetManagement();
            foreach (AssetInfo info in assets)
            {
                if (info.AssetSource != Asset.Source.AssetManager) continue;

                IAssetProject project = await cam.SelectProjectAsync(info.ToAsset());
                if (project == null) continue;

                await cam.CurrentProject.UnlinkAssetsAsync(new[] {new AssetId(info.Guid)}, CancellationToken.None);
                AI.ForgetAssetFile(info);
            }
            AI.TriggerPackageRefresh();

            CloudAssetManagement.DecBusyCount();
        }
#endif
    }
}
