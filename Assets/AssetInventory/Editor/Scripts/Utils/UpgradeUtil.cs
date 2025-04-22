using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public static class UpgradeUtil
    {
        public const int CURRENT_CONFIG_VERSION = 2;
        private const int CURRENT_DB_VERSION = 18;

        public static bool LongUpgradeRequired { get; private set; }
        private static List<string> PendingUpgrades { get; set; } = new List<string>();
        private static Vector2 _scrollPos;

        public static void PerformUpgrades()
        {
            // filename was introduced in version 2
            AppProperty dbVersion = DBAdapter.DB.Find<AppProperty>("Version");
            int oldVersion;

            AppProperty requireUpgrade = DBAdapter.DB.Find<AppProperty>("UpgradeRequired");
            LongUpgradeRequired = requireUpgrade != null && requireUpgrade.Value.ToLowerInvariant() == "true";

            if (dbVersion == null)
            {
                // Upgrade from Initial to v2
                // add filenames to DB
                List<AssetFile> assetFiles = DBAdapter.DB.Table<AssetFile>().ToList();
                foreach (AssetFile assetFile in assetFiles)
                {
                    assetFile.FileName = Path.GetFileName(assetFile.Path);
                }
                DBAdapter.DB.UpdateAll(assetFiles);
                oldVersion = CURRENT_DB_VERSION;
            }
            else
            {
                oldVersion = int.Parse(dbVersion.Value);
            }
            if (oldVersion > CURRENT_DB_VERSION)
            {
                Debug.LogError("You are using an outdated version of Asset Inventory with a database that was created/used by a newer version. This can lead to data inconsistencies. It is highly recommended to use the same Asset Inventory version in all projects accessing the same database.");
            }
            if (oldVersion < 5)
            {
                // force re-fetching of asset details to get state
                DBAdapter.DB.Execute("update Asset set ETag=null, LastOnlineRefresh=0");
                LongUpgradeRequired = true;

                // change how colors are indexed
                if (DBAdapter.ColumnExists("AssetFile", "DominantColor")) DBAdapter.DB.Execute("alter table AssetFile drop column DominantColor");
                if (DBAdapter.ColumnExists("AssetFile", "DominantColorGroup")) DBAdapter.DB.Execute("alter table AssetFile drop column DominantColorGroup");

                requireUpgrade = new AppProperty("UpgradeRequired", "true");
                DBAdapter.DB.InsertOrReplace(requireUpgrade);

                AppProperty upgradeType = new AppProperty("UpgradeType-PreviewConversion", "true");
                DBAdapter.DB.InsertOrReplace(upgradeType);
            }
            if (oldVersion < 6)
            {
                DBAdapter.DB.Execute("update AssetFile set Hue=-1");
            }
            if (oldVersion < 7)
            {
                if (DBAdapter.ColumnExists("AssetFile", "PreviewFile"))
                {
                    DBAdapter.DB.Execute("alter table AssetFile drop column PreviewFile");
                    DBAdapter.DB.Execute("update AssetFile set PreviewState=99 where PreviewState=0");
                    DBAdapter.DB.Execute("update AssetFile set PreviewState=0 where PreviewState=1");
                    DBAdapter.DB.Execute("update AssetFile set PreviewState=1 where PreviewState=99");
                }
            }
            if (oldVersion < 8)
            {
                if (DBAdapter.ColumnExists("AssetFile", "PreviewImage"))
                {
                    DBAdapter.DB.Execute("alter table AssetFile drop column PreviewImage");
                }
            }
            if (oldVersion < 9)
            {
                if (DBAdapter.ColumnExists("Asset", "PreferredVersion"))
                {
                    DBAdapter.DB.Execute("alter table Asset drop column PreferredVersion");
                }
            }
            if (oldVersion < 10)
            {
                // force re-fetching of asset details to get new state
                DBAdapter.DB.Execute("update Asset set ETag=null, LastOnlineRefresh=0");
            }
            if (oldVersion < 11)
            {
                // force rescanning local assets once to get all correct metadata
                DBAdapter.DB.InsertOrReplace(new AppProperty("ForceLocalUpdate", "true"));
            }
            if (oldVersion < 12)
            {
                if (DBAdapter.ColumnExists("Asset", "PreviewImage"))
                {
                    DBAdapter.DB.Execute("alter table Asset drop column PreviewImage");
                }
                if (DBAdapter.ColumnExists("Asset", "MainImage"))
                {
                    DBAdapter.DB.Execute("alter table Asset drop column MainImage");
                }
                if (DBAdapter.ColumnExists("Asset", "MainImageIcon"))
                {
                    DBAdapter.DB.Execute("alter table Asset drop column MainImageIcon");
                }
                if (DBAdapter.ColumnExists("Asset", "MainImageSmall"))
                {
                    DBAdapter.DB.Execute("alter table Asset drop column MainImageSmall");
                }
                if (DBAdapter.ColumnExists("AssetFile", "ProjectPath"))
                {
                    DBAdapter.DB.Execute("alter table AssetFile drop column ProjectPath");
                }
            }
            if (oldVersion < 13)
            {
                // convert asset cache index to relative structure
                requireUpgrade = new AppProperty("UpgradeRequired", "true");
                DBAdapter.DB.InsertOrReplace(requireUpgrade);
                LongUpgradeRequired = true;

                AppProperty upgradeType = new AppProperty("UpgradeType-AssetCacheConversion", "true");
                DBAdapter.DB.InsertOrReplace(upgradeType);
            }
            if (oldVersion < 14)
            {
                // rename extracted folders
                RenameExtractedFolders();
            }
            if (oldVersion < 15)
            {
                // fix incorrect Safe entries
                DBAdapter.DB.Execute("UPDATE Asset SET SafeCategory = REPLACE(SafeCategory, ?, ?)", "/", " ");
                DBAdapter.DB.Execute("UPDATE Asset SET SafePublisher = REPLACE(SafePublisher, ?, ?)", "/", " ");
            }
            if (oldVersion < 16)
            {
                // add dates to custom packages
                requireUpgrade = new AppProperty("UpgradeRequired", "true");
                DBAdapter.DB.InsertOrReplace(requireUpgrade);
                LongUpgradeRequired = true;

                AppProperty upgradeType = new AppProperty("UpgradeType-CustomPackageDates", "true");
                DBAdapter.DB.InsertOrReplace(upgradeType);
            }
            if (oldVersion < 17)
            {
                DBAdapter.DB.Execute("DROP INDEX IF EXISTS AssetFile_Path");
                DBAdapter.DB.Execute("CREATE INDEX \"AssetFile_Path\" ON \"AssetFile\" (\"Path\" COLLATE NOCASE)");

                DBAdapter.DB.Execute("DROP INDEX IF EXISTS AssetFile_FileName");
                DBAdapter.DB.Execute("CREATE INDEX \"AssetFile_FileName\" ON \"AssetFile\" (\"FileName\" COLLATE NOCASE)");
            }
            if (oldVersion < 18)
            {
                DBAdapter.DB.Execute("UPDATE AssetFile set SourcePath = Path where SourcePath like '%/Extracted/%'");
            }
            if (dbVersion == null || (oldVersion < CURRENT_DB_VERSION && !LongUpgradeRequired))
            {
                DBAdapter.DB.InsertOrReplace(new AppProperty("Version", CURRENT_DB_VERSION.ToString()));
                if (dbVersion != null) Debug.Log($"Asset Inventory database upgraded to version {CURRENT_DB_VERSION}");
            }

            // check for config upgrades
            int oldConfigVersion = AI.Config.version;
            if (oldConfigVersion < 2)
            {
                // change media folders type after introducing new "all" type
                AI.Config.folders.ForEach(f =>
                {
                    if (f.scanFor > 0) f.scanFor++;
                });
            }
            if (oldConfigVersion < CURRENT_CONFIG_VERSION)
            {
                AI.Config.version = CURRENT_CONFIG_VERSION;
                AI.SaveConfig();
                Debug.Log($"Asset Inventory configuration upgraded to version {CURRENT_CONFIG_VERSION}");
            }

            PendingUpgrades = DBAdapter.DB.Table<AppProperty>()
                .Where(a => a.Name.StartsWith("UpgradeType-"))
                .Select(a => a.Name.Substring(12))
                .ToList();
        }

        private static void RenameExtractedFolders()
        {
            List<Asset> assets = DBAdapter.DB.Table<Asset>().ToList();
            foreach (Asset asset in assets)
            {
                if (asset.AssetSource == Asset.Source.Directory || asset.AssetSource == Asset.Source.RegistryPackage) continue;

                string expectedPath = AI.GetMaterializedAssetPath(asset);
                string oldPath = GetOldMaterializedAssetPath(asset);
                if (Directory.Exists(oldPath) && !Directory.Exists(expectedPath))
                {
                    Directory.Move(oldPath, expectedPath);
                }
            }
        }

        private static string GetOldMaterializedAssetPath(Asset asset)
        {
            return IOUtils.PathCombine(AI.GetMaterializeFolder(), asset.SafeName);
        }

        private static async void StartLongRunningUpgrades()
        {
            foreach (string upgrade in PendingUpgrades)
            {
                switch (upgrade.ToLowerInvariant())
                {
                    case "previewconversion":
                        await UpgradePreviewImageStructure();
                        break;

                    case "assetcacheconversion":
                        UpgradeAssetCashLocation();
                        break;

                    case "custompackagedates":
                        await UpgradeCustomPackageDates();
                        break;
                }
            }

            DBAdapter.DB.Execute("delete from AppProperty where Name like ?", "UpgradeType-%");
            DBAdapter.DB.Delete<AppProperty>("UpgradeRequired");
            AppProperty newVersion = new AppProperty("Version", CURRENT_DB_VERSION.ToString());
            DBAdapter.DB.InsertOrReplace(newVersion);

            LongUpgradeRequired = false;
            AI.TriggerPackageRefresh();
        }

        private static async Task UpgradeCustomPackageDates()
        {
            AI.CurrentMain = "Upgrading preview images structure...";

            List<Asset> assets = DBAdapter.DB.Table<Asset>().Where(a => a.AssetSource == Asset.Source.CustomPackage && a.LastRelease == DateTime.MinValue && a.ParentId == 0).ToList();
            AI.MainCount = assets.Count;
            AI.MainProgress = 0;

            foreach (Asset asset in assets)
            {
                AI.MainProgress++;
                AI.CurrentMainItem = asset.SafeName;
                if (AI.MainProgress % 1000 == 0) await Task.Yield();

                if (asset.Location == null) continue;
                string loc = AI.DeRel(asset.Location);
                if (!File.Exists(loc)) continue;

                FileInfo fInfo = new FileInfo(loc);
                asset.LastRelease = fInfo.LastWriteTime;
                DBAdapter.DB.Update(asset);
            }

            AI.CurrentMain = null;
        }

        private static async Task UpgradePreviewImageStructure()
        {
            AI.CurrentMain = "Upgrading preview images structure...";

            string previewFolder = AI.GetPreviewFolder();
            IEnumerable<string> files = IOUtils.GetFiles(previewFolder, new[] {"*.png"});
            AI.MainCount = files.Count();
            AI.MainProgress = 0;

            int cleanedFiles = 0;
            foreach (string file in files)
            {
                AI.MainProgress++;
                AI.CurrentMainItem = file;
                if (AI.MainProgress % 1000 == 0) await Task.Yield();

                string[] arr = Path.GetFileNameWithoutExtension(file).Split('-');

                string assetId;
                switch (arr[0])
                {
                    case "a":
                        assetId = arr[1];
                        break;

                    case "af":
                        int fileId = int.Parse(arr[1]);
                        AssetFile af = DBAdapter.DB.Find<AssetFile>(fileId);
                        if (af == null)
                        {
                            // legacy, can be removed
                            cleanedFiles++;
                            File.Delete(file);
                            continue;
                        }
                        assetId = af.AssetId.ToString();
                        break;

                    default:
                        Debug.LogError($"Unknown preview type: {file}");
                        continue;
                }

                // move file from root into new sub-structure
                string targetDir = Path.Combine(previewFolder, assetId);
                string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                if (File.Exists(targetFile)) File.Delete(targetFile);
                File.Move(file, targetFile);
            }
            Debug.Log($"Cleaned up orphaned preview files: {cleanedFiles}");

            AI.CurrentMain = null;
        }

        private static void UpgradeAssetCashLocation()
        {
            AI.CurrentMain = "Aligning all paths to use forward slashes...";
            int affected = DBAdapter.DB.Execute("UPDATE Asset SET Location = REPLACE(Location, ?, ?)", "\\", "/");
            Debug.Log($"Changed asset paths: {affected}");

            affected = DBAdapter.DB.Execute("UPDATE Asset SET SafeName = REPLACE(SafeName, ?, ?)", "\\", "/");
            Debug.Log($"Changed asset safe names: {affected}");

            affected = DBAdapter.DB.Execute("UPDATE AssetFile SET Path = REPLACE(Path, ?, ?)", "\\", "/");
            Debug.Log($"Changed asset file paths: {affected}");

            affected = DBAdapter.DB.Execute("UPDATE AssetFile SET SourcePath = REPLACE(SourcePath, ?, ?)", "\\", "/");
            Debug.Log($"Changed asset file source paths: {affected}");

            AI.CurrentMain = "Upgrading asset cache location persistence...";
            string oldPrefix = AI.GetAssetCacheFolder();
            string newPrefix = "[ac]";
            affected = DBAdapter.DB.Execute("UPDATE Asset SET Location = REPLACE(Location, ?, ?) WHERE Location LIKE ?", oldPrefix, newPrefix, oldPrefix + "%");
            Debug.Log($"Converted asset cache entries: {affected}");

            AI.CurrentMain = "Upgrading package cache location persistence...";
            oldPrefix = AI.GetPackageCacheFolder();
            newPrefix = "[pc]";
            affected = DBAdapter.DB.Execute("UPDATE Asset SET Location = REPLACE(Location, ?, ?) WHERE Location LIKE ?", oldPrefix, newPrefix, oldPrefix + "%");
            Debug.Log($"Converted package cache entries: {affected}");

            AI.CurrentMain = null;
        }

        public static void DrawUpgradeRequired()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(BasicEditorUI.Logo, EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("A longer or incompatible database upgrade is required for this version.", UIStyles.whiteCenter);
            EditorGUILayout.LabelField("It's recommended to make a backup of your database.", EditorStyles.centeredGreyMiniLabel);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            EditorGUILayout.Space(30);
            EditorGUILayout.LabelField("Pending Upgrades", EditorStyles.boldLabel);
            for (int i = 0; i < PendingUpgrades.Count; i++)
            {
                string upgrade = PendingUpgrades[i];

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField((i + 1) + ".", GUILayout.Width(15));
                switch (upgrade.ToLowerInvariant())
                {
                    case "previewconversion":
                        EditorGUILayout.LabelField("Upgrade preview images structure");
                        break;

                    case "assetcacheconversion":
                        EditorGUILayout.LabelField("Store asset cache paths in a relative fashion in the database, making it easier reusable across devices and align all paths to use forward slashes", EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(300));
                        break;

                    case "custompackagedates":
                        EditorGUILayout.LabelField("Set the last modified date of a custom package as their release date to enable sorting by date", EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(300));
                        break;

                    default:
                        Debug.LogError("Unknown upgrade type: " + upgrade);
                        break;

                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(AI.CurrentMain));
            if (GUILayout.Button("Start Upgrade Process", GUILayout.Height(50))) StartLongRunningUpgrades();
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(AI.CurrentMain))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(AI.CurrentMain, UIStyles.whiteCenter);
                EditorGUILayout.Space();
                UIStyles.DrawProgressBar(AI.MainProgress / (float)AI.MainCount, AI.CurrentMainItem);
            }
            GUILayout.EndScrollView();
        }
    }
}