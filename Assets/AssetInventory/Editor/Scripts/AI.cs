using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !ASSET_INVENTORY_NOAUDIO
using JD.EditorAudioUtils;
#endif
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
#if !UNITY_2021_2_OR_NEWER
using Unity.SharpZipLib.Zip;
#endif
using UnityEditor;
using UnityEditor.Callbacks;
#if USE_URP_CONVERTER
using UnityEditor.Rendering.Universal;
#endif
using UnityEngine;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace AssetInventory
{
    public static class AI
    {
        public const string VERSION = "2.7.0";
        public const string DEFINE_SYMBOL = "ASSET_INVENTORY";

        internal const string ASSET_STORE_LINK = "https://u3d.as/3e4D";
        internal const string ASSET_STORE_FOLDER_NAME = "Asset Store-5.x";
        internal const string TEMP_FOLDER = "_AssetInventoryTemp";
        internal const int ASSET_STORE_ID = 890400;
        internal const string TAG_START = "[";
        internal const string TAG_END = "]";
        internal static readonly bool DEBUG_MODE = false;

        private const string PARTIAL_INDICATOR = "ai-partial.info";
        private static readonly string[] ConversionExtensions = {"mat", "fbx"};

        internal static string CurrentMain { get; set; }
        internal static string CurrentMainItem { get; set; }
        internal static int MainCount { get; set; }
        internal static int MainProgress { get; set; }
        internal static string UsedConfigLocation { get; private set; }
        internal static DateTime LastIndexUpdate { get; private set; }

        public static event Action OnPackagesUpdated;
        public static event Action OnIndexingDone;
        public static event Action<Asset> OnPackageImageLoaded;

        private const int MAX_DROPDOWN_ITEMS = 25;
        private const int FOLDER_CACHE_TIME = 60;
        private const string CONFIG_NAME = "AssetInventoryConfig.json";
        private const string DIAG_PURCHASES = "Purchases.json";

        private static bool InitDone { get; set; }
        private static UpdateObserver _observer;
        private static readonly TimedCache<string> _assetCacheFolder = new TimedCache<string>();
        private static readonly TimedCache<string> _materializeFolder = new TimedCache<string>();
        private static readonly TimedCache<string> _previewFolder = new TimedCache<string>();

        internal static List<RelativeLocation> RelativeLocations
        {
            get
            {
                if (_relativeLocations == null) LoadRelativeLocations();
                return _relativeLocations;
            }
        }
        private static List<RelativeLocation> _relativeLocations;

        internal static List<RelativeLocation> UserRelativeLocations
        {
            get
            {
                if (_userRelativeLocations == null) LoadRelativeLocations();
                return _userRelativeLocations;
            }
        }
        private static List<RelativeLocation> _userRelativeLocations;

        public static AssetInventorySettings Config
        {
            get
            {
                if (_config == null) LoadConfig();
                return _config;
            }
        }

        private static AssetInventorySettings _config;
        internal static readonly List<string> ConfigErrors = new List<string>();
        internal static bool UICustomizationMode { get; set; }

        public static bool IndexingInProgress { get; set; }
        public static bool ClearCacheInProgress { get; private set; }

        public static Dictionary<string, string[]> TypeGroups { get; } = new Dictionary<string, string[]>
        {
            {"Audio", new[] {"wav", "mp3", "ogg", "aiff", "aif", "mod", "it", "s3m", "xm", "flac"}},
            {
                "Images",
                new[]
                {
                    "png", "jpg", "jpeg", "bmp", "tga", "tif", "tiff", "psd", "svg", "webp", "ico", "gif", "hdr", "iff", "pict"
                }
            },
            {"Videos", new[] {"avi", "asf", "dv", "m4v", "mov", "mp4", "mpg", "mpeg", "ogv", "vp8", "webm", "wmv"}},
            {"Prefabs", new[] {"prefab"}},
            {"Materials", new[] {"mat", "physicmaterial", "physicsmaterial", "sbs", "sbsar", "cubemap"}},
            {"Shaders", new[] {"shader", "shadergraph", "shadersubgraph", "compute", "raytrace"}},
            {"Models", new[] {"fbx", "obj", "blend", "dae", "3ds", "dxf", "max", "c4d", "mb", "ma"}},
            {"Animations", new[] {"anim"}},
            {"Fonts", new[] {"ttf", "otf"}},
            {"Scripts", new[] {"cs", "php", "py", "js", "lua"}},
            {"Libraries", new[] {"zip", "rar", "7z", "unitypackage", "so", "bundle", "dll", "jar"}},
            {"Documents", new[] {"md", "doc", "docx", "txt", "json", "rtf", "pdf", "htm", "html", "readme", "xml", "chm", "csv"}}
        };

        internal static Dictionary<string, string[]> FilterRestriction { get; } = new Dictionary<string, string[]>
        {
            {"Length", new[] {"Audio", "Videos"}},
            {"Width", new[] {"Images", "Videos"}},
            {"Height", new[] {"Images", "Videos"}},
            {"ImageType", new[] {"Images"}}
        };

        internal static int TagHash { get; private set; }

        private static Queue<Asset> _extractionQueue = new Queue<Asset>();
        private static Tuple<Asset, Task> _currentExtraction;
        private static int _extractionProgress;

        [DidReloadScripts(1)]
        private static void AutoInit()
        {
            // this will be run after a recompile so keep to a minimum, e.g. ensure third party tools can work
            EditorApplication.delayCall += () => Init();
            EditorApplication.update += UpdateLoop;
        }

        private static void UpdateLoop()
        {
            if (_extractionQueue.Count > 0)
            {
                if (_extractionProgress == 0) _extractionProgress = MetaProgress.Start("Package Extraction");
                if (_currentExtraction == null || _currentExtraction.Item2.IsCompleted)
                {
                    Asset next = _extractionQueue.Dequeue();
                    MetaProgress.Report(_extractionProgress, 1, _extractionQueue.Count, next.DisplayName);

                    Task task = EnsureMaterializedAsset(next);
                    _currentExtraction = new Tuple<Asset, Task>(next, task);
                }
            }
            else if (_extractionProgress > 0)
            {
                MetaProgress.Remove(_extractionProgress);
                _extractionProgress = 0;
            }
        }

        internal static void ReInit()
        {
            InitDone = false;
            LoadConfig();
            Init();
        }

        public static void Init(bool secondTry = false)
        {
            if (InitDone) return;

            ThreadUtils.Initialize();
            SetupDefines();

            _materializeFolder.Clear();
            _assetCacheFolder.Clear();
            _previewFolder.Clear();

            string folder = GetStorageFolder();
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception e)
                {
                    if (secondTry)
                    {
                        Debug.LogError($"Could not create storage folder for database in default location '{folder}' as well. Giving up: {e.Message}");
                    }
                    else
                    {
                        Debug.LogError($"Could not create storage folder '{folder}' for database. Reverting to default location: {e.Message}");
                        Config.customStorageLocation = null;
                        SaveConfig();
                        Init(true);
                        return;
                    }
                }
            }
            UnityPreviewGenerator.CleanUp();
            UpgradeUtil.PerformUpgrades();
            Tagging.LoadTagAssignments(null, false);
            LoadRelativeLocations();
            UpdateSystemData();

            AppProperty lastIndexUpdate = DBAdapter.DB.Find<AppProperty>("LastIndexUpdate");
            LastIndexUpdate = lastIndexUpdate != null ? DateTime.Parse(lastIndexUpdate.Value, DateTimeFormatInfo.InvariantInfo) : DateTime.MinValue;

            AssetStore.FillBufferOnDemand(true);

            InitDone = true;
        }

        internal static void StartCacheObserver()
        {
            GetObserver().Start();
        }

        internal static void StopCacheObserver()
        {
            GetObserver().Stop();
        }

        internal static bool IsObserverActive()
        {
            return GetObserver().IsActive();
        }

        internal static UpdateObserver GetObserver()
        {
            if (_observer == null) _observer = new UpdateObserver(GetAssetCacheFolder(), new[] {"unitypackage", "tmp"});
            return _observer;
        }

        private static void SetupDefines()
        {
            if (!AssetUtils.HasDefine(DEFINE_SYMBOL)) AssetUtils.AddDefine(DEFINE_SYMBOL);
        }

        private static void UpdateSystemData()
        {
            SystemData data = new SystemData();
            data.Key = SystemInfo.deviceUniqueIdentifier;
            data.Name = SystemInfo.deviceName;
            data.Type = SystemInfo.deviceType.ToString();
            data.Model = SystemInfo.deviceModel;
            data.OS = SystemInfo.operatingSystem;
            data.LastUsed = DateTime.Now;

            try
            {
                DBAdapter.DB.InsertOrReplace(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not update system data: {e.Message}");
            }
        }

        internal static bool IsFileType(string path, string typeGroup)
        {
            if (path == null) return false;
            return TypeGroups[typeGroup].Contains(IOUtils.GetExtensionWithoutDot(path).ToLowerInvariant());
        }

        public static string GetStorageFolder()
        {
            if (!string.IsNullOrEmpty(Config.customStorageLocation)) return Path.GetFullPath(Config.customStorageLocation);

            return IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AssetInventory");
        }

        public static string GetConfigLocation()
        {
            // search for local project-specific override first
            string guid = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(CONFIG_NAME)).FirstOrDefault();
            if (guid != null) return AssetDatabase.GUIDToAssetPath(guid);

            // second fallback is environment variable
            string configPath = Environment.GetEnvironmentVariable("ASSETINVENTORY_CONFIG_PATH");
            if (!string.IsNullOrWhiteSpace(configPath)) return IOUtils.PathCombine(configPath, CONFIG_NAME);

            // finally use from central well-known folder
            return IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), CONFIG_NAME);
        }

        public static string GetPreviewFolder(string customFolder = null, bool noCache = false)
        {
            if (!noCache && _previewFolder.TryGetValue(out string path)) return path;

            string previewPath = IOUtils.PathCombine(customFolder ?? GetStorageFolder(), "Previews");
            if (!Directory.Exists(previewPath)) Directory.CreateDirectory(previewPath);

            if (!noCache) _previewFolder.SetValue(previewPath, TimeSpan.FromSeconds(FOLDER_CACHE_TIME));

            return previewPath;
        }

        public static string GetBackupFolder(bool createOnDemand = true)
        {
            string backupPath = string.IsNullOrWhiteSpace(Config.backupFolder)
                ? IOUtils.PathCombine(GetStorageFolder(), "Backups")
                : Config.backupFolder;
            if (createOnDemand && !Directory.Exists(backupPath)) Directory.CreateDirectory(backupPath);
            return backupPath;
        }

        public static string GetMaterializeFolder()
        {
            if (_materializeFolder.TryGetValue(out string path)) return path;

            string cachePath = string.IsNullOrWhiteSpace(Config.cacheFolder)
                ? IOUtils.PathCombine(GetStorageFolder(), "Extracted")
                : Config.cacheFolder;

            _materializeFolder.SetValue(cachePath, TimeSpan.FromSeconds(FOLDER_CACHE_TIME));

            return cachePath;
        }

        public static string GetMaterializedAssetPath(Asset asset)
        {
            // append the ID to support identically named packages in different locations
            return IOUtils.ToLongPath(IOUtils.PathCombine(GetMaterializeFolder(), asset.SafeName + " - " + asset.Id));
        }

        public static async Task<string> ExtractAsset(Asset asset, AssetFile assetFile = null, bool fileOnly = false)
        {
            if (string.IsNullOrEmpty(asset.GetLocation(true))) return null;

            // make sure parents are extracted first
            string archivePath = IOUtils.ToLongPath(await asset.GetLocation(true, true));
            if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
            {
                if (asset.ParentId <= 0)
                {
                    Debug.LogError($"Asset has vanished since last refresh and cannot be indexed: {archivePath}");

                    // reflect new state
                    // TODO: consider rel systems 
                    asset.SetLocation(null);
                    DBAdapter.DB.Execute("update Asset set Location=null where Id=?", asset.Id);
                }
                return null;
            }

            string tempPath = GetMaterializedAssetPath(asset);

            // delete existing cache if interested in whole bundle to make sure everything is there
            if (assetFile == null || !fileOnly || asset.KeepExtracted)
            {
                int retries = 0;
                while (retries < 5 && Directory.Exists(tempPath))
                {
                    try
                    {
                        await Task.Run(() => Directory.Delete(tempPath, true));
                        break;
                    }
                    catch (Exception)
                    {
                        retries++;
                        await Task.Delay(500);
                    }
                }
                if (Directory.Exists(tempPath)) Debug.LogWarning($"Could not remove temporary directory: {tempPath}");

                try
                {
                    if (asset.AssetSource == Asset.Source.Archive)
                    {
#if UNITY_2021_2_OR_NEWER
                        if (!await Task.Run(() => IOUtils.ExtractArchive(archivePath, tempPath)))
                        {
                            // stop here when archive could not be extracted (e.g. path too long) as otherwise files get removed from index
                            return null;
                        }
#else
                        if (asset.Location.ToLowerInvariant().EndsWith(".zip"))
                        {
                            FastZip fastZip = new FastZip();
                            await Task.Run(() => fastZip.ExtractZip(archivePath, tempPath, null));
                        }
#endif
                    }
                    else
                    {
                        // special handling for Tar as that will throw null errors with SharpCompress
                        await Task.Run(() => TarUtil.ExtractGz(archivePath, tempPath));
                    }

                    // safety delay in case this is a network drive which needs some time to unlock all files
                    await Task.Delay(100);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not extract archive '{archivePath}' due to errors. Index results will be partial: {e.Message}");
                    return null;
                }

                return Directory.Exists(tempPath) ? tempPath : null;
            }

            // single file only
            string targetPath = Path.Combine(GetMaterializedAssetPath(asset), assetFile.GetSourcePath(true));
            if (File.Exists(targetPath)) return targetPath;

            try
            {
                if (asset.AssetSource == Asset.Source.Archive)
                {
                    // TODO: switch to single file
#if UNITY_2021_2_OR_NEWER
                    await Task.Run(() => IOUtils.ExtractArchive(archivePath, tempPath));
#else
                    if (asset.Location.ToLowerInvariant().EndsWith(".zip"))
                    {
                        FastZip fastZip = new FastZip();
                        await Task.Run(() => fastZip.ExtractZip(archivePath, tempPath, null));
                    }
#endif
                }
                else
                {
                    // special handling for Tar as that will throw null errors with SharpCompress
                    await Task.Run(() => TarUtil.ExtractGzFile(archivePath, assetFile.GetSourcePath(true), tempPath));
                    string indicator = Path.Combine(tempPath, PARTIAL_INDICATOR);
                    if (!File.Exists(indicator)) File.WriteAllText(indicator, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                }

                // safety delay in case this is a network drive which needs some time to unlock all files
                await Task.Delay(100);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not extract archive '{archivePath}' due to errors: {e.Message}");
                return null;
            }

            return File.Exists(targetPath) ? targetPath : null;
        }

        public static bool IsMaterialized(Asset asset, AssetFile assetFile = null)
        {
            if (asset.AssetSource == Asset.Source.Directory || asset.AssetSource == Asset.Source.RegistryPackage)
            {
                if (assetFile != null) return File.Exists(assetFile.GetSourcePath(true));
                return Directory.Exists(asset.GetLocation(true));
            }

            string assetPath = GetMaterializedAssetPath(asset);
            if (asset.AssetSource == Asset.Source.AssetManager)
            {
                if (assetFile == null) return false;
                return Directory.Exists(Path.Combine(assetPath, assetFile.Guid));
            }
            return assetFile != null
                ? File.Exists(Path.Combine(assetPath, assetFile.GetSourcePath(true)))
                : Directory.Exists(assetPath);
        }

        public static async Task<string> EnsureMaterializedAsset(AssetInfo info, bool fileOnly = false)
        {
            string targetPath = await EnsureMaterializedAsset(info.ToAsset(), info, fileOnly);
            info.IsMaterialized = IsMaterialized(info.ToAsset(), info);
            return targetPath;
        }

        public static async Task<string> EnsureMaterializedAsset(Asset asset, AssetFile assetFile = null, bool fileOnly = false)
        {
            if (asset.AssetSource == Asset.Source.Directory || asset.AssetSource == Asset.Source.RegistryPackage)
            {
                return File.Exists(assetFile.GetSourcePath(true)) ? assetFile.GetSourcePath(true) : null;
            }

            string targetPath;
            if (asset.AssetSource == Asset.Source.AssetManager)
            {
                if (assetFile == null) return null;

                targetPath = Path.Combine(GetMaterializedAssetPath(asset), assetFile.Guid);
                if (!Directory.Exists(targetPath))
                {
#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
                    CloudAssetManagement cam = await GetCloudAssetManagement();

                    List<string> files = await cam.FetchAssetFromRemote(asset, assetFile, targetPath);
                    if (files == null || files.Count == 0) return null;
#else
                    return null;
#endif
                }

                // special handling for single files
                List<string> allFiles = await Task.Run(() => Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories).ToList());
                if (allFiles.Count == 1) return allFiles[0];

                return targetPath;
            }

            // ensure parent hierarchy is extracted first
            string archivePath = IOUtils.ToLongPath(await asset.GetLocation(true, true));
            if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath)) return null;

            if (assetFile == null)
            {
                targetPath = GetMaterializedAssetPath(asset);
                if (!Directory.Exists(targetPath) || File.Exists(Path.Combine(targetPath, PARTIAL_INDICATOR))) await ExtractAsset(asset);
                if (!Directory.Exists(targetPath)) return null;
            }
            else
            {
                string sourcePath = Path.Combine(GetMaterializedAssetPath(asset), assetFile.GetSourcePath(true));
                if (!File.Exists(sourcePath))
                {
                    if (await ExtractAsset(asset, assetFile, fileOnly) == null)
                    {
                        Debug.LogError($"Archive could not be extracted: {asset}");
                        return null;
                    }
                }
                if (!File.Exists(sourcePath))
                {
                    // file is most likely not contained in package anymore
                    Debug.LogError($"File is not contained in this version of the package '{asset}' anymore. Reindexing might solve this.");

                    if (Config.removeUnresolveableDBFiles)
                    {
                        // remove from index
                        Debug.LogError($"Removing from index: {assetFile.FileName}");

                        DBAdapter.DB.Execute("delete from AssetFile where Id=?", assetFile.Id);
                        assetFile.Id = 0;
                    }
                    return null;
                }

                targetPath = Path.Combine(Path.GetDirectoryName(sourcePath), "Content", Path.GetFileName(assetFile.GetPath(true)));
                try
                {
                    if (!File.Exists(targetPath))
                    {
                        string directoryName = Path.GetDirectoryName(targetPath);
                        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
                        File.Copy(sourcePath, targetPath, true);
                    }

                    string sourceMetaPath = sourcePath + ".meta";
                    string targetMetaPath = targetPath + ".meta";
                    if (File.Exists(sourceMetaPath) && !File.Exists(targetMetaPath)) File.Copy(sourceMetaPath, targetMetaPath, true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not extract file. Most likely the target device ran out of space: {e.Message}");
                    return null;
                }
            }

            return targetPath;
        }

        public static async Task CalculateDependencies(AssetInfo info)
        {
            DependencyAnalysis da = new DependencyAnalysis();
            await da.Analyze(info);
        }

        public static List<AssetInfo> LoadAssets()
        {
            string indexedQuery = "SELECT *, Count(*) as FileCount, Sum(af.Size) as UncompressedSize from AssetFile af left join Asset on Asset.Id = af.AssetId group by af.AssetId order by Asset.SafeName COLLATE NOCASE";
            Dictionary<int, AssetInfo> indexedResult = DBAdapter.DB.Query<AssetInfo>(indexedQuery).ToDictionary(a => a.AssetId);

            string allQuery = "SELECT *, Id as AssetId from Asset order by SafeName COLLATE NOCASE";
            List<AssetInfo> result = DBAdapter.DB.Query<AssetInfo>(allQuery);

            // sqlite does not support "right join", therefore merge two queries manually 
            // TODO: it does in this newer version now, upgrade
            result.ForEach(asset =>
            {
                if (indexedResult.TryGetValue(asset.Id, out AssetInfo match))
                {
                    asset.FileCount = match.FileCount;
                    asset.UncompressedSize = match.UncompressedSize;
                }
            });

            InitAssets(result);

            return result;
        }

        internal static void InitAssets(List<AssetInfo> result)
        {
            ResolveParents(result, result);
            GetObserver().SetAll(result);
        }

        internal static void ResolveParents(List<AssetInfo> assets, List<AssetInfo> allAssets)
        {
            if (allAssets == null) return;

            Dictionary<int, AssetInfo> assetDict = allAssets.ToDictionary(a => a.AssetId);

            foreach (AssetInfo asset in assets)
            {
                // copy over additional metadata from allAssets (mostly file count which enables other features)
                if (asset.FileCount == 0 && assetDict.TryGetValue(asset.AssetId, out AssetInfo fullInfo))
                {
                    asset.FileCount = fullInfo.FileCount;
                    asset.UncompressedSize = fullInfo.UncompressedSize;
                }

                if (asset.ParentId > 0 && asset.ParentInfo == null)
                {
                    if (assetDict.TryGetValue(asset.ParentId, out AssetInfo parentInfo))
                    {
                        asset.ParentInfo = parentInfo;
                        if (asset.IsAsset()) parentInfo.ChildInfos.Add(asset);
                    }
                }
            }
        }

        internal static string[] ExtractAssetNames(IEnumerable<AssetInfo> assets, bool includeIdForDuplicates)
        {
            bool intoSubmenu = Config.groupLists && assets.Count(a => a.FileCount > 0) > MAX_DROPDOWN_ITEMS;
            List<string> result = new List<string> {"-all-"};
            List<AssetEntry> assetEntries = new List<AssetEntry>();

            foreach (AssetInfo asset in assets)
            {
                if (asset.FileCount > 0 && !asset.Exclude)
                {
                    // Use display name when IDs are included
                    string name = includeIdForDuplicates ? asset.GetDisplayName().Replace("/", " ") : asset.SafeName;

                    if (includeIdForDuplicates && asset.SafeName != Asset.NONE)
                    {
                        name = $"{name} [{asset.AssetId}]";
                    }

                    bool isSubPackage = asset.ParentId > 0;
                    string groupKey = intoSubmenu && !asset.SafeName.StartsWith("-")
                        ? name.Substring(0, 1).ToUpperInvariant()
                        : string.Empty;

                    assetEntries.Add(new AssetEntry
                    {
                        Name = name,
                        IsSubPackage = isSubPackage,
                        GroupKey = groupKey
                    });
                }
            }

            // Custom sorting
            assetEntries.Sort((a, b) =>
            {
                int cmp = a.IsSubPackage.CompareTo(b.IsSubPackage); // Non-sub-packages first
                if (cmp != 0) return cmp;

                cmp = string.Compare(a.GroupKey, b.GroupKey, StringComparison.OrdinalIgnoreCase);
                if (cmp != 0) return cmp;

                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            // Building the final list
            if (assetEntries.Count > 0)
            {
                int noneIdx = -1;
                result.Add(string.Empty);
                for (int i = 0; i < assetEntries.Count; i++)
                {
                    AssetEntry entry = assetEntries[i];

                    string displayName;
                    if (intoSubmenu)
                    {
                        if (entry.IsSubPackage)
                        {
                            // Sub-packages under "-Sub- / GroupKey / Name"
                            displayName = "-Sub-/" + entry.GroupKey + "/" + entry.Name;
                        }
                        else
                        {
                            // Non-sub-packages under "GroupKey / Name"
                            displayName = entry.GroupKey + "/" + entry.Name;
                        }
                    }
                    else
                    {
                        if (entry.IsSubPackage)
                        {
                            displayName = "-Sub- " + entry.Name;
                        }
                        else
                        {
                            displayName = entry.Name;
                        }
                    }

                    result.Add(displayName);
                    if (entry.Name == Asset.NONE) noneIdx = result.Count - 1;
                }

                if (noneIdx >= 0)
                {
                    result.RemoveAt(noneIdx);
                    result.Insert(1, Asset.NONE);
                }
            }

            return result.ToArray();
        }

        internal static string[] ExtractTagNames(List<Tag> tags)
        {
            bool intoSubmenu = Config.groupLists && tags.Count > MAX_DROPDOWN_ITEMS;
            List<string> result = new List<string> {"-all-", "-none-", string.Empty};
            result.AddRange(tags
                .Select(a =>
                    intoSubmenu && !a.Name.StartsWith("-")
                        ? a.Name.Substring(0, 1).ToUpperInvariant() + "/" + a.Name
                        : a.Name)
                .OrderBy(s => s));

            if (result.Count == 2) result.RemoveAt(1);

            return result.ToArray();
        }

        internal static string[] ExtractPublisherNames(IEnumerable<AssetInfo> assets)
        {
            bool intoSubmenu =
                Config.groupLists &&
                assets.Count(a => a.FileCount > 0) >
                MAX_DROPDOWN_ITEMS; // approximation, publishers != assets but roughly the same
            List<string> result = new List<string> {"-all-", string.Empty};
            result.AddRange(assets
                .Where(a => a.FileCount > 0)
                .Where(a => !a.Exclude)
                .Where(a => !string.IsNullOrEmpty(a.SafePublisher))
                .Select(a =>
                    intoSubmenu
                        ? a.SafePublisher.Substring(0, 1).ToUpperInvariant() + "/" + a.SafePublisher
                        : a.SafePublisher)
                .Distinct()
                .OrderBy(s => s));

            if (result.Count == 2) result.RemoveAt(1);

            return result.ToArray();
        }

        internal static string[] ExtractCategoryNames(IEnumerable<AssetInfo> assets)
        {
            bool intoSubmenu = Config.groupLists;
            List<string> result = new List<string> {"-all-", string.Empty};
            result.AddRange(assets
                .Where(a => a.FileCount > 0)
                .Where(a => !a.Exclude)
                .Where(a => !string.IsNullOrEmpty(a.SafeCategory))
                .Select(a =>
                {
                    if (intoSubmenu)
                    {
                        string[] arr = a.GetDisplayCategory().Split('/');
                        return arr[0] + "/" + a.SafeCategory;
                    }

                    return a.SafeCategory;
                })
                .Distinct()
                .OrderBy(s => s));

            if (result.Count == 2) result.RemoveAt(1);

            return result.ToArray();
        }

        internal static string[] LoadTypes()
        {
            List<string> result = new List<string> {"-all-", string.Empty};

            string query = "SELECT Distinct(Type) from AssetFile where Type not null and Type != \"\" order by Type";
            List<string> raw = DBAdapter.DB.QueryScalars<string>($"{query}");

            List<string> groupTypes = new List<string>();
            foreach (KeyValuePair<string, string[]> group in TypeGroups)
            {
                groupTypes.AddRange(group.Value);
                foreach (string type in group.Value)
                {
                    if (raw.Contains(type))
                    {
                        result.Add($"{group.Key}");
                        break;
                    }
                }
            }

            if (Config.showExtensionsList)
            {
                if (result.Last() != "") result.Add(string.Empty);

                // others
                result.AddRange(raw.Where(r => !groupTypes.Contains(r)).Select(type => $"Others/{type}"));

                // all
                result.AddRange(raw.Select(type => $"All/{type}"));
            }

            if (result.Count == 2) result.RemoveAt(1);

            return result.ToArray();
        }

        public static async Task<long> GetCacheFolderSize()
        {
            return await IOUtils.GetFolderSize(GetMaterializeFolder());
        }

        public static async Task<long> GetPersistedCacheSize()
        {
            if (!Directory.Exists(GetMaterializeFolder())) return 0;

            long result = 0;

            List<Asset> keepAssets = DBAdapter.DB.Table<Asset>().Where(a => a.KeepExtracted).ToList();
            List<string> keepPaths = keepAssets.Select(a => GetMaterializedAssetPath(a).ToLowerInvariant()).ToList();
            string[] packages = Directory.GetDirectories(GetMaterializeFolder());
            foreach (string package in packages)
            {
                if (!keepPaths.Contains(package.ToLowerInvariant())) continue;
                result += await IOUtils.GetFolderSize(package);
            }

            return result;
        }

        public static async Task<long> GetBackupFolderSize()
        {
            return await IOUtils.GetFolderSize(GetBackupFolder());
        }

        public static async Task<long> GetPreviewFolderSize()
        {
            return await IOUtils.GetFolderSize(GetPreviewFolder());
        }

        public static async void RefreshIndex(bool force = false)
        {
            IndexingInProgress = true;
            AssetProgress.CancellationRequested = false;

            Init();

            // refresh registry packages in parallel
            AssetStore.GatherAllMetadata();

            // pass 1: metadata
            // special handling for normal asset store assets since directory structure yields additional information
            if (Config.indexAssetCache)
            {
                string assetDownloadCache = GetAssetCacheFolder();
                if (Directory.Exists(assetDownloadCache))
                {
                    // check if forced local update is requested after upgrading
                    AppProperty forceLocalUpdate = DBAdapter.DB.Find<AppProperty>("ForceLocalUpdate");
                    if (forceLocalUpdate != null && forceLocalUpdate.Value.ToLowerInvariant() == "true")
                    {
                        force = true;
                        DBAdapter.DB.Delete<AppProperty>("ForceLocalUpdate");
                    }

                    await new UnityPackageImporter().IndexRoughLocal(new FolderSpec(assetDownloadCache), true, force);
                }
                else
                {
                    Debug.LogWarning($"Could not find the asset download folder: {assetDownloadCache}");
                    EditorUtility.DisplayDialog("Error",
                        $"Could not find the asset download folder: {assetDownloadCache}.\n\nEither nothing was downloaded yet through the Package Manager or you changed the Asset cache location. In the latter case, please configure the new location under Settings.",
                        "OK");
                }
            }

            if (Config.indexPackageCache)
            {
                string packageDownloadCache = GetPackageCacheFolder();
                if (Directory.Exists(packageDownloadCache))
                {
                    await new PackageImporter().IndexRough(packageDownloadCache, true);
                }
                else
                {
                    Debug.LogWarning($"Could not find the package download folder: {packageDownloadCache}");
                    EditorUtility.DisplayDialog("Error",
                        $"Could not find the package download folder: {packageDownloadCache}.\n\nEither nothing was downloaded yet through the Package Manager or you changed the Package cache location. In the latter case, please configure the new location under Settings.",
                        "OK");
                }
            }

            // pass 2: details
            if (Config.indexAssetCache && Config.indexAssetPackageContents) await new UnityPackageImporter().IndexDetails();
            if (Config.indexPackageCache) await new PackageImporter().IndexDetails();

            // scan custom folders
            if (Config.indexAdditionalFolders)
            {
                for (int i = 0; i < Config.folders.Count; i++)
                {
                    if (AssetProgress.CancellationRequested) break;

                    FolderSpec spec = Config.folders[i];
                    if (!spec.enabled) continue;
                    if (!Directory.Exists(spec.GetLocation(true)))
                    {
                        Debug.LogWarning($"Specified folder to scan for assets does not exist anymore: {spec.location}");
                        continue;
                    }

                    switch (spec.folderType)
                    {
                        case 0:
                            bool hasAssetStoreLayout = Path.GetFileName(spec.GetLocation(true)) == ASSET_STORE_FOLDER_NAME;
                            await new UnityPackageImporter().IndexRoughLocal(spec, hasAssetStoreLayout, force);

                            if (Config.indexAssetPackageContents) await new UnityPackageImporter().IndexDetails();
                            break;

                        case 1:
                            await new MediaImporter().Index(spec);
                            break;

                        case 2:
                            await new ArchiveImporter().Index(spec);
                            break;

                        case 3:
                            await new DevPackageImporter().Index(spec);
                            break;

                        default:
                            Debug.LogError($"Unsupported folder scan type: {spec.folderType}");
                            break;
                    }
                }
            }

            // pass 3: online index
            if (Config.indexAssetCache && Config.downloadAssets)
            {
                List<AssetInfo> assets = LoadAssets()
                    .Where(info =>
                        info.AssetSource == Asset.Source.AssetStorePackage &&
                        !info.Exclude &&
                        info.ParentId <= 0 &&
                        !info.IsAbandoned && (!info.IsIndexed || info.CurrentState == Asset.State.SubInProcess) && !string.IsNullOrEmpty(info.OfficialState)
                        && !info.IsDownloaded)
                    .ToList();

                // needs to be started as coroutine due to download triggering which cannot happen outside main thread 
                bool done = false;
                EditorCoroutineUtility.StartCoroutineOwnerless(new UnityPackageImporter().IndexRoughOnline(assets, () => done = true));
                do
                {
                    await Task.Delay(100);
                } while (!done);
            }

            // pass 4: Unity Asset Manager
            if (Config.indexAssetManager)
            {
                await new AssetManagerImporter().Index();
            }

            // pass 5: index colors
            if (Config.extractColors)
            {
                await new ColorImporter().Index();
            }

            // pass 6: AI captions
            if (Config.createAICaptions)
            {
                await new CaptionCreator().Index();
            }

            // pass 7: backup
            if (Config.createBackups)
            {
                AssetBackup backup = new AssetBackup();
                await backup.Sync();
            }

            // final pass: start over once if that was the very first time indexing since after all updates are pulled the indexing might crunch additional data
            AppProperty initialIndexingDone = DBAdapter.DB.Find<AppProperty>("InitialIndexingDone");
            if (!AssetProgress.CancellationRequested && (initialIndexingDone == null || initialIndexingDone.Value.ToLowerInvariant() != "true"))
            {
                DBAdapter.DB.InsertOrReplace(new AppProperty("InitialIndexingDone", "true"));
                RefreshIndex(true);
                return;
            }

            LastIndexUpdate = DateTime.Now;
            AppProperty lastUpdate = new AppProperty("LastIndexUpdate", LastIndexUpdate.ToString(CultureInfo.InvariantCulture));
            DBAdapter.DB.InsertOrReplace(lastUpdate);

            IndexingInProgress = false;
            OnIndexingDone?.Invoke();
        }

        public static async void RefreshIndex(AssetInfo info)
        {
            IndexingInProgress = true;
            AssetProgress.CancellationRequested = false;

            switch (info.AssetSource)
            {
                case Asset.Source.AssetStorePackage:
                case Asset.Source.CustomPackage:
                    await new UnityPackageImporter().IndexDetails(info.Id);
                    break;

                case Asset.Source.RegistryPackage:
                    await new PackageImporter().IndexDetails(info.Id);
                    break;

                case Asset.Source.Archive:
                    await new ArchiveImporter().IndexDetails(info.ToAsset());
                    break;

                case Asset.Source.AssetManager:
                    await new AssetManagerImporter().Index(info.ToAsset());
                    break;

                case Asset.Source.Directory:
                    FolderSpec spec = Config.folders.FirstOrDefault(f => f.location == info.Location && f.folderType == info.GetFolderSpecType());
                    if (spec != null) await new MediaImporter().Index(spec);
                    break;

                default:
                    Debug.LogError($"Unsupported asset source of '{info.GetDisplayName()}' for index refresh: {info.AssetSource}");
                    break;
            }

            IndexingInProgress = false;
            OnIndexingDone?.Invoke();
        }

        internal static async Task ProcessSubPackages(Asset asset, List<AssetFile> subPackages)
        {
            List<AssetFile> unityPackages = subPackages.Where(p => p.IsPackage()).ToList();
            List<AssetFile> archives = subPackages.Where(p => p.IsArchive()).ToList();

            if (unityPackages.Count > 0)
            {
                await UnityPackageImporter.ProcessSubPackages(asset, unityPackages);
            }

            if (archives.Count > 0)
            {
                await ArchiveImporter.ProcessSubArchives(asset, archives);
            }
        }

        public static string GetAssetCacheFolder()
        {
            if (_assetCacheFolder.TryGetValue(out string path)) return path;

            string result;

            // explicit custom configuration always wins
            if (Config.assetCacheLocationType == 1 && !string.IsNullOrWhiteSpace(Config.assetCacheLocation))
            {
                result = Config.assetCacheLocation;
            }
            // then try what Unity is reporting itself
            else if (!string.IsNullOrWhiteSpace(AssetStore.GetAssetCacheFolder()))
            {
                result = AssetStore.GetAssetCacheFolder();
            }
            else
            {
                // environment variable overrides default location
                string envPath = StringUtils.GetEnvVar("ASSETSTORE_CACHE_PATH");
                if (!string.IsNullOrWhiteSpace(envPath))
                {
                    result = envPath;
                }
                else
                {
                    // custom special location (Unity 2022+) overrides default as well, kept in for legacy compatibility
                    string customLocation = Config.folders.FirstOrDefault(f => f.GetLocation(true).EndsWith(ASSET_STORE_FOLDER_NAME))?.GetLocation(true);
                    if (!string.IsNullOrWhiteSpace(customLocation))
                    {
                        result = customLocation;
                    }
                    else
                    {
#if UNITY_EDITOR_WIN
                        result = IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity", ASSET_STORE_FOLDER_NAME);
#endif
#if UNITY_EDITOR_OSX
                        result = IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Unity", ASSET_STORE_FOLDER_NAME);
#endif
#if UNITY_EDITOR_LINUX
                        result = IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local/share/unity3d", ASSET_STORE_FOLDER_NAME);
#endif
                    }
                }
            }
            if (result != null) result = result.Replace("\\", "/");

            _assetCacheFolder.SetValue(result, TimeSpan.FromSeconds(FOLDER_CACHE_TIME));
            return result;
        }

        public static string GetPackageCacheFolder()
        {
            string result;
            if (Config.packageCacheLocationType == 1 && !string.IsNullOrWhiteSpace(Config.packageCacheLocation))
            {
                result = Config.packageCacheLocation;
            }
            else
            {
#if UNITY_EDITOR_WIN
                result = IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Unity", "cache", "packages");
#endif
#if UNITY_EDITOR_OSX
                result = IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Unity", "cache", "packages");
#endif
#if UNITY_EDITOR_LINUX
                result = IOUtils.PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config/unity3d/cache/packages");
#endif
            }
            if (result != null) result = result.Replace("\\", "/");

            return result;
        }

        public static async void ClearCache(Action callback = null)
        {
            ClearCacheInProgress = true;
            try
            {
                string cachePath = GetMaterializeFolder();
                if (Directory.Exists(cachePath))
                {
                    List<Asset> keepAssets = DBAdapter.DB.Table<Asset>().Where(a => a.KeepExtracted).ToList();
                    List<string> keepPaths = keepAssets.Select(a => GetMaterializedAssetPath(a).ToLowerInvariant()).ToList();

                    // go through 1 by 1 to keep persisted packages in the cache
                    string[] packages = Directory.GetDirectories(cachePath);
                    foreach (string package in packages)
                    {
                        if (keepPaths.Contains(package.ToLowerInvariant())) continue;
                        await IOUtils.DeleteFileOrDirectory(package);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not delete full cache directory: {e.Message}");
            }

            ClearCacheInProgress = false;
            callback?.Invoke();
        }

        private static void LoadConfig()
        {
            string configLocation = GetConfigLocation();
            UsedConfigLocation = configLocation;

            if (configLocation == null || !File.Exists(configLocation))
            {
                _config = new AssetInventorySettings();
                return;
            }

            ConfigErrors.Clear();
            _config = JsonConvert.DeserializeObject<AssetInventorySettings>(File.ReadAllText(configLocation), new JsonSerializerSettings
            {
                Error = delegate(object _, ErrorEventArgs args)
                {
                    ConfigErrors.Add(args.ErrorContext.Error.Message);

                    Debug.LogError("Invalid config file format: " + args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            });
            if (_config == null) _config = new AssetInventorySettings();
            if (_config.folders == null) _config.folders = new List<FolderSpec>();

            // ensure all paths are in the correct format
            _config.folders.ForEach(f => f.location = f.location?.Replace("\\", "/"));
        }

        public static void SaveConfig()
        {
            string configFile = GetConfigLocation();
            if (configFile == null) return;

            if (_config.reportingBatchSize > 500) _config.reportingBatchSize = 500; // SQLite cannot handle more than that

            try
            {
                File.WriteAllText(configFile, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not persist configuration. It might be locked by another application: {e.Message}");
            }
        }

        public static void ResetConfig()
        {
            DBAdapter.Close(); // in case DB path changes

            _config = new AssetInventorySettings();
            SaveConfig();
            AssetDatabase.Refresh();
        }

        public static void ResetUICustomization()
        {
            _config.ResetAdvancedUI();
            SaveConfig();
        }

        public static async Task<AssetPurchases> FetchOnlineAssets()
        {
            AssetStore.CancellationRequested = false;
            AssetPurchases assets = await AssetStore.RetrievePurchases();
            if (assets == null) return null; // happens if token was invalid 

            CurrentMain = "Phase 2/3: Updating purchases";
            MainCount = assets.results.Count;
            MainProgress = 1;
            int progressId = MetaProgress.Start("Updating purchases");

            // store for later troubleshooting
            File.WriteAllText(Path.Combine(GetStorageFolder(), DIAG_PURCHASES), JsonConvert.SerializeObject(assets, Formatting.Indented));

            bool tagsChanged = false;
            try
            {
                for (int i = 0; i < MainCount; i++)
                {
                    MainProgress = i + 1;
                    MetaProgress.Report(progressId, i + 1, MainCount, string.Empty);
                    if (i % 50 == 0) await Task.Yield(); // let editor breath
                    if (AssetStore.CancellationRequested) break;

                    AssetPurchase purchase = assets.results[i];

                    // update all known assets with that foreignId to support updating duplicate assets as well 
                    List<Asset> existingAssets = DBAdapter.DB.Table<Asset>().Where(a => a.ForeignId == purchase.packageId).ToList();
                    if (existingAssets.Count == 0 || existingAssets.Count(a => a.AssetSource == Asset.Source.AssetStorePackage || (a.AssetSource == Asset.Source.RegistryPackage && a.ForeignId > 0)) == 0)
                    {
                        // create new asset on-demand or if only available as custom asset so far
                        Asset asset = purchase.ToAsset();
                        asset.SafeName = purchase.CalculatedSafeName;
                        if (Config.excludeByDefault) asset.Exclude = true;
                        if (Config.extractByDefault) asset.KeepExtracted = true;
                        if (Config.backupByDefault) asset.Backup = true;
                        AssetImporter.Persist(asset);
                        existingAssets.Add(asset);
                    }

                    for (int i2 = 0; i2 < existingAssets.Count; i2++)
                    {
                        Asset asset = existingAssets[i2];

                        // temporarily store guessed safe name to ensure locally indexed files are mapped correctly
                        // will be overridden in detail run
                        asset.DisplayName = purchase.displayName.Trim();
                        asset.ForeignId = purchase.packageId;
                        if (!string.IsNullOrEmpty(purchase.grantTime))
                        {
                            if (DateTime.TryParse(purchase.grantTime, out DateTime result))
                            {
                                asset.PurchaseDate = result;
                            }
                        }
                        if (purchase.isHidden && Config.excludeHidden) asset.Exclude = true;

                        if (string.IsNullOrEmpty(asset.SafeName)) asset.SafeName = purchase.CalculatedSafeName;

                        // override data with local truth in case header information exists
                        if (File.Exists(asset.GetLocation(true)))
                        {
                            AssetHeader header = UnityPackageImporter.ReadHeader(asset.GetLocation(true), true);
                            UnityPackageImporter.ApplyHeader(header, asset);
                        }

                        AssetImporter.Persist(asset);

                        // handle tags
                        if (purchase.tagging != null)
                        {
                            foreach (string tag in purchase.tagging)
                            {
                                if (Tagging.AddTagAssignment(asset.Id, tag, TagAssignment.Target.Package, true)) tagsChanged = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not update purchases: {e.Message}");
            }

            if (tagsChanged)
            {
                Tagging.LoadTags();
                Tagging.LoadTagAssignments();
            }

            CurrentMain = null;
            MetaProgress.Remove(progressId);

            return assets;
        }

        public static async Task FetchAssetsDetails(bool forceUpdate = false, int assetId = 0, bool skipProgress = false, bool skipEvents = false)
        {
            if (forceUpdate)
            {
                DBAdapter.DB.Execute("update Asset set ETag=null, LastOnlineRefresh=0" + (assetId > 0 ? " where Id=" + assetId : string.Empty));
            }

            IEnumerable<Asset> dbAssets = DBAdapter.DB.Table<Asset>()
                .Where(a => a.ForeignId > 0)
                .ToList();

            if (assetId > 0)
            {
                dbAssets = dbAssets.Where(a => a.Id == assetId);
            }
            else
            {
                dbAssets = dbAssets.Where(a => (DateTime.Now - a.LastOnlineRefresh).TotalDays >= Config.assetStoreRefreshCycle);
            }
            List<Asset> assets = dbAssets.OrderBy(a => a.LastOnlineRefresh).ToList();

            if (!skipProgress)
            {
                CurrentMain = "Phase 3/3: Updating package details";
                MainCount = assets.Count;
                MainProgress = 1;
            }
            int progressId = MetaProgress.Start("Updating package details");
            string previewFolder = GetPreviewFolder();

            SemaphoreSlim semaphore = new SemaphoreSlim(Config.maxConcurrentUnityRequests);
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];
                int id = asset.ForeignId;

                if (!skipProgress) MainProgress = i + 1;
                MetaProgress.Report(progressId, i + 1, MainCount, string.Empty);
                if (i % 5 == 0) await Task.Yield(); // let editor breathe
                if (!skipProgress && AssetStore.CancellationRequested) break;

                await semaphore.WaitAsync();

                async Task ProcessAsset(Asset currentAsset, int curAssetId)
                {
                    try
                    {
                        AssetDetails details = await AssetStore.RetrieveAssetDetails(curAssetId, currentAsset.ETag);
                        currentAsset = DBAdapter.DB.Find<Asset>(a => a.Id == currentAsset.Id); // reload in case it was changed in the meantime
                        if (details == null) // happens if unchanged through etag
                        {
                            currentAsset.LastOnlineRefresh = DateTime.Now;
                            DBAdapter.DB.Update(currentAsset);
                            return;
                        }
                        if (!string.IsNullOrEmpty(details.packageName) && currentAsset.AssetSource != Asset.Source.RegistryPackage)
                        {
                            // special case of registry packages listed on asset store
                            // registry package could already exist so make sure to only have one entry
                            Asset existing = DBAdapter.DB.Find<Asset>(a => a.SafeName == details.packageName && a.AssetSource == Asset.Source.RegistryPackage);
                            if (existing != null)
                            {
                                DBAdapter.DB.Delete(currentAsset);
                                assets[i] = existing;
                                currentAsset = existing;
                            }
                            currentAsset.AssetSource = Asset.Source.RegistryPackage;
                            currentAsset.SafeName = details.packageName;
                            currentAsset.ForeignId = curAssetId;
                        }

                        // check if disabled, then download links are not available anymore, deprecated would still work
                        DownloadInfo downloadDetails = null;
                        if (currentAsset.AssetSource == Asset.Source.AssetStorePackage && details.state != "disabled")
                        {
                            downloadDetails = await AssetStore.RetrieveAssetDownloadInfo(curAssetId, code =>
                            {
                                // if unauthorized then seat was removed again for that user, mark asset as custom
                                if (code == 403)
                                {
                                    currentAsset.AssetSource = Asset.Source.CustomPackage;
                                    DBAdapter.DB.Execute("UPDATE Asset set AssetSource=? where Id=?", Asset.Source.CustomPackage, currentAsset.Id);

                                    Debug.Log($"No more access to {currentAsset}. Seat was probably removed. Switching asset source to custom and disabling download possibility.");
                                }
                            });
                            if (currentAsset.AssetSource == Asset.Source.AssetStorePackage && (downloadDetails == null || string.IsNullOrEmpty(downloadDetails.filename_safe_package_name)))
                            {
                                Debug.Log($"Could not fetch download detail information for '{currentAsset.SafeName}'");
                            }
                            else if (downloadDetails != null)
                            {
                                currentAsset.UploadId = downloadDetails.upload_id;
                                currentAsset.SafeName = downloadDetails.filename_safe_package_name;
                                currentAsset.SafeCategory = downloadDetails.filename_safe_category_name;
                                currentAsset.SafePublisher = downloadDetails.filename_safe_publisher_name;
                                currentAsset.OriginalLocation = downloadDetails.url;
                                currentAsset.OriginalLocationKey = downloadDetails.key;
                                if (currentAsset.AssetSource == Asset.Source.AssetStorePackage && !string.IsNullOrEmpty(currentAsset.GetLocation(true)) && currentAsset.GetCalculatedLocation().ToLower() != currentAsset.GetLocation(true).ToLower())
                                {
                                    currentAsset.CurrentSubState = Asset.SubState.Outdated;
                                }
                                else
                                {
                                    currentAsset.CurrentSubState = Asset.SubState.None;
                                }
                            }
                        }

                        currentAsset.LastOnlineRefresh = DateTime.Now;
                        currentAsset.OfficialState = details.state;
                        currentAsset.ETag = details.ETag;
                        currentAsset.DisplayName = details.name;
                        currentAsset.DisplayPublisher = details.productPublisher?.name;
                        currentAsset.DisplayCategory = details.category?.name;
                        if (details.properties != null && details.properties.ContainsKey("firstPublishedDate") && DateTime.TryParse(details.properties["firstPublishedDate"], out DateTime firstPublishedDate))
                        {
                            currentAsset.FirstRelease = firstPublishedDate;
                        }
                        if (int.TryParse(details.publisherId, out int publisherId)) currentAsset.PublisherId = publisherId;

                        // prices
                        if (details.productRatings != null)
                        {
                            NumberStyles style = NumberStyles.Number;
                            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

                            AssetPrice eurPrice = details.productRatings.FirstOrDefault(r => r.currency.ToLowerInvariant() == "eur");
                            if (eurPrice != null && float.TryParse(eurPrice.finalPrice, style, culture, out float eur)) currentAsset.PriceEur = eur;
                            AssetPrice usdPrice = details.productRatings.FirstOrDefault(r => r.currency.ToLowerInvariant() == "usd");
                            if (usdPrice != null && float.TryParse(usdPrice.finalPrice, style, culture, out float usd)) currentAsset.PriceUsd = usd;
                            AssetPrice yenPrice = details.productRatings.FirstOrDefault(r => r.currency.ToLowerInvariant() == "cny");
                            if (yenPrice != null && float.TryParse(yenPrice.finalPrice, style, culture, out float yen)) currentAsset.PriceCny = yen;
                        }

                        if (string.IsNullOrEmpty(currentAsset.SafeName)) currentAsset.SafeName = AssetUtils.GuessSafeName(details.name);
                        currentAsset.Description = details.description;
                        currentAsset.Requirements = string.Join(", ", details.requirements);
                        currentAsset.Keywords = string.Join(", ", details.keyWords);
                        currentAsset.SupportedUnityVersions = string.Join(", ", details.supportedUnityVersions);
                        currentAsset.Revision = details.revision;
                        currentAsset.Slug = details.slug;
                        currentAsset.LatestVersion = details.version.name;
                        currentAsset.LastRelease = details.version.publishedDate;
                        if (currentAsset.LastRelease == DateTime.MinValue) currentAsset.LastRelease = details.updatedTime; // can happen for deprecated assets, their version published date will be 0
                        if (details.productReview != null)
                        {
                            currentAsset.AssetRating = details.productReview.ratingAverage;
                            currentAsset.RatingCount = int.Parse(details.productReview.ratingCount);
                            if (float.TryParse(details.productReview.hotness, NumberStyles.Float, CultureInfo.InvariantCulture, out float hotness)) currentAsset.Hotness = hotness;
                        }

                        currentAsset.CompatibilityInfo = details.compatibilityInfo;
                        currentAsset.ReleaseNotes = details.publishNotes;
                        currentAsset.KeyFeatures = details.keyFeatures;
                        if (details.uploads != null)
                        {
                            // use size of download for latest Unity version, usually good enough approximation
                            KeyValuePair<string, UploadInfo> upload = details.uploads
                                .OrderBy(pair => new SemVer(pair.Key))
                                .LastOrDefault();
                            if (upload.Value != null)
                            {
                                if (currentAsset.PackageSize == 0 && long.TryParse(upload.Value.downloadSize, out long size))
                                {
                                    currentAsset.PackageSize = size;
                                }

                                // store SRP info
                                if (upload.Value.srps != null)
                                {
                                    currentAsset.BIRPCompatible = upload.Value.srps.Contains("standard");
                                    currentAsset.URPCompatible = upload.Value.srps.Contains("lightweight");
                                    currentAsset.HDRPCompatible = upload.Value.srps.Contains("hd");
                                }

                                // parse and prepare dependencies
                                if (upload.Value.dependencies != null && upload.Value.dependencies.Length > 0)
                                {
                                    List<Dependency> deps = new List<Dependency>();
                                    foreach (string link in upload.Value.dependencies)
                                    {
                                        Dependency dep = new Dependency();
                                        dep.location = link;

                                        // try to resolve more information about the dependency
                                        string[] arr = dep.location.Split('-');
                                        if (int.TryParse(arr[arr.Length - 1], out dep.id))
                                        {
                                            AssetDetails depDetails = await AssetStore.RetrieveAssetDetails(dep.id);
                                            if (depDetails != null)
                                            {
                                                dep.name = depDetails.name;
                                            }
                                        }

                                        deps.Add(dep);
                                    }
                                    currentAsset.PackageDependencies = JsonConvert.SerializeObject(deps);
                                }
                                else
                                {
                                    currentAsset.PackageDependencies = null;
                                }
                            }
                        }

                        // linked but not-purchased packages should not contain null for safe_names for search filters to work
                        if (downloadDetails == null && currentAsset.AssetSource == Asset.Source.CustomPackage)
                        {
                            // safe entries must not contain forward slashes due to sub-menu construction
                            if (string.IsNullOrWhiteSpace(currentAsset.SafePublisher)) currentAsset.SafePublisher = AssetUtils.GuessSafeName(currentAsset.DisplayPublisher.Replace("/", " "));
                            if (string.IsNullOrWhiteSpace(currentAsset.SafeCategory)) currentAsset.SafeCategory = AssetUtils.GuessSafeName(currentAsset.DisplayCategory.Replace("/", " "));
                        }

                        // override data with local truth in case header information exists
                        if (File.Exists(currentAsset.GetLocation(true)))
                        {
                            AssetHeader header = UnityPackageImporter.ReadHeader(currentAsset.GetLocation(true), true);
                            UnityPackageImporter.ApplyHeader(header, currentAsset);
                        }

                        DBAdapter.DB.Update(currentAsset);
                        PersistMedia(currentAsset, details);

                        // load package icon on demand
                        string icon = details.mainImage?.icon;
                        if (!string.IsNullOrWhiteSpace(icon) && string.IsNullOrWhiteSpace(currentAsset.GetPreviewFile(previewFolder)))
                        {
                            _ = AssetUtils.LoadImageAsync(icon, currentAsset.GetPreviewFile(previewFolder, false)).ContinueWith(task =>
                            {
                                if (task.Exception != null)
                                {
                                    Debug.LogError($"Failed to download image from {icon}: {task.Exception.Message}");
                                }
                                else
                                {
                                    OnPackageImageLoaded?.Invoke(currentAsset);
                                }
                            }, TaskScheduler.FromCurrentSynchronizationContext());
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error fetching asset details for '{currentAsset}': {e.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }

                tasks.Add(ProcessAsset(asset, id));
            }

            // Await all tasks to complete
            await Task.WhenAll(tasks);

            if (!skipProgress) CurrentMain = null;
            MetaProgress.Remove(progressId);
            if (!skipEvents) OnPackagesUpdated?.Invoke();
        }

        private static void PersistMedia(Asset asset, AssetDetails details)
        {
            List<AssetMedia> existing = DBAdapter.DB.Query<AssetMedia>("select * from AssetMedia where AssetId=?", asset.Id).ToList();

            // handle main image
            if (!string.IsNullOrWhiteSpace(details.mainImage?.url)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "main", Url = details.mainImage.url});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.icon)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "icon", Url = details.mainImage.icon});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.icon25)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "icon25", Url = details.mainImage.icon25});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.icon75)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "icon75", Url = details.mainImage.icon75});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.small)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "small", Url = details.mainImage.small});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.small_v2)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "small_v2", Url = details.mainImage.small_v2});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.big)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "big", Url = details.mainImage.big});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.big_v2)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "big_v2", Url = details.mainImage.big_v2});
            if (!string.IsNullOrWhiteSpace(details.mainImage?.facebook)) StoreMedia(existing, new AssetMedia {AssetId = asset.Id, Type = "facebook", Url = details.mainImage.facebook});

            // handle screenshots & videos
            for (int i = 0; i < details.images.Length; i++)
            {
                AssetImage img = details.images[i];
                StoreMedia(existing, new AssetMedia {Order = i, AssetId = asset.Id, Type = img.type, Url = img.imageUrl, ThumbnailUrl = img.thumbnailUrl, Width = img.width, Height = img.height, WebpUrl = img.webpUrl});
            }

            // TODO: remove outdated
        }

        private static void StoreMedia(List<AssetMedia> existing, AssetMedia media)
        {
            AssetMedia match = existing.FirstOrDefault(m => m.Type == media.Type && m.Url == media.Url);
            if (match == null)
            {
                DBAdapter.DB.Insert(media);
                existing.Add(media);
            }
            else
            {
                media.Id = match.Id;
                DBAdapter.DB.Update(media);
            }
        }

        internal static void LoadMedia(AssetInfo info)
        {
            // when already downloading don't trigger again
            if (info.IsMediaLoading()) return;

            info.DisposeMedia();
            if (info.ParentInfo != null)
            {
                LoadMedia(info.ParentInfo);
                info.AllMedia = info.ParentInfo.AllMedia;
                info.Media = info.ParentInfo.Media;
                return;
            }

            info.AllMedia = DBAdapter.DB.Query<AssetMedia>("select * from AssetMedia where AssetId=? order by [Order]", info.AssetId).ToList();
            info.Media = info.AllMedia.Where(m => m.Type == "main" || m.Type == "screenshot" || m.Type == "youtube").ToList();
            DownloadMedia(info);
        }

        private static async void DownloadMedia(AssetInfo info)
        {
            List<AssetMedia> files = info.Media.Where(m => !m.IsDownloading).OrderBy(m => m.Order).ToList();
            for (int i = 0; i < files.Count; i++)
            {
                if (info.Media == null) return; // happens when cancelled
                if (info.Media[i].IsDownloading) continue;

                // thumbnail
                if (!string.IsNullOrWhiteSpace(files[i].ThumbnailUrl))
                {
                    string thumbnailFile = info.ToAsset().GetMediaThumbnailFile(files[i], GetPreviewFolder(), false);
                    if (!File.Exists(thumbnailFile))
                    {
                        if (info.Media == null) return; // happens when cancelled
                        info.Media[i].IsDownloading = true;
                        await AssetUtils.LoadImageAsync(files[i].ThumbnailUrl, thumbnailFile);
                        if (info.Media == null) return; // happens when cancelled
                        info.Media[i].IsDownloading = false;
                    }
                    if (info.Media != null && !info.Media[i].IsDownloading && File.Exists(thumbnailFile))
                    {
                        files[i].ThumbnailTexture = await AssetUtils.LoadLocalTexture(thumbnailFile, false); //, Config.mediaThumbnailWidth, true);
                    }
                }

                // full
                if (files[i].Type != "youtube" && !string.IsNullOrWhiteSpace(files[i].Url))
                {
                    string targetFile = info.ToAsset().GetMediaFile(files[i], GetPreviewFolder(), false);
                    if (!File.Exists(targetFile))
                    {
                        if (info.Media == null) return; // happens when cancelled
                        info.Media[i].IsDownloading = true;
                        await AssetUtils.LoadImageAsync(files[i].Url, targetFile);
                        if (info.Media == null) return; // happens when cancelled
                        info.Media[i].IsDownloading = false;
                    }
                    if (info.Media != null && !info.Media[i].IsDownloading && File.Exists(targetFile))
                    {
                        files[i].Texture = await AssetUtils.LoadLocalTexture(targetFile, false); //, Mathf.RoundToInt(Config.mediaHeight * 1.5f), true);
                    }
                }
            }
        }

        public static int CountPurchasedAssets(IEnumerable<AssetInfo> assets)
        {
            return assets.Count(a => a.ParentId == 0 && (a.AssetSource == Asset.Source.AssetStorePackage || (a.AssetSource == Asset.Source.RegistryPackage && a.ForeignId > 0)));
        }

        public static void MoveDatabase(string targetFolder)
        {
            string targetDBFile = Path.Combine(targetFolder, Path.GetFileName(DBAdapter.GetDBPath()));
            if (File.Exists(targetDBFile)) File.Delete(targetDBFile);
            string oldStorageFolder = GetStorageFolder();
            DBAdapter.Close();

            bool success = false;
            try
            {
                // for safety copy first, then delete old state after everything is done
                EditorUtility.DisplayProgressBar("Moving Database", "Copying database to new location...", 0.2f);
                File.Copy(DBAdapter.GetDBPath(), targetDBFile);
                EditorUtility.ClearProgressBar();

                EditorUtility.DisplayProgressBar("Moving Preview Images", "Copying preview images to new location...", 0.4f);
                IOUtils.CopyDirectory(GetPreviewFolder(), GetPreviewFolder(targetFolder, true));
                EditorUtility.ClearProgressBar();

                // set new location
                SwitchDatabase(targetFolder);
                success = true;
            }
            catch
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error Moving Data",
                    "There were errors moving the existing database to a new location. Check the error log for details. Current database location remains unchanged.",
                    "OK");
            }

            if (success)
            {
                EditorUtility.DisplayProgressBar("Freeing Up Space", "Removing backup files from old location...", 0.8f);
                Directory.Delete(oldStorageFolder, true);
                EditorUtility.ClearProgressBar();
            }
        }

        public static void SwitchDatabase(string targetFolder)
        {
            DBAdapter.Close();
            AssetUtils.ClearCache();
            Config.customStorageLocation = targetFolder;
            SaveConfig();

            InitDone = false;
            Init();
        }

        public static void ForgetAssetFile(AssetFile info)
        {
            DBAdapter.DB.Execute("DELETE from AssetFile where Id=?", info.Id);
            DBAdapter.DB.Execute("DELETE from TagAssignment where TagTarget=? and TargetId=?", TagAssignment.Target.Asset, info.Id);
        }

        public static Asset ForgetPackage(AssetInfo info, bool removeExclusion = false)
        {
            // delete child packages first
            foreach (AssetInfo childInfo in info.ChildInfos)
            {
                RemovePackage(childInfo, true);
            }

            DBAdapter.DB.Execute("DELETE from AssetFile where AssetId=?", info.AssetId);
            // TODO: remove assetfile tag assignments

            Asset existing = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (existing == null) return null;

            existing.CurrentState = Asset.State.New;
            info.CurrentState = Asset.State.New;
            existing.LastOnlineRefresh = DateTime.MinValue;
            info.LastOnlineRefresh = DateTime.MinValue;
            existing.ETag = null;
            info.ETag = null;
            if (removeExclusion)
            {
                existing.Exclude = false;
                info.Exclude = false;
            }

            DBAdapter.DB.Update(existing);

            return existing;
        }

        public static void RemovePackage(AssetInfo info, bool deleteFiles)
        {
            // delete child packages first
            foreach (AssetInfo childInfo in info.ChildInfos)
            {
                RemovePackage(childInfo, deleteFiles);
            }

            if (deleteFiles && info.ParentId == 0)
            {
                if (File.Exists(info.GetLocation(true))) File.Delete(info.GetLocation(true));
                if (Directory.Exists(info.GetLocation(true))) Directory.Delete(info.GetLocation(true), true);
            }
            string previewFolder = Path.Combine(GetPreviewFolder(), info.AssetId.ToString());
            if (Directory.Exists(previewFolder)) Directory.Delete(previewFolder, true);

            Asset existing = ForgetPackage(info);
            if (existing == null) return;

            DBAdapter.DB.Execute("DELETE from AssetMedia where AssetId=?", info.AssetId);
            DBAdapter.DB.Execute("DELETE from TagAssignment where TagTarget=? and TargetId=?", TagAssignment.Target.Package, info.AssetId);
            DBAdapter.DB.Execute("DELETE from Asset where Id=?", info.AssetId);
        }

        public static async Task<string> CopyTo(AssetInfo info, string folder, bool withDependencies = false, bool withScripts = false, bool fromDragDrop = false, bool outOfProject = false, bool reimport = false)
        {
            string result = null;

            // copy over SRP support reference if required for main file
            AssetInfo workInfo = info;
            if (info.SRPMainReplacement != null)
            {
                workInfo = new AssetInfo()
                    .CopyFrom(workInfo, false)
                    .CopyFrom(info.SRPSupportPackage, info.SRPMainReplacement);
            }

            string sourcePath = await EnsureMaterializedAsset(workInfo);
            bool conversionNeeded = false;
            if (sourcePath != null)
            {
                string finalPath = folder;

                // complex import structure only supported for Unity Packages
                int finalImportStructure = workInfo.AssetSource == Asset.Source.CustomPackage ||
                    workInfo.AssetSource == Asset.Source.Archive ||
                    workInfo.AssetSource == Asset.Source.AssetStorePackage
                        ? Config.importStructure
                        : 0;

                // calculate dependencies on demand
                while (workInfo.DependencyState == AssetInfo.DependencyStateOptions.Calculating) await Task.Yield();
                if (withDependencies && info.DependencyState == AssetInfo.DependencyStateOptions.Unknown)
                {
                    await CalculateDependencies(workInfo);
                }

                // override again for single files without dependencies in drag & drop scenario as that feels more natural
                if (fromDragDrop && (workInfo.Dependencies == null || workInfo.Dependencies.Count == 0)) finalImportStructure = 0;

                switch (finalImportStructure)
                {
                    case 0:
                        // put into subfolder if multiple files are affected
                        if (withDependencies && workInfo.Dependencies != null && workInfo.Dependencies.Count > 0)
                        {
                            finalPath = Path.Combine(finalPath.RemoveTrailing("."), Path.GetFileNameWithoutExtension(workInfo.FileName)).Trim().RemoveTrailing(".");
                            if (!Directory.Exists(finalPath)) Directory.CreateDirectory(finalPath);
                        }
                        break;

                    case 1:
                        string path = workInfo.Path;
                        if (path.ToLowerInvariant().StartsWith("assets/")) path = path.Substring(7);
                        finalPath = Path.Combine(folder, Path.GetDirectoryName(path));
                        break;
                }

                string targetPath = Path.Combine(finalPath, Path.GetFileName(sourcePath));
                targetPath = DoCopyTo(workInfo, sourcePath, targetPath, reimport, outOfProject);
                if (targetPath == null) return null; // error occurred

                result = targetPath;
                if (ConversionExtensions.Contains(IOUtils.GetExtensionWithoutDot(targetPath).ToLowerInvariant())) conversionNeeded = true;

                if (withDependencies)
                {
                    List<AssetFile> deps = withScripts ? workInfo.Dependencies : workInfo.MediaDependencies;
                    if (deps != null)
                    {
                        for (int i = 0; i < deps.Count; i++)
                        {
                            if (ConversionExtensions.Contains(IOUtils.GetExtensionWithoutDot(deps[i].FileName).ToLowerInvariant())) conversionNeeded = true;

                            // special handling for Asset Manager assets, as they will bring in dependencies automatically
                            if (workInfo.AssetSource == Asset.Source.AssetManager) continue;

                            // select correct asset from pool
                            Asset asset = workInfo.CrossPackageDependencies.FirstOrDefault(p => p.Id == deps[i].AssetId);
                            if (asset == null)
                            {
                                // if not found this is either the SRP original or the current asset
                                asset = workInfo.SRPSupportPackage == null ? workInfo.ToAsset() : workInfo.SRPOriginalBackup.ToAsset();
                            }

                            sourcePath = await EnsureMaterializedAsset(asset, deps[i]);
                            if (sourcePath != null)
                            {
                                switch (finalImportStructure)
                                {
                                    case 0:
                                        targetPath = Path.Combine(finalPath, Path.GetFileName(deps[i].Path));
                                        break;

                                    case 1:
                                        string path = deps[i].Path;
                                        if (path.ToLowerInvariant().StartsWith("assets/")) path = path.Substring(7);
                                        targetPath = Path.Combine(folder, path);
                                        break;
                                }

                                AssetInfo depInfo = new AssetInfo().CopyFrom(asset, deps[i]);
                                targetPath = DoCopyTo(depInfo, sourcePath, targetPath, reimport, outOfProject);
                                if (targetPath == null) return null; // error occurred
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"Dependency calculation failed for {workInfo}");
                    }
                }

                AssetDatabase.Refresh();

                if (string.IsNullOrEmpty(info.Guid))
                {
                    // special case of original index without GUID, fall back to file check only
                    if (File.Exists(targetPath)) info.ProjectPath = targetPath;
                }
                else
                {
                    info.ProjectPath = AssetDatabase.GUIDToAssetPath(workInfo.Guid);
                }

                if (Config.convertToPipeline && conversionNeeded && info.SRPSupportPackage == null)
                {
#if USE_URP_CONVERTER
                    if (AssetUtils.IsOnURP())
                    {
                        Converters.RunInBatchMode(
                            ConverterContainerId.BuiltInToURP
                            , new List<ConverterId>
                            {
                                ConverterId.Material,
                                ConverterId.ReadonlyMaterial
                            }
                            , ConverterFilter.Inclusive
                        );
                    }
#endif
                }

                Config.statsImports++;
                SaveConfig();
            }

            return result;
        }

        private static string DoCopyTo(AssetInfo info, string sourcePath, string targetPath, bool reimport = false, bool outOfProject = false)
        {
            try
            {
                bool isDirectory = Directory.Exists(sourcePath);
                if (!outOfProject && !isDirectory)
                {
                    // don't copy to different location if existing already, override instead
                    string existing = AssetDatabase.GUIDToAssetPath(info.Guid);
                    if (!string.IsNullOrWhiteSpace(existing) && !existing.Contains(TEMP_FOLDER) && File.Exists(existing))
                    {
                        targetPath = existing;
                        if (!reimport) return targetPath;
                    }
                }

                string targetFolder = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                // special handling for directory assets, e.g. complex Asset Manager assets with dependencies
                if (isDirectory)
                {
                    // copy contents of source path to target path
                    string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        string relativePath = file.Substring(sourcePath.Length + 1);
                        string targetFile = Path.Combine(targetFolder, relativePath);
                        string targetFolder2 = Path.GetDirectoryName(targetFile);
                        if (!Directory.Exists(targetFolder2)) Directory.CreateDirectory(targetFolder2);
                        File.Copy(file, targetFile, true);
                    }
                    return targetPath;
                }

                File.Copy(sourcePath, targetPath, true);

                string sourceMetaPath = sourcePath + ".meta";
                string targetMetaPath = targetPath + ".meta";
                if (File.Exists(sourceMetaPath))
                {
                    File.Copy(sourceMetaPath, targetMetaPath, true);

                    // adjust meta file to contain asset origin
                    string[] metaContent = File.ReadAllLines(targetMetaPath);
                    if (!metaContent.Any(l => l.StartsWith("AssetOrigin:")))
                    {
                        AssetOrigin origin = info.ToAsset().GetAssetOrigin();
                        string assetPath = targetPath.Replace("\\", "/");
                        try
                        {
                            origin.assetPath = assetPath.Substring(assetPath.IndexOf("Assets/", StringComparison.Ordinal));
                        }
                        catch (Exception e)
                        {
                            if (!outOfProject) Debug.LogError($"Could not determine asset path from '{assetPath}': {e.Message}");
                        }
                        List<string> newMetaContent = new List<string>(metaContent)
                        {
                            "AssetOrigin:",
                            "  serializedVersion: 1",
                            $"  productId: {origin.productId}",
                            $"  packageName: {origin.packageName}",
                            $"  packageVersion: {origin.packageVersion}",
                            $"  assetPath: {origin.assetPath}",
                            $"  uploadId: {origin.uploadId}"
                        };
                        File.WriteAllLines(targetMetaPath, newMetaContent);
                    }
                }

                return targetPath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error copying file '{sourcePath}' to '{targetPath}': {e.Message}");
                return null;
            }
        }

        public static async Task PlayAudio(AssetInfo info)
        {
            string targetPath;

            // check if in project already, then skip extraction
            if (info.InProject)
            {
                targetPath = IOUtils.PathCombine(Path.GetDirectoryName(Application.dataPath), info.ProjectPath);
            }
            else
            {
                targetPath = await EnsureMaterializedAsset(info, Config.extractSingleFiles);
            }

#if !ASSET_INVENTORY_NOAUDIO
            EditorAudioUtility.StopAllPreviewClips();
            if (targetPath != null)
            {
                AudioClip clip = await AssetUtils.LoadAudioFromFile(targetPath);
                if (clip != null) EditorAudioUtility.PlayPreviewClip(clip, 0, Config.loopAudio);
            }
#endif
        }

        public static void StopAudio()
        {
#if !ASSET_INVENTORY_NOAUDIO
            EditorAudioUtility.StopAllPreviewClips();
#endif
        }

        internal static void SetAssetExclusion(AssetInfo info, bool exclude)
        {
            Asset asset = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (asset == null) return;

            asset.Exclude = exclude;
            info.Exclude = exclude;

            DBAdapter.DB.Update(asset);
        }

        internal static void SetAssetBackup(AssetInfo info, bool backup)
        {
            Asset asset = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (asset == null) return;

            asset.Backup = backup;
            info.Backup = backup;

            DBAdapter.DB.Update(asset);
        }

        internal static void SetAssetAIUse(AssetInfo info, bool useAI, bool invokeUpdate = true)
        {
            Asset asset = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (asset == null) return;

            asset.UseAI = useAI;
            info.UseAI = useAI;

            DBAdapter.DB.Update(asset);

            if (invokeUpdate) OnPackagesUpdated?.Invoke();
        }

        internal static bool ShowAdvanced()
        {
            return !Config.hideAdvanced || Event.current.control;
        }

        internal static void SetVersion(AssetInfo info, string version)
        {
            Asset asset = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (asset == null) return;

            asset.Version = version;
            info.Version = version;

            DBAdapter.DB.Update(asset);
        }

        internal static void SetPackageVersion(AssetInfo info, PackageInfo package)
        {
            Asset asset = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (asset == null) return;

            asset.LatestVersion = package.versions.latestCompatible;
            info.LatestVersion = package.versions.latestCompatible;

            DBAdapter.DB.Update(asset);
        }

        internal static void SetAssetExtraction(AssetInfo info, bool extract)
        {
            Asset asset = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (asset == null) return;

            asset.KeepExtracted = extract;
            info.KeepExtracted = extract;

            if (extract) _extractionQueue.Enqueue(asset);

            DBAdapter.DB.Update(asset);
        }

        internal static void SetAssetUpdateStrategy(AssetInfo info, Asset.Strategy strategy)
        {
            Asset asset = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (asset == null) return;

            asset.UpdateStrategy = strategy;
            info.UpdateStrategy = strategy;

            DBAdapter.DB.Update(asset);
        }

        internal static void LoadRelativeLocations()
        {
            string curSystem = GetSystemId();

            string dataQuery = "SELECT * from RelativeLocation order by Key, Location";
            List<RelativeLocation> locations = DBAdapter.DB.Query<RelativeLocation>($"{dataQuery}").ToList();
            locations.ForEach(l => l.SetLocation(l.Location)); // ensure all paths use forward slashes

            // ensure additional folders don't contain additional unmapped keys (e.g. after database cleanup)
            foreach (FolderSpec spec in Config.folders)
            {
                if (!string.IsNullOrWhiteSpace(spec.relativeKey) && !locations.Any(rl => rl.Key == spec.relativeKey))
                {
                    // self-heal
                    RelativeLocation rel = new RelativeLocation();
                    rel.System = curSystem;
                    rel.Key = spec.relativeKey;
                    DBAdapter.DB.Insert(rel);
                    locations.Add(rel);
                }
            }

            _relativeLocations = locations.Where(l => l.System == curSystem).ToList();

            // add predefined locations
            _relativeLocations.Insert(0, new RelativeLocation("ac", curSystem, GetAssetCacheFolder()));
            _relativeLocations.Insert(1, new RelativeLocation("pc", curSystem, GetPackageCacheFolder()));

            foreach (RelativeLocation location in locations.Where(l => l.System != curSystem))
            {
                // add key as undefined if not there
                if (!_relativeLocations.Any(rl => rl.Key == location.Key))
                {
                    _relativeLocations.Add(new RelativeLocation(location.Key, curSystem, null));
                }

                // add location inside other systems for reference
                RelativeLocation loc = _relativeLocations.First(rl => rl.Key == location.Key);
                if (loc.otherLocations == null) loc.otherLocations = new List<string>();
                loc.otherLocations.Add(location.Location);
            }

            // ensure never null
            _relativeLocations.ForEach(rl =>
            {
                if (rl.otherLocations == null) rl.otherLocations = new List<string>();
            });

            _userRelativeLocations = _relativeLocations.Where(rl => rl.Key != "ac" && rl.Key != "pc").ToList();
        }

        internal static void ConnectToAssetStore(AssetInfo info, AssetDetails details)
        {
            Asset existing = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (existing == null) return;

            existing.ETag = null;
            info.ETag = null;
            existing.ForeignId = int.Parse(details.packageId);
            info.ForeignId = int.Parse(details.packageId);
            existing.LastOnlineRefresh = DateTime.MinValue;
            info.LastOnlineRefresh = DateTime.MinValue;

            DBAdapter.DB.Update(existing);
        }

        internal static void DisconnectFromAssetStore(AssetInfo info, bool removeMetadata)
        {
            Asset existing = DBAdapter.DB.Find<Asset>(info.AssetId);
            if (existing == null) return;

            existing.ForeignId = 0;
            info.ForeignId = 0;

            if (removeMetadata)
            {
                existing.AssetRating = null;
                info.AssetRating = null;
                existing.SafePublisher = null;
                info.SafePublisher = null;
                existing.DisplayPublisher = null;
                info.DisplayPublisher = null;
                existing.SafeCategory = null;
                info.SafeCategory = null;
                existing.DisplayCategory = null;
                info.DisplayCategory = null;
                existing.DisplayName = null;
                info.DisplayName = null;
                existing.OfficialState = null;
                info.OfficialState = null;
                existing.PriceCny = 0;
                info.PriceCny = 0;
                existing.PriceEur = 0;
                info.PriceEur = 0;
                existing.PriceUsd = 0;
                info.PriceUsd = 0;
            }

            DBAdapter.DB.Update(existing);
        }

        internal static string CreateDebugReport()
        {
            string result = "Asset Inventory Support Diagnostics\n";
            result += $"\nDate: {DateTime.Now}";
            result += $"\nVersion: {VERSION}";
            result += $"\nUnity: {Application.unityVersion}";
            result += $"\nPlatform: {Application.platform}";
            result += $"\nOS: {Environment.OSVersion}";
            result += $"\nLanguage: {Application.systemLanguage}";

            List<AssetInfo> assets = LoadAssets();
            result += $"\n\n{assets.Count} Packages";
            foreach (AssetInfo asset in assets)
            {
                result += $"\n{asset} ({asset.SafeName}) - {asset.AssetSource} - {asset.GetVersion()}";
            }

            List<Tag> tags = Tagging.LoadTags();
            result += $"\n\n{tags.Count} Tags";
            foreach (Tag tag in tags)
            {
                result += $"\n{tag} ({tag.Id})";
            }

            result += $"\n\n{Tagging.Tags.Count()} Tag Assignments";
            foreach (TagInfo tag in Tagging.Tags)
            {
                result += $"\n{tag})";
            }

            return result;
        }

        internal static string GetSystemId()
        {
            return SystemInfo.deviceUniqueIdentifier; // + "test";
        }

        internal static bool IsRel(string path)
        {
            return path != null && path.StartsWith(TAG_START);
        }

        internal static string GetRelKey(string path)
        {
            return path.Replace(TAG_START, "").Replace(TAG_END, "");
        }

        internal static string DeRel(string path, bool emptyIfMissing = false)
        {
            if (path == null) return null;
            if (!IsRel(path)) return path;

            foreach (RelativeLocation location in RelativeLocations)
            {
                if (string.IsNullOrWhiteSpace(location.Location))
                {
                    if (emptyIfMissing) return null;
                    continue;
                }

                path = path.Replace($"{TAG_START}{location.Key}{TAG_END}", location.Location);
            }

            // check if some rule caught it
            if (IsRel(path) && emptyIfMissing) return null;

            return path;
        }

        internal static string MakeRelative(string path)
        {
            path = IOUtils.ToShortPath(path.Replace("\\", "/"));

            StringBuilder sb = new StringBuilder(path);
            foreach (RelativeLocation location in RelativeLocations)
            {
                if (string.IsNullOrWhiteSpace(location.Location)) continue;

                string oldPath = location.Location;
                if (path.Contains(oldPath))
                {
                    string newPath = $"{TAG_START}{location.Key}{TAG_END}";

                    sb.Replace(oldPath, newPath);
                }
            }

            return sb.ToString();
        }

        internal static AssetInfo GetAssetByPath(string path, Asset asset)
        {
            string query = "SELECT *, AssetFile.Id as Id from AssetFile left join Asset on Asset.Id = AssetFile.AssetId where Lower(AssetFile.Path) = ? and Asset.Id = ?";
            List<AssetInfo> result = DBAdapter.DB.Query<AssetInfo>(query, path.ToLowerInvariant(), asset.Id);

            return result.FirstOrDefault();
        }

        internal static void RegisterSelection(List<AssetInfo> assets)
        {
            GetObserver().SetPrioritized(assets);
        }

        public static void TriggerPackageRefresh()
        {
            OnPackagesUpdated?.Invoke();
        }

        internal static void SetPipelineConversion(bool active)
        {
            Config.convertToPipeline = active;
            SaveConfig();
        }

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
        private static CloudAssetManagement _cam;
        internal static async Task<CloudAssetManagement> GetCloudAssetManagement()
        {
            await PlatformServices.InitOnDemand();
            if (_cam == null) _cam = new CloudAssetManagement();

            return _cam;
        }
#endif
        private class AssetEntry
        {
            public string Name;
            public bool IsSubPackage;
            public string GroupKey;
        }
    }
}
