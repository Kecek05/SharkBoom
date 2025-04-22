using System;
using System.Collections.Generic;

namespace AssetInventory
{
    [Serializable]
    public sealed class AssetInventorySettings
    {
        private const int LOG_MEDIA_DOWNLOADS = 1;
        private const int LOG_IMAGE_RESIZING = 2;
        private const int LOG_AUDIO_PARSING = 4;
        private const int LOG_PACKAGE_PARSING = 8;

        public int version = UpgradeUtil.CURRENT_CONFIG_VERSION;
        public int searchType;
        public int searchField;
        public int sortField;
        public bool sortDescending;
        public int maxResults = 5;
        public int maxResultsLimit = 10000;
        public int timeout = 20;
        public int tileText;
        public bool allowEasyMode = true;
        public bool autoPlayAudio = true;
        public int autoCalculateDependencies; // 0 - none, 1 - all, 2 - only simple, no fbx
        public bool allowCrossPackageDependencies = true;
        public bool loopAudio;
        public bool pingSelected = true;
        public bool pingImported = true;
        public int doubleClickBehavior = 1; // 0 = none, 1 = import, 2 = open
        public bool groupLists = true;
        public bool autoHideSettings;
        public bool showTileSizeSlider;
        public bool keepAutoDownloads;
        public bool limitAutoDownloads;
        public int downloadLimit = 500;
        public bool searchAutomatically = true;
        public bool searchSubPackages;
        public bool extractSingleFiles;
        public int previewVisibility;
        public int searchTileSize = 128;
        public float searchDelay = 0.3f;
        public float hueRange = 10f;
        public int animationGrid = 4;
        public float animationSpeed = 0.1f;
        public bool excludePreviewExtensions;
        public string excludedPreviewExtensions;
        public bool excludeExtensions = true;
        public string excludedExtensions = "asset;json;txt;cs;md;uss;asmdef;uxml;editorconfig;signature;yml;cginc;gitattributes;release;collabignore;suo";
        public bool showExtensionsList;

        public float rowHeightMultiplier = 1.1f;
        public int previewChunkSize = 20;
        public int mediaHeight = 350;
        public int mediaThumbnailWidth = 120;
        public int mediaThumbnailHeight = 75;
        public int currency; // 0 - EUR, 1 - USD, 2 - CYN
        public int packageTileSize = 150;
        public int noPackageTileTextBelow = 110;
        public int tagListHeight = 250;
        public bool enlargeTiles = true;
        public bool centerTiles;
        public int[] visiblePackageTreeColumns;

        public bool showSearchFilterBar;
        public bool showSearchDetailsBar = true;
        public bool filterOnlyIfBarVisible;
        public bool showPackageFilterBar;
        public bool expandPackageDetails;
        public bool showDetailFilters = true;
        public bool showSavedSearches = true;
        public bool showIndexLocations = true;
        public bool showIndexingSettings;
        public bool showImportSettings;
        public bool showBackupSettings;
        public bool showAISettings;
        public bool showPreviewSettings;
        public bool showAdvancedSettings;
        public bool showHints = true;
        public int packageViewMode; // 0 = list, 1 = grid
        public int packageSearchMode; // 0 = name, 1 = name & description
        public bool showPackageStatsDetails;
        public bool onlyInProject;
        public int projectDetailTabsMode; // 0 = tabs, 1 = list

        public bool indexAssetStore = true;
        public bool indexAssetCache = true;
        public bool indexPackageCache;
        public bool indexAdditionalFolders = true;
        public bool indexAssetManager;
        public bool excludeHidden = true;
        public int assetStoreRefreshCycle = 7; // days
        public int assetCacheLocationType; // 0 = auto, 1 = custom
        public string assetCacheLocation;
        public int packageCacheLocationType; // 0 = auto, 1 = custom
        public string packageCacheLocation;
        public bool downloadAssets = true;
        public bool gatherExtendedMetadata = true;
        public bool extractPreviews = true;
        public bool extractColors;
        public bool extractAudioColors;
        public bool excludeByDefault;
        public bool extractByDefault;
        public bool convertToPipeline;
        public bool scanFBXDependencies = true;
        public bool indexSubPackages = true;
        public bool indexAssetPackageContents = true;
        public bool showIconsForMissingPreviews = true;
        public bool importPackageKeywordsAsTags;
        public string customStorageLocation;
        public bool showCustomPackageUpdates;
        public bool showIndirectPackageUpdates;
        public bool removeUnresolveableDBFiles;

        public bool createAICaptions;
        public bool logAICaptions;
        public bool aiForPrefabs = true;
        public bool aiForModels;
        public bool aiForImages;
        public int blipType; // 0 - small, 1 = large
        public int blipChunkSize = 1;
        public bool aiContinueOnEmpty;
        public bool aiUseGPU;
        public int aiPause;

        public bool upscalePreviews = true;
        public bool upscaleLossless = true;
        public int upscaleSize = 256;

        public bool hideAdvanced = true;
        public bool useCooldown = true;
        public int cooldownInterval = 20; // minutes
        public int cooldownDuration = 20; // seconds
        public int reportingBatchSize = 500;
        public long memoryLimit = (1024 * 1024) * 1000; // every X megabytes
        public int logAreas = LOG_IMAGE_RESIZING | LOG_AUDIO_PARSING | LOG_MEDIA_DOWNLOADS | LOG_PACKAGE_PARSING;
        public int dbOptimizationPeriod = 30; // days
        public int dbOptimizationReminderPeriod = 1; // days
        public string dbJournalMode = "WAL"; // DELETE is an alternative for better compatibility while WAL is faster

        public bool createBackups;
        public bool backupByDefault;
        public bool onlyLatestPatchVersion = true;
        public int backupsPerAsset = 5;
        public string backupFolder;
        public string cacheFolder;
        public string exportFolder;
        public string exportFolder2;
        public string exportFolder3;

        public int importStructure = 1;
        public int importDestination = 2;
        public string importFolder = "Assets/ThirdParty";

        public int assetSorting;
        public bool sortAssetsDescending;
        public int assetGrouping;
        public int assetDeprecation;
        public int assetSRPs;
        public int packagesListing = 1; // only assets per default
        public int maxConcurrentUnityRequests = 10;
        public int observationSpeed = 5;
        public bool autoRefreshMetadata = true;
        public int metadataTimeout = 12; // in hours
        public bool autoStopObservation = true;
        public int observationTimeout = 10; // in seconds

        // non-preferences for convenience
        public int tab;
        public ulong statsImports;

        public List<FolderSpec> folders = new List<FolderSpec>();
        public List<SavedSearch> searches = new List<SavedSearch>();

        // log helpers
        public bool LogMediaDownloads => (logAreas & LOG_MEDIA_DOWNLOADS) != 0;
        public bool LogImageExtraction => (logAreas & LOG_IMAGE_RESIZING) != 0;
        public bool LogAudioParsing => (logAreas & LOG_AUDIO_PARSING) != 0;
        public bool LogPackageParsing => (logAreas & LOG_PACKAGE_PARSING) != 0;

        // UI customization
        public HashSet<string> advancedUI;

        public AssetInventorySettings()
        {
            ResetAdvancedUI();
        }

        public void ResetAdvancedUI()
        {
            // list of UI elements that should be hidden by default
            advancedUI = new HashSet<string>
            {
                "settings.actions.clearcache",
                "settings.actions.cleardb",
                "settings.actions.resetconfig",
                "settings.actions.resetuiconfig",
                "settings.actions.closedb",
                "settings.actions.openassetcache",
                "settings.actions.openpackagecache",
                "settings.actions.dblocation",
                "package.category",
                "package.childcount",
                "package.exclude",
                "package.extract",
                "package.indexedfiles",
                "package.price",
                "package.purchasedate",
                "package.releasedate",
                "package.srps",
                "package.unityversions",
                "package.actions.openinpackagemanager",
                "package.actions.reindexnextrun",
                "package.actions.recreatemissingpreviews",
                "package.actions.recreateimagepreviews",
                "package.actions.recreateallpreviews",
                "package.actions.delete",
                "package.actions.openlocation",
                "package.actions.refreshmetadata",
                "package.actions.export",
                "package.actions.deletefile",
                "package.actions.nameonly",
                "package.actions.reindexnow",
                "package.actions.removeassetstoreconnection",
                "asset.actions.openexplorer",
                "asset.actions.delete",
                "asset.bulk.actions.delete",
                "asset.bulk.actions.openexplorer",
                "package.bulk.actions.refreshmetadata",
                "package.bulk.actions.delete",
                "package.bulk.actions.deletefile",
                "package.bulk.actions.openlocation",
                "search.actions.sidebar"
            };
        }
    }
}