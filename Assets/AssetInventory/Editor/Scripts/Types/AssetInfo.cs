using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
using Unity.Cloud.Assets;
#endif

namespace AssetInventory
{
    [Serializable]
    // used to contain results of join calls
    public sealed class AssetInfo : AssetFile
    {
        public enum ImportStateOptions
        {
            Unknown = 0,
            Queued = 1,
            Missing = 2,
            Importing = 3,
            Imported = 4,
            Failed = 5,
            Cancelled = 6
        }

        public enum DependencyStateOptions
        {
            Unknown = 0,
            Calculating = 1,
            Done = 2,
            NotPossible = 3,
            Failed = 4
        }

        public int ParentId { get; set; }
        public Asset.Source AssetSource { get; set; }
        public string Location { get; set; }
        public string OriginalLocation { get; set; }
        public string OriginalLocationKey { get; set; }
        public string Registry { get; set; }
        public string Repository { get; set; }
        public PackageSource PackageSource { get; set; }
        public int ForeignId { get; set; }
        public long PackageSize { get; set; }
        public string SafeName { get; set; }
        public string DisplayName { get; set; }
        public string SafePublisher { get; set; }
        public string DisplayPublisher { get; set; }
        public string SafeCategory { get; set; }
        public string DisplayCategory { get; set; }
        public int PublisherId { get; set; }
        public Asset.State CurrentState { get; set; }
        public Asset.SubState CurrentSubState { get; set; }
        public string Slug { get; set; }
        public int Revision { get; set; }
        public string Description { get; set; }
        public string KeyFeatures { get; set; }
        public string CompatibilityInfo { get; set; }
        public string SupportedUnityVersions { get; set; }
        public bool BIRPCompatible { get; set; }
        public bool URPCompatible { get; set; }
        public bool HDRPCompatible { get; set; }
        public string Keywords { get; set; }
        public string PackageDependencies { get; set; }
        public string Version { get; set; }
        public string LatestVersion { get; set; }
        public Asset.Strategy UpdateStrategy { get; set; }
        public string License { get; set; }
        public string LicenseLocation { get; set; }
        public DateTime LastRelease { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime FirstRelease { get; set; }
        public string AssetRating { get; set; }
        public int RatingCount { get; set; }
        public float Hotness { get; set; }
        public float PriceEur { get; set; }
        public float PriceUsd { get; set; }
        public float PriceCny { get; set; }
        public string Requirements { get; set; }
        public string ReleaseNotes { get; set; }
        public string OfficialState { get; set; }
        public bool IsHidden { get; set; }
        public bool Exclude { get; set; }
        public bool Backup { get; set; }
        public bool KeepExtracted { get; set; }
        public bool UseAI { get; set; }
        public string UploadId { get; set; }
        public string ETag { get; set; }
        public DateTime LastOnlineRefresh { get; set; }
        public int FileCount { get; set; }
        public long UncompressedSize { get; set; }

        // runtime only
        internal AssetDownloader PackageDownloader { get; set; }
        internal AssetOrigin Origin { get; set; }

        [field: NonSerialized] public AssetInfo ParentInfo { get; set; }
        [field: NonSerialized] public List<AssetInfo> ChildInfos { get; set; } = new List<AssetInfo>();

        [field: NonSerialized] internal AssetInfo SRPOriginalBackup { get; set; }
        internal Asset SRPSupportPackage { get; set; }
        internal AssetFile SRPMainReplacement { get; set; }
        internal List<AssetFile> SRPSupportFiles { get; set; }
        internal bool SRPUsed { get; set; }

        internal Texture2D PreviewTexture { get; set; }
        public bool IsIndexed => FileCount > 0 && (CurrentState == Asset.State.Done || CurrentState == Asset.State.New); // new is set when deleting local package file
        public bool IsDeprecated => OfficialState == "deprecated";
        public bool IsAbandoned => OfficialState == "disabled";
        public bool IsMaterialized { get; set; }
        public ImportStateOptions ImportState { get; set; }
        public DependencyStateOptions DependencyState { get; set; } = DependencyStateOptions.Unknown;
        public List<AssetFile> Dependencies { get; set; }
        public List<AssetFile> MediaDependencies { get; set; }
        public List<AssetFile> ScriptDependencies { get; set; }
        public List<Asset> CrossPackageDependencies { get; set; }
        public long DependencySize { get; set; }
        internal bool WasOutdated { get; set; }
        public List<AssetMedia> AllMedia { get; set; }
        public List<AssetMedia> Media { get; set; }

        private bool _tagsDone;
        private List<TagInfo> _packageTags;
        private List<TagInfo> _assetTags;
        private int _tagHash;
        private string _packageSamplesLoaded;
        private IEnumerable<UnityEditor.PackageManager.UI.Sample> _packageSamples;
        private List<Dependency> _packageDependencies;
        [field: NonSerialized] private List<AssetInfo> _packageUsageDependencies;

        public List<TagInfo> PackageTags
        {
            get
            {
                EnsureTagsLoaded();
                return _packageTags;
            }
        }

        internal void SetTagsDirty() => _tagsDone = false;

        public List<TagInfo> AssetTags
        {
            get
            {
                EnsureTagsLoaded();
                return _assetTags;
            }
        }

        public List<Dependency> GetPackageDependencies()
        {
            if (string.IsNullOrEmpty(PackageDependencies)) return null;
            if (_packageDependencies == null || _packageDependencies.Count == 0)
            {
                _packageDependencies = JsonConvert.DeserializeObject<List<Dependency>>(PackageDependencies);
            }
            return _packageDependencies != null && _packageDependencies.Count > 0 ? _packageDependencies : null;
        }

        public List<AssetInfo> GetPackageUsageDependencies(List<AssetInfo> assets)
        {
            if (assets == null || assets.Count == 0) return null;
            if (_packageUsageDependencies == null)
            {
                _packageUsageDependencies = assets.Where(a => a.GetPackageDependencies() != null && a.GetPackageDependencies().Any(d => (ForeignId > 0 && d.id == ForeignId) || d.location == SafeName)).ToList();
            }
            return _packageUsageDependencies != null && _packageUsageDependencies.Count > 0 ? _packageUsageDependencies : null;
        }

        public bool IsDownloaded
        {
            get
            {
                if (ParentInfo != null) return ParentInfo.IsDownloaded;
                if (_downloaded != null) return _downloaded.Value;

                // special asset types
                if (AssetSource == Asset.Source.RegistryPackage
                    || AssetSource == Asset.Source.AssetManager
                    || (AssetSource == Asset.Source.Archive && File.Exists(GetLocation(true)))
                    || (AssetSource == Asset.Source.Directory && Directory.Exists(GetLocation(true))))
                {
                    _downloaded = true;
                    return _downloaded.Value;
                }

                // "none" is a special case, it's a placeholder for assets that are not attached
                if (SafeName == Asset.NONE)
                {
                    _downloaded = true;
                    return _downloaded.Value;
                }

                // check for missing location
                string location = GetLocation(true);
                if (string.IsNullOrEmpty(location))
                {
                    _downloaded = false;
                    return _downloaded.Value;
                }

                // check for missing file
                if (!File.Exists(location))
                {
                    _downloaded = false;
                    return _downloaded.Value;
                }

                // due to Unity bug verify downloaded asset is indeed asset in question, could be multi-versioned asset
                if (AssetSource == Asset.Source.AssetStorePackage || AI.Config.showCustomPackageUpdates)
                {
                    AssetHeader header = UnityPackageImporter.ReadHeader(location, true);
                    if (header != null)
                    {
                        if (int.TryParse(header.id, out int id))
                        {
                            if (id != ForeignId)
                            {
                                _downloaded = false;
                                _downloadedActual = header.version;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(header.unity_version))
                        {
                            _downloadedCompatible = new SemVer(header.unity_version) <= new SemVer(Application.unityVersion);
                            _downloadedUnityVersion = header.unity_version;
                        }
                    }
                }
                _downloaded = _downloaded == null;
                return _downloaded.Value;
            }
        }
        private bool? _downloaded;
        private bool? _updateAvailable;
        private bool? _updateAvailableForced;
        private bool? _updateAvailableList;
        private bool? _updateAvailableListForced;

        public string DownloadedActual
        {
            get
            {
                if (ParentInfo != null) return ParentInfo.DownloadedActual;
                if (_downloaded == null) _ = IsDownloaded;
                return _downloadedActual;
            }
        }
        private string _downloadedActual;

        public bool IsDownloadedCompatible
        {
            get
            {
                if (ParentInfo != null) return ParentInfo.IsDownloadedCompatible;
                if (_downloaded == null) _ = IsDownloaded;
                return _downloadedCompatible;
            }
        }
        private bool _downloadedCompatible = true;

        public string DownloadededUnityVersion
        {
            get
            {
                if (ParentInfo != null) return ParentInfo.DownloadededUnityVersion;
                if (_downloaded == null) _ = IsDownloaded;
                return _downloadedUnityVersion;
            }
        }
        private string _downloadedUnityVersion;

        public bool IsCurrentUnitySupported()
        {
            if (string.IsNullOrWhiteSpace(SupportedUnityVersions)) return true;
            string[] arr = SupportedUnityVersions.Split(new[] {", "}, StringSplitOptions.None); // 2019.4 C# syntax

            return new SemVer(arr[0]) <= new SemVer(Application.unityVersion);
        }

        private string _forcedTargetVersion;

        public AssetInfo()
        {
        }

        public AssetInfo(Asset asset)
        {
            CopyFrom(asset);
        }

        public AssetInfo(AssetInfo info)
        {
            CopyFrom(info, false);
        }

        public AssetInfo GetRoot()
        {
            if (ParentId > 0 && ParentInfo != null && (ForeignId == 0 || ForeignId == ParentInfo.ForeignId))
            {
                return ParentInfo;
            }
            return this;
        }

        internal bool IsMediaLoading()
        {
            return AllMedia != null && AllMedia.Any(m => m.IsDownloading);
        }

        internal void DisposeMedia()
        {
            DisposeMedia(AllMedia);
            AllMedia = null;

            DisposeMedia(Media);
            Media = null;
        }

        internal void DisposeMedia(List<AssetMedia> media)
        {
            media?.ForEach(m =>
            {
                if (m.Texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(m.Texture);
                    m.Texture = null;
                }
                if (m.ThumbnailTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(m.ThumbnailTexture);
                    m.ThumbnailTexture = null;
                }
            });
        }

        private void EnsureTagsLoaded()
        {
            if (!_tagsDone || AI.TagHash != _tagHash)
            {
                _assetTags = Tagging.GetAssetTags(Id);
                _packageTags = Tagging.GetPackageTags(AssetId);
                _tagsDone = true;
                _tagHash = AI.TagHash;
            }
        }

        public bool IsIndirectPackageDependency()
        {
            if (AssetSource != Asset.Source.RegistryPackage) return false;

            PackageInfo pInfo = AssetStore.GetInstalledPackage(this);
            return pInfo != null && !pInfo.isDirectDependency;
        }

        public bool HasSamples()
        {
            IEnumerable<UnityEditor.PackageManager.UI.Sample> packageSamples = GetSamples();
            return packageSamples != null && packageSamples.Any();
        }

        public IEnumerable<UnityEditor.PackageManager.UI.Sample> GetSamples()
        {
            if (AssetSource != Asset.Source.RegistryPackage) return null;

            string installedPackageVersion = InstalledPackageVersion();
            if (installedPackageVersion == null) return null;

            if (_packageSamplesLoaded != installedPackageVersion)
            {
                _packageSamplesLoaded = installedPackageVersion;
                _packageSamples = UnityEditor.PackageManager.UI.Sample.FindByPackage(SafeName, installedPackageVersion);
            }
            return _packageSamples;
        }

        public string InstalledPackageVersion()
        {
            if (AssetSource != Asset.Source.RegistryPackage) return null;

            PackageInfo pInfo = AssetStore.GetInstalledPackage(this);
            return pInfo?.version;
        }

        public string TargetPackageVersion()
        {
            if (AssetSource != Asset.Source.RegistryPackage) return null;
            if (!string.IsNullOrEmpty(_forcedTargetVersion)) return _forcedTargetVersion;

            PackageInfo pInfo = AssetStore.GetPackageInfo(this);
            if (pInfo == null) return null;

            switch (UpdateStrategy)
            {
                case Asset.Strategy.LatestStableCompatible:
                    return pInfo.versions.compatible.LastOrDefault(p => !p.ToLowerInvariant().Contains("pre") && !p.ToLowerInvariant().Contains("exp"));

                case Asset.Strategy.LatestCompatible:
                    return pInfo.versions.compatible.LastOrDefault();

                case Asset.Strategy.Recommended:
                    return string.IsNullOrWhiteSpace(GetVerifiedVersion(pInfo)) ? null : GetVerifiedVersion(pInfo);

                case Asset.Strategy.RecommendedOrLatestStableCompatible:
                    if (string.IsNullOrWhiteSpace(GetVerifiedVersion(pInfo)))
                    {
                        return pInfo.versions.compatible.LastOrDefault(p => !p.ToLowerInvariant().Contains("pre") && !p.ToLowerInvariant().Contains("exp"));
                    }
                    return GetVerifiedVersion(pInfo);

                case Asset.Strategy.Manually:
                    return null;
            }

            return null;
        }

        #if UNITY_2022_2_OR_NEWER
        private string GetVerifiedVersion(PackageInfo pInfo) => pInfo.versions.recommended;
        #else
        private string GetVerifiedVersion(PackageInfo pInfo) => pInfo.versions.verified;
        #endif

        public string GetDisplayName(bool extended = false)
        {
            string result = string.IsNullOrEmpty(DisplayName) ? SafeName : DisplayName;
            if (extended && AssetSource == Asset.Source.RegistryPackage && !string.IsNullOrWhiteSpace(InstalledPackageVersion())) result += " - " + InstalledPackageVersion();
            return result;
        }

        public string GetDisplayPublisher() => string.IsNullOrEmpty(DisplayPublisher) ? SafePublisher : DisplayPublisher;
        public string GetDisplayCategory() => string.IsNullOrEmpty(DisplayCategory) ? SafeCategory : DisplayCategory;

        public string GetChangeLog(string versionOverride = null)
        {
            if (string.IsNullOrWhiteSpace(ReleaseNotes) && Registry == Asset.UNITY_REGISTRY)
            {
                SemVer version = new SemVer(string.IsNullOrEmpty(versionOverride) ? Version : versionOverride);
                return $"https://docs.unity3d.com/Packages/{SafeName}@{version.Major}.{version.Minor}/changelog/CHANGELOG.html";
            }
            return ReleaseNotes;
        }

        public string GetChangeLogURL(string versionOverride = null)
        {
            string changeLog = GetChangeLog(versionOverride);

            return StringUtils.IsUrl(changeLog) ? changeLog : null;
        }

        public string GetLocation(bool expanded)
        {
            return expanded ? AI.DeRel(Location) : Location;
        }

        public string GetVersion(bool returnIndexedIfNone = false)
        {
            if (AssetSource == Asset.Source.RegistryPackage)
            {
                string installedPackageVersion = InstalledPackageVersion();
                return installedPackageVersion == null ? (returnIndexedIfNone ? Version : null) : installedPackageVersion;
            }

            return GetRoot().Version;
        }

        // keep in sync with copy in Asset
        public async Task<string> GetLocation(bool expanded, bool resolveParent)
        {
            string archivePath = Location;
            if (resolveParent && ParentId > 0)
            {
                Asset parentAsset = ParentInfo?.ToAsset();
                if (parentAsset == null) parentAsset = DBAdapter.DB.Find<Asset>(ParentId);
                if (parentAsset == null)
                {
                    Debug.LogError($"Could not resolve parent asset of '{GetDisplayName()}'.");
                }
                else
                {
                    if (!AI.IsMaterialized(parentAsset)) await AI.ExtractAsset(parentAsset);

                    string[] arr = Location.Split(Asset.SUB_PATH);
                    AssetFile parentAssetFile = DBAdapter.DB.Query<AssetFile>("select * from AssetFile where AssetId=? and Path=?", ParentId, arr.Last()).FirstOrDefault();
                    if (parentAssetFile == null)
                    {
                        Debug.LogError($"Could not resolve package in parent index {arr.Last()}. This usually means the parent package changed or was reorganized and this is an orphan. Reindex the parent package to resolve this.");
                        return null;
                    }
                    archivePath = await AI.EnsureMaterializedAsset(parentAsset, parentAssetFile);
                }
            }
            return expanded ? AI.DeRel(archivePath) : archivePath;
        }

        public void SetLocation(string location)
        {
            if (location == null)
            {
                Location = null;
                return;
            }
            Location = AI.MakeRelative(location);
        }

        internal bool IsLocationUnmappedRelative()
        {
            return AI.IsRel(Location) && AI.DeRel(Location, true) == null;
        }

        public bool IsUpdateAvailable(bool force = true)
        {
            // quick checks can remain uncached
            if (ParentId > 0) return false;
            if (WasOutdated) return false;

            if (force && _updateAvailableForced != null) return _updateAvailableForced.Value;
            if (!force && _updateAvailable != null) return _updateAvailable.Value;

            if (IsAbandoned || IsDeprecated)
            {
                _updateAvailable = false;
                _updateAvailableForced = false;
                return false;
            }

            // registry packages should only flag update if inside current project and compatible
            if (AssetSource == Asset.Source.RegistryPackage)
            {
                if (!AI.Config.showIndirectPackageUpdates)
                {
                    PackageInfo pInfo = AssetStore.GetInstalledPackage(this);
                    if (pInfo != null && !pInfo.isDirectDependency)
                    {
                        _updateAvailable = false;
                        _updateAvailableForced = false;
                        return false;
                    }
                }
                bool packageUpdateAvailable = InstalledPackageVersion() != null && TargetPackageVersion() != null && InstalledPackageVersion() != TargetPackageVersion();

                _updateAvailable = packageUpdateAvailable;
                _updateAvailableForced = packageUpdateAvailable;

                return packageUpdateAvailable;
            }

            // custom packages are typically treated as not updateable
            if (!force && AssetSource == Asset.Source.CustomPackage && !AI.Config.showCustomPackageUpdates)
            {
                _updateAvailable = false;
                return false;
            }

            // check for missing version information
            if (string.IsNullOrWhiteSpace(Version) || string.IsNullOrWhiteSpace(LatestVersion))
            {
                _updateAvailable = false;
                _updateAvailableForced = false;
                return false;
            }

            // compare versions
            bool updateAvailable = new SemVer(Version) < new SemVer(LatestVersion);
            if (force)
            {
                _updateAvailableForced = updateAvailable;
            }
            else
            {
                _updateAvailable = updateAvailable;
            }
            return updateAvailable;
        }

        public bool IsUpdateAvailable(List<AssetInfo> assets, bool force = true)
        {
            if (ParentId > 0) return false;
            if (force && _updateAvailableListForced != null) return _updateAvailableListForced.Value;
            if (!force && _updateAvailableList != null) return _updateAvailableList.Value;

            bool isOlderVersion = IsUpdateAvailable(force);
            if (isOlderVersion && assets != null && AssetSource != Asset.Source.RegistryPackage)
            {
                // if asset in that version is already loaded don't flag as update available
                if (assets.Any(a => a.AssetSource == Asset.Source.AssetStorePackage && a.ForeignId == ForeignId && a.Version == LatestVersion && !string.IsNullOrEmpty(a.GetLocation(true))))
                {
                    if (force)
                    {
                        _updateAvailableListForced = false;
                    }
                    else
                    {
                        _updateAvailableList = false;
                    }
                    return false;
                }
            }
            if (force)
            {
                _updateAvailableListForced = isOlderVersion;
            }
            else
            {
                _updateAvailableList = isOlderVersion;
            }
            return isOlderVersion;
        }

        public bool IsDownloading()
        {
            if (ParentInfo != null) return ParentInfo.IsDownloading();
            return PackageDownloader != null && PackageDownloader.GetState().state == AssetDownloader.State.Downloading;
        }

        // duplicated from Asset to avoid thousands of unnecessary casts
        internal string GetCalculatedLocation()
        {
            if (string.IsNullOrEmpty(SafePublisher) || string.IsNullOrEmpty(SafeCategory) || string.IsNullOrEmpty(SafeName)) return null;

            try
            {
                return System.IO.Path.Combine(AI.GetAssetCacheFolder(), SafePublisher, SafeCategory, SafeName + ".unitypackage").Replace("\\", "/");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to calculate location for '{GetDisplayName()}' with publisher '{SafePublisher}', category '{SafeCategory}' and name '{SafeName}': {e.Message}");
                return null;
            }
        }

        internal AssetInfo WithTreeData(string name, int id = 0, int depth = 0)
        {
            m_Name = name;
            m_ID = id;
            m_Depth = depth;

            return this;
        }

        internal AssetInfo WithTreeId(int id)
        {
            m_ID = id;

            return this;
        }

        internal AssetInfo WithProjectPath(string path)
        {
            ProjectPath = path;

            return this;
        }

        public bool IsFeaturePackage()
        {
            return AssetSource == Asset.Source.RegistryPackage && SafeName != null && SafeName.StartsWith("com.unity.feature.");
        }

        internal Texture GetFallbackIcon()
        {
            Texture result = null;
            if (AssetSource == Asset.Source.RegistryPackage)
            {
                if (IsFeaturePackage())
                {
                    result = EditorGUIUtility.IconContent("d_Asset Store@2x").image;
                }
                else
                {
                #if UNITY_2020_1_OR_NEWER
                    result = EditorGUIUtility.IconContent("d_Package Manager@2x").image;
                #else
                    result = EditorGUIUtility.IconContent("d_PreMatCube@2x").image;
                #endif
                }
            }
            else if (AssetSource == Asset.Source.Archive)
            {
                result = EditorGUIUtility.IconContent("d_FilterByType@2x").image;
            }
            else if (AssetSource == Asset.Source.Directory)
            {
                result = EditorGUIUtility.IconContent("d_Folder Icon").image;
            }
            else if (AssetSource == Asset.Source.CustomPackage)
            {
                result = EditorGUIUtility.IconContent("d_ModelImporter Icon").image;
            }
            else if (AssetSource == Asset.Source.AssetManager)
            {
                result = EditorGUIUtility.IconContent("d_CloudConnect").image;
            }

            return result;
        }

        public Asset ToAsset()
        {
            Asset result = new Asset
            {
                AssetSource = AssetSource,
                DisplayCategory = DisplayCategory,
                SafeCategory = SafeCategory,
                CurrentState = CurrentState,
                CurrentSubState = CurrentSubState,
                Id = AssetId,
                ParentId = ParentId,
                Slug = Slug,
                Revision = Revision,
                Registry = Registry,
                Repository = Repository,
                PackageSource = PackageSource,
                Description = Description,
                KeyFeatures = KeyFeatures,
                CompatibilityInfo = CompatibilityInfo,
                SupportedUnityVersions = SupportedUnityVersions,
                Keywords = Keywords,
                PackageDependencies = PackageDependencies,
                Version = Version,
                LatestVersion = LatestVersion,
                UpdateStrategy = UpdateStrategy,
                License = License,
                LicenseLocation = LicenseLocation,
                PurchaseDate = PurchaseDate,
                LastRelease = LastRelease,
                FirstRelease = FirstRelease,
                AssetRating = AssetRating,
                RatingCount = RatingCount,
                Hotness = Hotness,
                PriceEur = PriceEur,
                PriceUsd = PriceUsd,
                PriceCny = PriceCny,
                Requirements = Requirements,
                ReleaseNotes = ReleaseNotes,
                OfficialState = OfficialState,
                IsHidden = IsHidden,
                Exclude = Exclude,
                UploadId = UploadId,
                ETag = ETag,
                OriginalLocation = OriginalLocation,
                OriginalLocationKey = OriginalLocationKey,
                ForeignId = ForeignId,
                SafeName = SafeName,
                DisplayName = DisplayName,
                PackageSize = PackageSize,
                SafePublisher = SafePublisher,
                DisplayPublisher = DisplayPublisher,
                PublisherId = PublisherId,
                Backup = Backup,
                KeepExtracted = KeepExtracted,
                UseAI = UseAI,
                LastOnlineRefresh = LastOnlineRefresh
            };
            result.SetLocation(Location);
            if (ParentInfo != null) result.ParentAsset = ParentInfo.ToAsset();

            return result;
        }

        public string GetItemLink()
        {
            return $"https://assetstore.unity.com/packages/slug/{ForeignId}";
        }

        public string GetPublisherLink()
        {
            return $"https://assetstore.unity.com/publishers/{PublisherId}";
        }

        public string GetAMOrganizationUrl()
        {
            if (AssetSource != Asset.Source.AssetManager) return null;

            return $"https://cloud.unity.com/home/organizations/{OriginalLocationKey}";
        }

        public string GetAMProjectUrl()
        {
            if (AssetSource != Asset.Source.AssetManager) return null;

            Asset root = ToAsset().GetRootAsset();

            return $"{GetAMOrganizationUrl()}/projects/{root.SafeName}/assets";
        }

        public string GetAMCollectionUrl()
        {
            if (AssetSource != Asset.Source.AssetManager) return null;

            return $"{GetAMProjectUrl()}/collectionPath/{UnityWebRequest.EscapeURL(Location)}";
        }

        public string GetAMAssetUrl()
        {
            if (AssetSource != Asset.Source.AssetManager) return null;

            return $"{GetAMProjectUrl()}?assetId={Guid}:{FileVersion}";
        }

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
        public AssetType GetAMAssetType()
        {
            AssetType result = AssetType.Other;

            if (AI.TypeGroups["Audio"].Contains(Type))
            {
                result = AssetType.Audio;
            }
            else if (AI.TypeGroups["Models"].Contains(Type))
            {
                result = AssetType.Model_3D;
            }
            else if (AI.TypeGroups["Images"].Contains(Type))
            {
                result = AssetType.Asset_2D;
            }
            else if (AI.TypeGroups["Materials"].Contains(Type))
            {
                result = AssetType.Material;
            }
            else if (AI.TypeGroups["Scripts"].Contains(Type))
            {
                result = AssetType.Script;
            }
            else if (AI.TypeGroups["Videos"].Contains(Type))
            {
                result = AssetType.Video;
            }

            return result;
        }
#endif

        internal int GetChildDepth()
        {
            if (ParentId == 0 || ParentInfo == null) return 0;
            return ParentInfo.GetChildDepth() + 1;
        }

        public float GetPrice()
        {
            switch (AI.Config.currency)
            {
                case 0: return PriceEur;
                case 1: return PriceUsd;
                case 2: return PriceCny;
            }
            return 0;
        }

        public string GetPriceText()
        {
            return GetPriceText(GetPrice());
        }

        public string GetPriceText(float priceVal)
        {
            string price = priceVal.ToString("N2");
            switch (AI.Config.currency)
            {
                case 0: return $"€{price}";
                case 1: return $"${price}";
                case 2: return $"¥{price}";
            }

            return price;
        }

        internal void Refresh(bool downloadStateOnly = false)
        {
            ParentInfo?.Refresh(downloadStateOnly);

            _downloaded = null;
            _downloadedActual = null;
            _updateAvailable = null;
            _updateAvailableForced = null;
            _updateAvailableList = null;
            _updateAvailableListForced = null;

            PackageDownloader?.SetAsset(this);

            if (downloadStateOnly) return;

            WasOutdated = false;
        }

        internal void ForceTargetVersion(string newVersion)
        {
            _forcedTargetVersion = newVersion;
        }

        internal int GetFolderSpecType()
        {
            if (IsArchive()) return 2;
            if (IsPackage()) return 0;

            return 1;
        }

        public bool IsAsset()
        {
            return string.IsNullOrEmpty(FileName);
        }

        internal AssetInfo CopyFrom(Asset asset, AssetFile af = null)
        {
            if (asset != null)
            {
                // take over all asset properties
                foreach (PropertyInfo assetProp in typeof (Asset).GetProperties())
                {
                    if (!assetProp.CanRead) continue;
                    PropertyInfo thisProp = GetType().GetProperty(assetProp.Name);
                    if (thisProp == null || !thisProp.CanWrite) continue; // Property doesn't exist or isn't writable in 'this'

                    object value = assetProp.GetValue(asset);
                    thisProp.SetValue(this, value);
                }
                AssetId = asset.Id;
                if (ParentInfo != null && ParentInfo.AssetId != AssetId)
                {
                    ParentInfo = null;
                    ChildInfos = new List<AssetInfo>();
                }
            }

            if (af != null)
            {
                // take over all asset file properties
                foreach (PropertyInfo pi in typeof (AssetFile).GetProperties())
                {
                    if (!pi.CanWrite) continue;

                    pi.SetValue(this, pi.GetValue(af));
                }
            }

            return this;
        }

        internal AssetInfo CopyFrom(AssetInfo info, bool publicOnly)
        {
            // take over all asset properties
            BindingFlags flags = publicOnly ?
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public :
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (PropertyInfo pi in typeof (AssetInfo).GetProperties(flags))
            {
                if (!pi.CanWrite) continue;

                pi.SetValue(this, pi.GetValue(info));
            }

            return this;
        }

        public override string ToString()
        {
            if (IsAsset())
            {
                return $"Asset Package '{GetDisplayName()}' ({AssetId}, {FileCount} files)";
            }
            return $"Asset Info '{FileName}' ({GetDisplayName()})'";
        }
    }
}
