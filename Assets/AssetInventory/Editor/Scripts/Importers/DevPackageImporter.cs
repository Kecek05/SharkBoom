using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;

namespace AssetInventory
{
    public sealed class DevPackageImporter : AssetImporter
    {
        private const int BREAK_INTERVAL = 30;

        public async Task Index(FolderSpec spec)
        {
            ResetState(false);

            if (string.IsNullOrEmpty(spec.location)) return;

            string fullLocation = spec.GetLocation(true);
            bool treatAsUnityProject = spec.detectUnityProjects && AssetUtils.IsUnityProject(fullLocation);
            string[] files = IOUtils.GetFiles(treatAsUnityProject ? Path.Combine(fullLocation, "Assets") : fullLocation, new[] {"package.json"}, SearchOption.AllDirectories).ToArray();

            MainCount = files.Length;
            MainProgress = 1; // small hack to trigger UI update in the end

            int progressId = MetaProgress.Start("Updating dev package index");
            for (int i = 0; i < files.Length; i++)
            {
                if (CancellationRequested) break;
                if (i % BREAK_INTERVAL == 0) await Task.Yield(); // let editor breath in case many files are already indexed
                await Cooldown.Do();

                string package = files[i];
                Asset asset = await HandlePackage(package);
                if (asset == null) continue;

                MetaProgress.Report(progressId, i + 1, files.Length, package);
                MainCount = files.Length;
                CurrentMain = asset.DisplayName + " (" + EditorUtility.FormatBytes(asset.PackageSize) + ")";
                MainProgress = i + 1;

                await Task.Yield();
                await IndexPackage(asset, spec);
                await Task.Yield();

                if (CancellationRequested) break;

                if (spec.assignTag && !string.IsNullOrWhiteSpace(spec.tag))
                {
                    Tagging.AddTagAssignment(new AssetInfo(asset), spec.tag, TagAssignment.Target.Package);
                }
            }
            MetaProgress.Remove(progressId);
            ResetState(true);
        }

        private static async Task<Asset> HandlePackage(string package)
        {
            Package info = PackageImporter.ReadPackageFile(package);
            if (info == null) return null;

            // create asset
            Asset asset = await PackageImporter.CreateAsset(info, package, PackageSource.Local);
            if (asset == null) return null;

            // handle tags
            bool tagsChanged = PackageImporter.ApplyTags(asset, info, false);

            asset.CurrentState = Asset.State.InProcess;
            UpdateOrInsert(asset);

            if (tagsChanged)
            {
                Tagging.LoadTags();
                Tagging.LoadTagAssignments();
            }

            return asset;
        }

        public async Task IndexDetails(Asset asset)
        {
            ResetState(false);

            MainCount = 1;
            CurrentMain = "Indexing dev package";

            FolderSpec importSpec = GetDefaultImportSpec();
            importSpec.createPreviews = true; // TODO: derive from additional folder settings
            await IndexPackage(asset, importSpec);

            ResetState(true);
        }

        private static async Task IndexPackage(Asset asset, FolderSpec spec)
        {
            await RemovePersistentCacheEntry(asset);

            FolderSpec importSpec = GetDefaultImportSpec();
            importSpec.location = asset.GetLocation(true);
            importSpec.createPreviews = spec.createPreviews;
            await new MediaImporter().Index(importSpec, asset, false, true);

            MarkDone(asset);
        }
    }
}