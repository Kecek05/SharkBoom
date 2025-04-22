using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.PackageManager;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetInventory
{
    public partial class IndexUI : BasicEditorUI
    {
        private const float CHECK_INTERVAL = 5;

        private readonly Dictionary<string, string> _staticPreviews = new Dictionary<string, string>
        {
            {"cs", "cs Script Icon"},
            {"php", "TextAsset Icon"},
            {"cg", "TextAsset Icon"},
            {"cginc", "TextAsset Icon"},
            {"js", "d_Js Script Icon"},
            {"prefab", "d_Prefab Icon"},
            {"png", "d_RawImage Icon"},
            {"jpg", "d_RawImage Icon"},
            {"gif", "d_RawImage Icon"},
            {"tga", "d_RawImage Icon"},
            {"tiff", "d_RawImage Icon"},
            {"ico", "d_RawImage Icon"},
            {"bmp", "d_RawImage Icon"},
            {"fbx", "d_PrefabModel Icon"},
            {"dll", "dll Script Icon"},
            {"meta", "MetaFile Icon"},
            {"unity", "d_SceneAsset Icon"},
            {"asset", "EditorSettings Icon"},
            {"txt", "TextScriptImporter Icon"},
            {"md", "TextScriptImporter Icon"},
            {"doc", "TextScriptImporter Icon"},
            {"docx", "TextScriptImporter Icon"},
            {"pdf", "TextScriptImporter Icon"},
            {"rtf", "TextScriptImporter Icon"},
            {"readme", "TextScriptImporter Icon"},
            {"chm", "TextScriptImporter Icon"},
            {"compute", "ComputeShader Icon"},
            {"shader", "Shader Icon"},
            {"shadergraph", "Shader Icon"},
            {"shadersubgraph", "Shader Icon"},
            {"mat", "d_Material Icon"},
            {"wav", "AudioImporter Icon"},
            {"mp3", "AudioImporter Icon"},
            {"ogg", "AudioImporter Icon"},
            {"xml", "UxmlScript Icon"},
            {"html", "UxmlScript Icon"},
            {"uss", "UssScript Icon"},
            {"css", "StyleSheet Icon"},
            {"json", "StyleSheet Icon"},
            {"exr", "d_ReflectionProbe Icon"}
        };

        private enum ChangeImpact
        {
            None,
            ReadOnly,
            Write
        }

        private List<Tag> _tags;
        private string[] _assetNames;
        private string[] _tagNames;
        private string[] _publisherNames;
        private string[] _colorOptions;
        private string[] _categoryNames;
        private string[] _types;
        private string[] _resultSizes;
        private string[] _sortFields;
        private string[] _searchFields;
        private string[] _tileTitle;
        private string[] _dependencyOptions;
        private string[] _previewOptions;
        private string[] _doubleClickOptions;
        private string[] _packageSortOptions;
        private string[] _groupByOptions;
        private string[] _packageListingOptions;
        private string[] _imageTypeOptions;
        private GUIContent[] _packageListingOptionsShort;
        private GUIContent[] _packageViewOptions;
        private GUIContent[] _packageDetailsViewOptions;
        private string[] _deprecationOptions;
        private string[] _srpOptions;
        private string[] _maintenanceOptions;
        private string[] _importDestinationOptions;
        private string[] _importStructureOptions;
        private string[] _assetCacheLocationOptions;
        private string[] _expertSearchFields;
        private string[] _currencyOptions;
        private string[] _logOptions;
        private string[] _blipOptions;
        private string[] _gpuOptions;

        private int _lastTab = -1;
        private string _newTag;
        private int _lastMainProgress;
        private string _importFolder;
        private bool _previewInProgress;

        private string[] _pvSelection;
        private string _pvSelectedPath;
        private string _pvSelectedFolder;
        private bool _pvSelectionChanged;
        private List<AssetInfo> _pvSelectedAssets;
        private int _selectedFolderIndex = -1;
        private int _packageCount;
        private int _packageFileCount;
        private int _availablePackageUpdates;
        private int _activePackageDownloads;

        private AssetPurchases _purchasedAssets = new AssetPurchases();
        private int _purchasedAssetsCount;
        private List<AssetInfo> _assets;
        private int _indexedPackageCount;
        private int _indexablePackageCount;
        private int _aiPackageCount;

        private static int _scriptsReloaded;
        private bool _requireAssetTreeRebuild;
        private bool _requireReportTreeRebuild;
        private ChangeImpact _requireLookupUpdate;
        private bool _requireSearchUpdate;
        private DateTime _lastCheck;
        private Rect _tagButtonRect;
        private Rect _tag2ButtonRect;
        private Rect _connectButtonRect;
        private bool _initDone;
        private bool _updateAvailable;
        private AssetDetails _onlineInfo;
        private bool _allowLogic;

        [MenuItem("Assets/Asset Inventory", priority = 9000)]
        [MenuItem("Window/Asset Management/Asset Inventory", priority = 9000)]
        public static void ShowWindow()
        {
            IndexUI window = GetWindow<IndexUI>("Asset Inventory");
            window.minSize = new Vector2(650, 300);
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (_initDone) return;
            _initDone = true;

            _fixedSearchTypeIdx = -1;
            AI.Init();
            InitFolderControl();

            _previewInProgress = false;
            AssetProgress.ResetState(true);

            _requireLookupUpdate = ChangeImpact.ReadOnly;
            _requireSearchUpdate = true;
            _requireAssetTreeRebuild = true;

            CheckForUpdates();
        }

        private void OnEnable()
        {
            EditorApplication.update += UpdateLoop;
            AI.OnIndexingDone += OnIndexingDone;
            AI.OnPackageImageLoaded += OnPackageImageLoaded;
            AI.OnPackagesUpdated += OnPackagesUpdated;
            Tagging.OnTagsChanged += OnTagsChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneLoaded;
            ImportUI.OnImportDone += OnImportDone;
            MaintenanceUI.OnMaintenanceDone += OnMaintenanceDone;
            AssetStore.OnPackageListUpdated += OnPackageListUpdated;
            AssetDatabase.importPackageCompleted += ImportCompleted;
            AssetDownloaderUtils.OnDownloadFinished += OnDownloadFinished;
#if UNITY_2020_1_OR_NEWER
            Events.registeredPackages += OnRegisteredPackages;
#endif
#if UNITY_2021_2_OR_NEWER
            DragAndDrop.AddDropHandler(OnSceneDrop);
            DragAndDrop.AddDropHandler(OnHierarchyDrop);
            DragAndDrop.AddDropHandler(OnProjectBrowserDrop);
            DragAndDrop.AddDropHandler(OnInspectorDrop);
#endif
            _initDone = false;

            AI.StopAudio();
            AssetStore.FillBufferOnDemand(true);
            if (!searchMode) SuggestOptimization();

            // have to go through preliminary title as OnEnable is called before setting any additional properties
            if (!titleContent.text.Contains("Picker")) AI.StartCacheObserver(); // expensive operation, only do when UI is visible
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateLoop;
            AI.OnIndexingDone -= OnIndexingDone;
            AI.OnPackageImageLoaded -= OnPackageImageLoaded;
            AI.OnPackagesUpdated -= OnPackagesUpdated;
            Tagging.OnTagsChanged -= OnTagsChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened -= OnSceneLoaded;
            ImportUI.OnImportDone -= OnImportDone;
            MaintenanceUI.OnMaintenanceDone -= OnMaintenanceDone;
            AssetStore.OnPackageListUpdated -= OnPackageListUpdated;
            AssetDatabase.importPackageCompleted -= ImportCompleted;
            AssetDownloaderUtils.OnDownloadFinished -= OnDownloadFinished;
#if UNITY_2020_1_OR_NEWER
            Events.registeredPackages -= OnRegisteredPackages;
#endif
#if UNITY_2021_2_OR_NEWER
            DragAndDrop.RemoveDropHandler(OnSceneDrop);
            DragAndDrop.RemoveDropHandler(OnHierarchyDrop);
            DragAndDrop.RemoveDropHandler(OnProjectBrowserDrop);
            DragAndDrop.RemoveDropHandler(OnInspectorDrop);
#endif
            AI.StopAudio();
            AI.StopCacheObserver();
        }

        private void UpdateLoop()
        {
            SearchUpdateLoop();
        }

        private void SuggestOptimization()
        {
            // check if last optimization (stored as "yyyy-MM-dd HH:mm:ss" string) was more than a month ago
            AppProperty lastOptimization = DBAdapter.DB.Find<AppProperty>("LastOptimization");
            if (lastOptimization == null || string.IsNullOrWhiteSpace(lastOptimization.Value) || !DateTime.TryParse(lastOptimization.Value, out DateTime lastOpt))
            {
                OptimizeDatabase(true);
                return;
            }
            if ((DateTime.Now - lastOpt).TotalDays < AI.Config.dbOptimizationPeriod) return;

            // check if last optimization request (stored as "yyyy-MM-dd HH:mm:ss" string) was more than a day ago
            AppProperty lastOptRequest = DBAdapter.DB.Find<AppProperty>("LastOptimizationRequest");
            if (lastOptRequest == null || (DateTime.TryParse(lastOptRequest.Value, out DateTime lastOptReq) && (DateTime.Now - lastOptReq).TotalDays > AI.Config.dbOptimizationReminderPeriod))
            {
                lastOptRequest = new AppProperty("LastOptimizationRequest", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                DBAdapter.DB.InsertOrReplace(lastOptRequest);

                if (EditorUtility.DisplayDialog("Database Optimization", "It is recommended to optimize the database regularly to ensure fast search results. Should it be done now?", "OK", "Not Now"))
                {
                    OptimizeDatabase();
                }
            }
        }

        private void OnPackagesUpdated()
        {
            _requireLookupUpdate = ChangeImpact.Write;
            _requireSearchUpdate = true;
            _requireAssetTreeRebuild = true;
        }

        private void OnMaintenanceDone()
        {
            _requireLookupUpdate = ChangeImpact.Write;
            _requireSearchUpdate = true;
            _requireAssetTreeRebuild = true;
        }

        private void OnDownloadFinished(int foreignId)
        {
            _requireAssetTreeRebuild = true;
            if (AI.Config.tab == 0 && _selectedEntry != null && _selectedEntry.ForeignId == foreignId)
            {
                _selectedEntry.Refresh();
                _selectedEntry.PackageDownloader?.RefreshState();
            }
        }

        private async void OnPackageImageLoaded(Asset asset)
        {
            AssetInfo info = _assets?.FirstOrDefault(a => a.Id == asset.Id);
            if (info == null) return;

            await AssetUtils.LoadPackageTexture(info);
            _requireAssetTreeRebuild = true;
        }

        private void OnSceneLoaded(Scene scene, OpenSceneMode mode)
        {
            // otherwise previews will be empty
            _requireSearchUpdate = true;
            _requireAssetTreeRebuild = true;
        }

        private void ImportCompleted(string packageName)
        {
            OnImportDone();
        }

#if UNITY_2020_1_OR_NEWER
        private void OnRegisteredPackages(PackageRegistrationEventArgs obj)
        {
            OnImportDone();
        }
#endif

        private void OnImportDone()
        {
            AssetStore.GatherProjectMetadata();

            _requireLookupUpdate = ChangeImpact.ReadOnly;
            _requireAssetTreeRebuild = true;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            AI.StopAudio();

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // will crash editor otherwise
                _textureLoading?.Cancel();
                _textureLoading2?.Cancel();
                _textureLoading3?.Cancel();
            }

            // UI will have lost all preview textures during play mode
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _requireSearchUpdate = true;
                _requireAssetTreeRebuild = true;
            }
        }

        private void ReloadLookups(bool force = true)
        {
            if (AI.DEBUG_MODE) Debug.LogWarning("Reload Lookups");

            _requireLookupUpdate = ChangeImpact.None;
            _resultSizes = new[] {"-all-", string.Empty, "10", "25", "50", "100", "250", "500", "1000", "1500", "2000", "2500", "3000", "4000", "5000"};
            _searchFields = new[] {"Asset Path", "File Name", "AI Caption"};
            _sortFields = new[] {"Asset Path", "File Name", "Size", "Type", "Length", "Width", "Height", "Color", "Category", "Last Updated", "Rating", "#Reviews", string.Empty, "-unsorted (fast)-"};
            _packageSortOptions = Enum.GetNames(typeof (AssetTreeViewControl.Columns)).Select(StringUtils.CamelCaseToWords).ToArray();
            _groupByOptions = new[] {"-none-", string.Empty, "Category", "Publisher", "Tag", "State", "Location"};
            _colorOptions = new[] {"-all-", "matching"};
            _tileTitle = new[] {"-Intelligent-", string.Empty, "Asset Path", "File Name", "File Name without Extension", "AI Caption", string.Empty, "None"};
            _dependencyOptions = new[] {"Never", "Upon Selection"};
            _previewOptions = new[] {"-all-", string.Empty, "Only With Preview", "Only Without Preview"};
            _doubleClickOptions = new[] {"-none-", "Import", "Open"};
            _packageListingOptions = new[] {"-all-", "-all except registry packages-", "Only Asset Store Packages", "Only Registry Packages", "Only Custom Packages", "Only Media Folders", "Only Archives", "Only Asset Manager"};
            _packageListingOptionsShort = new[] {new GUIContent("All", ""), new GUIContent("No Reg", _packageListingOptions[1]), new GUIContent("Store", _packageListingOptions[2]), new GUIContent("Reg", _packageListingOptions[3]), new GUIContent("Cust", _packageListingOptions[4]), new GUIContent("Media", _packageListingOptions[5]), new GUIContent("Arch", _packageListingOptions[6]), new GUIContent("AM", _packageListingOptions[7])};
            _packageViewOptions = new[] {EditorGUIUtility.IconContent("d_VerticalLayoutGroup Icon", "|List"), EditorGUIUtility.IconContent("d_GridLayoutGroup Icon", "|Grid")};
            _packageDetailsViewOptions = new[] {EditorGUIUtility.IconContent("d_GridLayoutGroup Icon", "|Tabs"), EditorGUIUtility.IconContent("d_VerticalLayoutGroup Icon", "|List")};
            _deprecationOptions = new[] {"-all-", "Exclude Deprecated", "Show Only Deprecated"};
            _srpOptions = new[] {"-all-", "BIRP", "URP", "HDRP"};
            _maintenanceOptions = new[] {"-all-", "Update Available", "Outdated in Unity Cache", "Disabled by Unity", "Custom Asset Store Link", "Indexed", "Not Indexed", "Custom Registry", "Downloaded", "Downloading", "Not Downloaded", "Duplicate", "Marked for Backup", "Not Marked for Backup", "Deleted", "Excluded", "With Sub-Packages", "Incompatible Packages", "Fixable Incompatibilities", "Unfixable Incompatibilities"};
            _importDestinationOptions = new[] {"Into Folder Selected in Project View", "Into Assets Root", "Into Specific Folder"};
            _importStructureOptions = new[] {"All Files Flat in Target Folder", "Keep Original Folder Structure"};
            _assetCacheLocationOptions = new[] {"Automatic", "Custom Folder"};
            _currencyOptions = new[] {"EUR", "USD", "CNY"};
            _logOptions = new[] {"Media Downloads", "Image Resizing", "Audio Parsing", "Package Parsing"};
            _blipOptions = new[] {"Small (1Gb)", "Large (1.8Gb)"};
            _gpuOptions = new[] {"-none-", string.Empty, "0", "1", "2"};
            _imageTypeOptions = new List<string> {"-all-", string.Empty}.Concat(TextureNameSuggester.suffixPatterns.Keys.Select(StringUtils.CamelCaseToWords)).ToArray();
            _expertSearchFields = new[]
            {
                "-Add Field-", string.Empty,
                "Asset/AssetRating", "Asset/AssetSource", "Asset/Backup", "Asset/BIRPCompatible", "Asset/CompatibilityInfo", "Asset/CurrentState", "Asset/CurrentSubState", "Asset/Description", "Asset/DisplayCategory", "Asset/DisplayName", "Asset/DisplayPublisher", "Asset/ETag", "Asset/Exclude",
                "Asset/FirstRelease", "Asset/ForeignId", "Asset/HDRPCompatible", "Asset/Hotness", "Asset/Hue", "Asset/Id", "Asset/IsHidden", "Asset/IsLatestVersion", "Asset/KeepExtracted", "Asset/KeyFeatures", "Asset/Keywords", "Asset/LastOnlineRefresh", "Asset/LastRelease", "Asset/LatestVersion",
                "Asset/License", "Asset/LicenseLocation", "Asset/Location", "Asset/OriginalLocation", "Asset/OriginalLocationKey", "Asset/PackageDependencies", "Asset/PackageSize", "Asset/PackageSource", "Asset/ParentId", "Asset/PriceCny", "Asset/PriceEur", "Asset/PriceUsd",
                "Asset/PublisherId", "Asset/PurchaseDate", "Asset/RatingCount", "Asset/Registry", "Asset/ReleaseNotes", "Asset/Repository", "Asset/Requirements", "Asset/Revision", "Asset/SafeCategory", "Asset/SafeName",
                "Asset/SafePublisher", "Asset/Slug", "Asset/SupportedUnityVersions", "Asset/UpdateStrategy", "Asset/UploadId", "Asset/URPCompatible", "Asset/UseAI", "Asset/Version",
                "AssetFile/AssetId", "AssetFile/FileName", "AssetFile/FileVersion", "AssetFile/FileStatus", "AssetFile/Guid", "AssetFile/Height", "AssetFile/Hue", "AssetFile/Id", "AssetFile/Length", "AssetFile/Path", "AssetFile/PreviewState", "AssetFile/Size", "AssetFile/SourcePath", "AssetFile/Type", "AssetFile/Width",
                "Tag/Color", "Tag/FromAssetStore", "Tag/Id", "Tag/Name",
                "TagAssignment/Id", "TagAssignment/TagId", "TagAssignment/TagTarget", "TagAssignment/TagTargetId"
            };

            UpdateStatistics(force);
            AssetStore.FillBufferOnDemand();

            _assetNames = AI.ExtractAssetNames(_assets, true);
            _publisherNames = AI.ExtractPublisherNames(_assets);
            _categoryNames = AI.ExtractCategoryNames(_assets);
            _tagNames = AI.ExtractTagNames(_tags);
            _purchasedAssetsCount = AI.CountPurchasedAssets(_assets);

            _types = AI.LoadTypes();
            if (!string.IsNullOrWhiteSpace(fixedSearchType))
            {
                _fixedSearchTypeIdx = Array.IndexOf(_types, fixedSearchType);
            }
        }

        [DidReloadScripts(2)]
        private static void DidReloadScripts()
        {
            _scriptsReloaded++;
        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (_scriptsReloaded > 0)
            {
                _requireAssetTreeRebuild = true;
                _requireReportTreeRebuild = true;
                _requireSearchUpdate = true;
                _scriptsReloaded--;
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("The Asset Inventory is not available during play mode.", MessageType.Info);
                return;
            }

            _allowLogic = Event.current.type == EventType.Layout; // nothing must be changed during repaint
            Init(); // in some docking scenarios OnGUI is called before Awake

            // check for config errors
            if (AI.ConfigErrors.Count > 0)
            {
                EditorGUILayout.HelpBox("Configuration errors detected. These need to be fixed to proceed.", MessageType.Error);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Config Location: {AI.UsedConfigLocation}");
                if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                {
                    EditorUtility.RevealInFinder(AI.UsedConfigLocation);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Errors", EditorStyles.boldLabel);
                foreach (string error in AI.ConfigErrors)
                {
                    EditorGUILayout.LabelField($"--> {error}");
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Reload Settings", GUILayout.Height(30), GUILayout.ExpandWidth(false)))
                {
                    AI.ReInit();
                }
                return;
            }

            if (UpgradeUtil.LongUpgradeRequired)
            {
                UpgradeUtil.DrawUpgradeRequired();
                return;
            }

            // determine import targets
            switch (AI.Config.importDestination)
            {
                case 0:
                    _importFolder = _pvSelectedFolder;
                    break;

                case 1:
                    _importFolder = "Assets";
                    break;

                case 2:
                    _importFolder = AI.Config.importFolder;
                    break;
            }

            if (Event.current.type == EventType.Repaint) _mouseOverSearchResultRect = false;
            if (DragDropAvailable()) HandleDragDrop();

            if (_requireLookupUpdate != ChangeImpact.None || _resultSizes == null || _resultSizes.Length == 0)
            {
                ReloadLookups(_requireLookupUpdate == ChangeImpact.Write || _requireLookupUpdate == ChangeImpact.None);
            }
            if (_allowLogic)
            {
                if (_lastTileSizeChange != DateTime.MinValue && (DateTime.Now - _lastTileSizeChange).TotalMilliseconds > 300f)
                {
                    _requireSearchUpdate = true;
                    _lastTileSizeChange = DateTime.MinValue;
                }

                // don't perform more expensive checks every frame
                if ((DateTime.Now - _lastCheck).TotalSeconds > CHECK_INTERVAL)
                {
                    _availablePackageUpdates = _assets.Count(a => a.ParentId == 0 && a.IsUpdateAvailable(_assets, false));
                    _activePackageDownloads = AI.GetObserver().DownloadCount;
                    _lastCheck = DateTime.Now;
                }
            }

            bool isNewTab = false;
            if (!searchMode)
            {
                isNewTab = DrawToolbar();
                if (isNewTab) AI.StopAudio();
                EditorGUILayout.Space();
            }
            else
            {
                AI.Config.tab = 0;
            }

            if (string.IsNullOrEmpty(CloudProjectSettings.accessToken))
            {
                EditorGUILayout.HelpBox("Asset Store connectivity is currently not possible. Please restart Unity and make sure you are logged in in the Unity hub.", MessageType.Warning);
                EditorGUILayout.Space();
            }

            // centrally handle project view selections since used in multiple views
            CheckProjectViewSelection();
            switch (AI.Config.tab)
            {
                case 0:
                    if (_allowLogic && _requireSearchUpdate && AI.Config.searchAutomatically) PerformSearch(_keepSearchResultPage);
                    DrawSearchTab();
                    break;

                case 1:
                    // will have lost asset tree on reload due to missing serialization
                    if (_requireAssetTreeRebuild) CreateAssetTree();
                    DrawPackagesTab();
                    break;

                case 2:
                    if (_requireReportTreeRebuild) CreateReportTree();
                    DrawReportingTab();
                    break;

                case 3:
                    if (isNewTab) EditorCoroutineUtility.StartCoroutineOwnerless(UpdateStatisticsDelayed());
                    DrawSettingsTab();
                    break;

                case 4:
                    DrawAboutTab();
                    break;
            }

            // reload if there is new data
            if (!AssetProgress.ReadOnly && _lastMainProgress != AssetProgress.MainProgress)
            {
                _lastMainProgress = AssetProgress.MainProgress;
                _requireLookupUpdate = ChangeImpact.Write;
                _requireSearchUpdate = true;
            }

            if (_allowLogic)
            {
                // handle double-clicks
                if (Event.current.clickCount > 1)
                {
                    if (_mouseOverSearchResultRect && (searchMode || AI.Config.doubleClickBehavior > 0) && _selectedEntry != null)
                    {
                        if (searchMode)
                        {
                            ExecuteSingleAction();
                        }
                        else
                        {
                            if ((AI.Config.doubleClickBehavior == 1 && !Event.current.alt) || (AI.Config.doubleClickBehavior == 2 && Event.current.alt))
                            {
                                _ = PerformCopyTo(_selectedEntry, _importFolder);
                            }
                            else
                            {
                                Open(_selectedEntry);
                            }
                        }
                    }
                }
            }
        }

        private void CheckProjectViewSelection()
        {
            if (_pvSelection != null && Selection.assetGUIDs != null && _pvSelection.SequenceEqual(Selection.assetGUIDs))
            {
                _pvSelectionChanged = false;
                return;
            }

            _pvSelection = Selection.assetGUIDs;
            string oldPvSelectedPath = _pvSelectedPath;
            _pvSelectedPath = null;
            if (_pvSelection != null && _pvSelection.Length > 0)
            {
                _pvSelectedPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
                if (_pvSelectedPath.StartsWith("Packages"))
                {
                    _pvSelectedPath = null;
                    _pvSelectedFolder = null;
                }
                else
                {
                    _pvSelectedFolder = Directory.Exists(_pvSelectedPath) ? _pvSelectedPath : Path.GetDirectoryName(_pvSelectedPath);
                    if (!string.IsNullOrWhiteSpace(_pvSelectedFolder)) _pvSelectedFolder = _pvSelectedFolder.Replace('/', Path.DirectorySeparatorChar);
                }
            }
            _pvSelectionChanged = oldPvSelectedPath != _pvSelectedPath;
            if (_pvSelectionChanged) _pvSelectedAssets = null;
        }

        private bool DrawToolbar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            List<string> strings = new List<string>
            {
                "Search",
                "Packages",
                "Reporting",
                "Settings" + (AI.CurrentMain != null || AI.IndexingInProgress ? " (indexing)" : "")
            };
            AI.Config.tab = GUILayout.Toolbar(AI.Config.tab, strings.ToArray(), GUILayout.Height(32), GUILayout.MinWidth(500));

            bool newTab = EditorGUI.EndChangeCheck();
            if (newTab) AI.SaveConfig();

            GUILayout.FlexibleSpace();
            if (_updateAvailable && _onlineInfo != null && GUILayout.Button(UIStyles.Content($"v{_onlineInfo.version.name} available!", $"Released {_onlineInfo.version.publishedDate}"), EditorStyles.linkLabel))
            {
                Application.OpenURL(AI.ASSET_STORE_LINK);
            }
            if (_activePackageDownloads > 0 && GUILayout.Button(EditorGUIUtility.IconContent("Loading", $"|{_activePackageDownloads} Downloads Active"), EditorStyles.label))
            {
                AI.Config.tab = 1;
                _selectedMaintenance = 9;
                _requireAssetTreeRebuild = true;
                AI.Config.showPackageFilterBar = true;
                AI.SaveConfig();
            }
            if (_availablePackageUpdates > 0 && GUILayout.Button(EditorGUIUtility.IconContent("Update-Available", $"|{_availablePackageUpdates} Updates Available"), EditorStyles.label))
            {
                AI.Config.tab = 1;
                _selectedMaintenance = 1;
                _requireAssetTreeRebuild = true;
                AI.Config.showPackageFilterBar = true;
                AI.SaveConfig();
            }
            UILine("toolbar.toggleadvanced", () =>
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(AI.Config.hideAdvanced ? "d_animationvisibilitytoggleoff" : "d_animationvisibilitytoggleon", "|Visibility of Advanced Features" + (AI.Config.hideAdvanced ? " - Hold CTRL to show temporarily" : "")), EditorStyles.label))
                {
                    AI.Config.hideAdvanced = !AI.Config.hideAdvanced;
                    AI.SaveConfig();
                }
            });
            UILine("toolbar.togglecustomization", () =>
            {
                Color curCol = GUI.color;
                if (AI.UICustomizationMode) GUI.color = Color.green;
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_CustomTool", "|Toggle UI Customization"), EditorStyles.label))
                {
                    AI.UICustomizationMode = !AI.UICustomizationMode;
                }
                GUI.color = curCol;
            });
            UILine("toolbar.toggleabout", () =>
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("_Help", "|About"), EditorStyles.label))
                {
                    if (_lastTab >= 0)
                    {
                        AI.Config.tab = _lastTab;
                    }
                    else
                    {
                        _lastTab = AI.Config.tab;
                        AI.Config.tab = 4;
                    }
                }
            });
            if (AI.Config.tab < 4) _lastTab = -1;
            GUILayout.EndHorizontal();

            return newTab;
        }

        private void ShowInterstitial()
        {
            if (EditorUtility.DisplayDialog("Your Support Counts", "This message will only appear once. Thanks for using Asset Inventory! I hope you enjoy using it.\n\n" +
                    "Developing a rather ground-braking asset like this as a solo-dev requires a huge amount of time and work.\n\n" +
                    "Please consider leaving a review and spreading the word. This is so important on the Asset Store and is the only way to make asset development viable.\n\n"
                    , "Leave Review", "Maybe Later"))
            {
                Application.OpenURL(AI.ASSET_STORE_LINK);
            }
        }

        private async void FetchAssetPurchases(bool forceUpdate)
        {
            AssetPurchases result = await AI.FetchOnlineAssets();
            if (AssetStore.CancellationRequested || result == null) return;

            _purchasedAssets = result;
            _purchasedAssetsCount = _purchasedAssets?.total ?? 0;
            ReloadLookups();
            FetchAssetDetails(forceUpdate);
        }

        private async void FetchAssetDetails(bool forceUpdate = false, int assetId = 0, bool skipEvents = false)
        {
            await AI.FetchAssetsDetails(forceUpdate, assetId, assetId > 0, skipEvents);

            // skip in optional update scenarios like when user selects something in the tree to avoid hickups 
            if (!skipEvents)
            {
                ReloadLookups();
                _requireAssetTreeRebuild = true;
            }
        }

        private void GatherTreeChildren(int id, List<AssetInfo> result, TreeModel<AssetInfo> treeModel)
        {
            AssetInfo info = treeModel.Find(id);
            if (info == null) return;

            if (info.Id > 0) result.Add(info);
            if (info.HasChildren)
            {
                foreach (TreeElement subInfo in info.Children)
                {
                    GatherTreeChildren(subInfo.TreeId, result, treeModel);
                }
            }
        }

        private async void CheckForUpdates()
        {
            _updateAvailable = false;

            await Task.Delay(2000); // let remainder of window initialize first
            if (string.IsNullOrEmpty(CloudProjectSettings.accessToken)) return;

            _onlineInfo = await AssetStore.RetrieveAssetDetails(AI.ASSET_STORE_ID, null, true);
            if (_onlineInfo == null) return;

            _updateAvailable = new SemVer(_onlineInfo.version.name) > new SemVer(AI.VERSION);
        }

        private void CreateDebugReport()
        {
            string reportFile = Path.Combine(AI.GetStorageFolder(), "DebugReport.log");
            File.WriteAllText(reportFile, AI.CreateDebugReport());
            EditorUtility.RevealInFinder(reportFile);
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}