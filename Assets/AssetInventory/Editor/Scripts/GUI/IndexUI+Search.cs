using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if !ASSET_INVENTORY_NOAUDIO
using JD.EditorAudioUtils;
#endif
using SQLite;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace AssetInventory
{
    public partial class IndexUI
    {
        // customizable interaction modes, search mode will only show search tab contents and no actions except "Select"
        public bool searchMode;

        // special mode that will return accompanying textures to the selected one, trying to identify normal, metallic etc. 
        public bool textureMode;

        // will hide detail pane
        public bool hideDetailsPane;

        // will not select items in the project window upon selection
        public bool disablePings;

        // will cause clicking on a grid tile to return the selection to the caller and close the window
        public bool instantSelection;

        // locks the search to a specific type, e.g. "Prefabs" 
        public string fixedSearchType;

        // event handler during search mode
        protected Action<string> searchModeCallback;
        protected Action<Dictionary<string, string>> searchModeTextureCallback;

        private List<AssetInfo> _files;

        private readonly GridControl _sgrid = new GridControl();
        private int _resultCount;
        private string _searchPhrase;
        private string _searchWidth;
        private string _searchHeight;
        private string _searchLength;
        private string _searchSize;
        private bool _checkMaxWidth;
        private bool _checkMaxHeight;
        private bool _checkMaxLength;
        private bool _checkMaxSize;
        private int _selectedPublisher;
        private int _selectedCategory;
        private int _selectedExpertSearchField;
        private int _selectedAsset;
        private int _selectedPackageTypes = 1;
        private int _selectedPackageSRPs;
        private int _selectedImageType;
        private int _selectedPackageTag;
        private int _selectedFileTag;
        private int _selectedMaintenance;
        private int _selectedColorOption;
        private Color _selectedColor;
        private bool _showSettings;

        private Vector2 _searchScrollPos;
        private Vector2 _inspectorScrollPos;

        private int _curPage = 1;
        private int _pageCount;

        private CancellationTokenSource _textureLoading;
        private CancellationTokenSource _textureLoading2;
        private CancellationTokenSource _textureLoading3;

        private AssetInfo _selectedEntry;

        private SearchField SearchField => _searchField = _searchField ?? new SearchField();
        private SearchField _searchField;

        private float _nextSearchTime;
        private Rect _pageButtonRect;
        private DateTime _lastTileSizeChange;
        private string _searchError;
        private bool _searchDone;
        private bool _lockSelection;
        private string _curOperation;
        private int _fixedSearchTypeIdx;
        private bool _mouseOverSearchResultRect;
        private bool _draggingPossible;
        private bool _dragging;
        private bool _keepSearchResultPage = true;
        private readonly Dictionary<string, Tuple<int, Color>> _assetFileBulkTags = new Dictionary<string, Tuple<int, Color>>();
        private int _assetFileAMProjectCount;
        private int _assetFileAMCollectionCount;
        private Texture2D _animTexture;
        private List<Rect> _animFrames;
        private int _curAnimFrame;
        private float _nextAnimTime;

        protected void SetInitialSearch(string searchPhrase)
        {
            _searchPhrase = searchPhrase;
        }

        private void DrawSearchTab()
        {
            if (_packageFileCount == 0)
            {
                _searchScrollPos = GUILayout.BeginScrollView(_searchScrollPos, false, false);
                bool canStillSearch = AI.IndexingInProgress || _packageCount == 0 || AI.Config.indexAssetPackageContents;
                if (canStillSearch)
                {
                    EditorGUILayout.HelpBox("The search index needs to be initialized. Start it right from here or go to the Settings tab to configure the details.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("The search is only available if package contents was indexed.", MessageType.Info);
                }

                EditorGUILayout.Space(30);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(Logo, EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (canStillSearch)
                {
                    EditorGUILayout.Space(30);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical();
                    EditorGUI.BeginDisabledGroup(AI.IndexingInProgress);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(AI.IndexingInProgress ? "Indexing..." : "Start Indexing", GUILayout.Height(50), GUILayout.MaxWidth(400))) PerformFullUpdate();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Settings...", GUILayout.ExpandWidth(false))) SetupWizardUI.ShowWindow();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();
                    if (AI.IndexingInProgress)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Index results will appear here automatically once available. To see the detailed progress go to the Settings tab.", EditorStyles.centeredGreyMiniLabel);
                    }
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox("Since the search index is shared across Unity projects it is highly recommended for performance to perform initial indexing from an empty project on a new Unity version and if possible on an SSD drive.", MessageType.Warning);
                }
                GUILayout.EndScrollView();
            }
            else if (_lockSelection)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Making asset available in project...", UIStyles.centerLabel);
                EditorGUILayout.LabelField("This can take a while depending on the size of the source package.", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(30);
                EditorGUILayout.LabelField(_curOperation, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }
            else
            {
                bool dirty = false;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(EditorGUIUtility.IconContent("Preset.Context", "|Show/Hide Search Filters")))
                {
                    AI.Config.showSearchFilterBar = !AI.Config.showSearchFilterBar;
                    AI.SaveConfig();
                    if (AI.Config.filterOnlyIfBarVisible) dirty = true;
                }
                EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
                EditorGUIUtility.labelWidth = 60;
                EditorGUI.BeginChangeCheck();
                _searchPhrase = SearchField.OnGUI(_searchPhrase, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    // delay search to allow fast typing
                    _nextSearchTime = Time.realtimeSinceStartup + AI.Config.searchDelay;
                }
                else if (_nextSearchTime > 0 && Time.realtimeSinceStartup > _nextSearchTime)
                {
                    _nextSearchTime = 0;
                    if (AI.Config.searchAutomatically && !_searchPhrase.StartsWith("=")) dirty = true;
                }
                if (_allowLogic && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
                {
                    PerformSearch();
                }
                if (!AI.Config.searchAutomatically)
                {
                    if (GUILayout.Button("Go", GUILayout.Width(30))) PerformSearch();
                }

                if (_searchPhrase != null && _searchPhrase.StartsWith("="))
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Space(2);
                    _selectedExpertSearchField = EditorGUILayout.Popup(_selectedExpertSearchField, _expertSearchFields, GUILayout.Width(90));
                    if (EditorGUI.EndChangeCheck())
                    {
                        string field = _expertSearchFields[_selectedExpertSearchField];
                        if (!string.IsNullOrEmpty(field) && !field.StartsWith("-"))
                        {
                            _searchPhrase += field.Replace('/', '.');
                            SearchField.SetFocus();
                        }
                        _selectedExpertSearchField = 0;
                    }
                }
                UILine("search.actions.assistant", () =>
                {
                    if (GUILayout.Button(UIStyles.Content("?", "Show example searches"), GUILayout.Width(20)))
                    {
                        AdvancedSearchUI searchUI = AdvancedSearchUI.ShowWindow();
                        searchUI.Init((searchPhrase, searchType) =>
                        {
                            _searchPhrase = searchPhrase;
                            if (searchType == null)
                            {
                                AI.Config.searchType = 0;
                            }
                            else
                            {
                                int typeIdx = Array.IndexOf(_types, searchType);
                                if (typeIdx >= 0) AI.Config.searchType = typeIdx;
                            }
                            _requireSearchUpdate = true;
                        });
                    }
                });
                if (_fixedSearchTypeIdx < 0)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Space(2);
                    AI.Config.searchType = EditorGUILayout.Popup(AI.Config.searchType, _types, GUILayout.ExpandWidth(false), GUILayout.MinWidth(85));
                    if (EditorGUI.EndChangeCheck())
                    {
                        AI.SaveConfig();
                        dirty = true;
                    }
                    GUILayout.Space(2);
                }
                if (!hideDetailsPane && !searchMode)
                {
                    UILine("search.actions.sidebar", () =>
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_animationvisibilitytoggleon", "|Show/Hide Details Inspector")))
                        {
                            AI.Config.showSearchDetailsBar = !AI.Config.showSearchDetailsBar;
                            AI.SaveConfig();
                        }
                    });
                }
                GUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(_searchError))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(90);
                    EditorGUILayout.LabelField($"Error: {_searchError}", UIStyles.ColoredText(Color.red));
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                if (AI.Config.showSearchFilterBar)
                {
                    GUILayout.BeginVertical("Filter Bar", "window", GUILayout.Width(UIStyles.INSPECTOR_WIDTH));
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    AI.Config.showDetailFilters = EditorGUILayout.Foldout(AI.Config.showDetailFilters, "Additional Filters");
                    if (EditorGUI.EndChangeCheck()) AI.SaveConfig();

                    if (AI.Config.showDetailFilters)
                    {
                        EditorGUI.BeginChangeCheck();

                        int labelWidth = 85;

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Package Tag", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        _selectedPackageTag = EditorGUILayout.Popup(_selectedPackageTag, _tagNames, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("File Tag", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        _selectedFileTag = EditorGUILayout.Popup(_selectedFileTag, _tagNames, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Package", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        _selectedAsset = EditorGUILayout.Popup(_selectedAsset, _assetNames, GUILayout.ExpandWidth(true), GUILayout.MinWidth(200));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Publisher", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        _selectedPublisher = EditorGUILayout.Popup(_selectedPublisher, _publisherNames, GUILayout.ExpandWidth(true), GUILayout.MinWidth(150));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Category", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        _selectedCategory = EditorGUILayout.Popup(_selectedCategory, _categoryNames, GUILayout.ExpandWidth(true), GUILayout.MinWidth(150));
                        GUILayout.EndHorizontal();

                        if (IsFilterApplicable("ImageType"))
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Image Type", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                            _selectedImageType = EditorGUILayout.Popup(_selectedImageType, _imageTypeOptions, GUILayout.ExpandWidth(true));
                            GUILayout.EndHorizontal();
                        }

                        if (IsFilterApplicable("Width"))
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Width", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                            if (GUILayout.Button(_checkMaxWidth ? "<=" : ">=", GUILayout.Width(25))) _checkMaxWidth = !_checkMaxWidth;
                            _searchWidth = EditorGUILayout.TextField(_searchWidth, GUILayout.Width(58));
                            EditorGUILayout.LabelField("px", EditorStyles.miniLabel);
                            GUILayout.EndHorizontal();
                        }

                        if (IsFilterApplicable("Height"))
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Height", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                            if (GUILayout.Button(_checkMaxHeight ? "<=" : ">=", GUILayout.Width(25))) _checkMaxHeight = !_checkMaxHeight;
                            _searchHeight = EditorGUILayout.TextField(_searchHeight, GUILayout.Width(58));
                            EditorGUILayout.LabelField("px", EditorStyles.miniLabel);
                            GUILayout.EndHorizontal();
                        }

                        if (IsFilterApplicable("Length"))
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Length", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                            if (GUILayout.Button(_checkMaxLength ? "<=" : ">=", GUILayout.Width(25))) _checkMaxLength = !_checkMaxLength;
                            _searchLength = EditorGUILayout.TextField(_searchLength, GUILayout.Width(58));
                            EditorGUILayout.LabelField("sec", EditorStyles.miniLabel);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("File Size", "File size in kilobytes"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        if (GUILayout.Button(_checkMaxSize ? "<=" : ">=", GUILayout.Width(25))) _checkMaxSize = !_checkMaxSize;
                        _searchSize = EditorGUILayout.TextField(_searchSize, GUILayout.Width(58));
                        EditorGUILayout.LabelField("kb", EditorStyles.miniLabel);
                        GUILayout.EndHorizontal();

                        if (AI.Config.extractColors)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                            _selectedColorOption = EditorGUILayout.Popup(_selectedColorOption, _colorOptions, GUILayout.Width(labelWidth + 2));
                            if (_selectedColorOption > 0) _selectedColor = EditorGUILayout.ColorField(_selectedColor);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Packages", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        _selectedPackageTypes = EditorGUILayout.Popup(_selectedPackageTypes, _packageListingOptions, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("SRPs", EditorStyles.boldLabel, GUILayout.MaxWidth(labelWidth));
                        _selectedPackageSRPs = EditorGUILayout.Popup(_selectedPackageSRPs, _srpOptions, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck()) dirty = true;

                        EditorGUILayout.Space();
                        if (GUILayout.Button("Reset Filters"))
                        {
                            ResetSearch(true, false);
                            _requireSearchUpdate = true;
                        }
                    }

                    UIBlock("asset.actions.savedsearches", () =>
                    {
                        EditorGUILayout.Space();
                        EditorGUI.BeginChangeCheck();
                        AI.Config.showSavedSearches = EditorGUILayout.Foldout(AI.Config.showSavedSearches, "Saved Searches");
                        if (EditorGUI.EndChangeCheck()) AI.SaveConfig();

                        if (AI.Config.showSavedSearches)
                        {
                            if (AI.Config.searches.Count == 0)
                            {
                                EditorGUILayout.HelpBox("Save different search settings to quickly pull up the results later again.", MessageType.Info);
                            }
                            if (GUILayout.Button("Save current search..."))
                            {
                                NameUI nameUI = new NameUI();
                                nameUI.Init(string.IsNullOrEmpty(_searchPhrase) ? "My Search" : _searchPhrase, SaveSearch);
                                PopupWindow.Show(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), nameUI);
                            }

                            EditorGUILayout.Space();
                            Color oldCol = GUI.backgroundColor;
                            for (int i = 0; i < AI.Config.searches.Count; i++)
                            {
                                SavedSearch search = AI.Config.searches[i];
                                GUILayout.BeginHorizontal();

                                if (ColorUtility.TryParseHtmlString($"#{search.color}", out Color color)) GUI.backgroundColor = color;
                                if (GUILayout.Button(UIStyles.Content(search.name, search.searchPhrase), GUILayout.MaxWidth(250))) LoadSearch(search);
                                if (GUILayout.Button(EditorGUIUtility.IconContent("TrueTypeFontImporter Icon", "|Set only search text"), GUILayout.Width(30), GUILayout.Height(EditorGUIUtility.singleLineHeight + 2)))
                                {
                                    _searchPhrase = search.searchPhrase;
                                    dirty = true;
                                }
                                GUI.backgroundColor = oldCol;

                                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash", "|Delete saved search"), GUILayout.Width(30), GUILayout.Height(EditorGUIUtility.singleLineHeight + 2)))
                                {
                                    AI.Config.searches.RemoveAt(i);
                                    AI.SaveConfig();
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    });
                    GUILayout.FlexibleSpace();
                    if (AI.DEBUG_MODE && GUILayout.Button("Reload Lookups")) ReloadLookups();

                    GUILayout.EndVertical();
                }

                // result
                if (_sgrid == null || (_sgrid.contents != null && _sgrid.contents.Length > 0 && _files == null)) PerformSearch(); // happens during recompilation
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                // assets
                GUILayout.BeginVertical();
                bool isAudio = AI.IsFileType(_selectedEntry?.Path, "Audio");
                if (_sgrid.contents != null && _sgrid.contents.Length > 0)
                {
                    EditorGUI.BeginChangeCheck();
                    _searchScrollPos = GUILayout.BeginScrollView(_searchScrollPos, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        // TODO: implement paged endless scrolling, needs some pixel calculations though
                        // if (_textureLoading != null) EditorCoroutineUtility.StopCoroutine(_textureLoading);
                        // _textureLoading = EditorCoroutineUtility.StartCoroutine(LoadTextures(false), this);
                    }

                    // draw contents
                    EditorGUI.BeginChangeCheck();

                    int inspectorCount = (AI.Config.showSearchFilterBar ? 2 : 1) - ((hideDetailsPane || !AI.Config.showSearchDetailsBar) ? 1 : 0);
                    _sgrid.Draw(position.width, inspectorCount, AI.Config.searchTileSize, UIStyles.searchTile, UIStyles.selectedSearchTile);

                    if (Event.current.type == EventType.Repaint)
                    {
                        _mouseOverSearchResultRect = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
                    }
                    if (EditorGUI.EndChangeCheck() || (_allowLogic && _searchDone))
                    {
                        // interactions
                        _sgrid.HandleMouseClicks();

                        if (AI.Config.autoHideSettings) _showSettings = false;
                        _sgrid.LimitSelection(_files.Count);
                        _selectedEntry = _files[_sgrid.selectionTile];

                        AI.StopAudio();
                        isAudio = AI.IsFileType(_selectedEntry?.Path, "Audio");
                        if (_selectedEntry != null)
                        {
                            _selectedEntry.Refresh();
                            AI.GetObserver().SetPrioritized(new List<AssetInfo> {_selectedEntry});
                            _selectedEntry.PackageDownloader.RefreshState();

                            _selectedEntry.CheckIfInProject();
                            _selectedEntry.IsMaterialized = AI.IsMaterialized(_selectedEntry.ToAsset(), _selectedEntry);
                            _ = AssetUtils.LoadPackageTexture(_selectedEntry);
                            LoadAnimTexture(_selectedEntry);

                            if (AI.Config.autoCalculateDependencies == 1)
                            {
                                // if entry is already materialized calculate dependencies immediately
                                if (!_previewInProgress && _selectedEntry.DependencyState == AssetInfo.DependencyStateOptions.Unknown && _selectedEntry.IsMaterialized)
                                {
                                    // must run in same thread
                                    _ = CalculateDependencies(_selectedEntry);
                                }
                            }

                            if (!_searchDone && AI.Config.pingSelected && _selectedEntry.InProject) PingAsset(_selectedEntry);
                        }
                        _searchDone = false;

                        // Used event is thrown if user manually selected the entry
                        if (Event.current.type == EventType.Used)
                        {
                            if (instantSelection)
                            {
                                ExecuteSingleAction();
                            }
                            else if (AI.Config.autoPlayAudio && isAudio) PlayAudio(_selectedEntry);
                        }
                    }
                    GUILayout.EndScrollView();

                    // navigation
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();

                    if (AI.Config.showTileSizeSlider)
                    {
                        EditorGUI.BeginChangeCheck();
                        AI.Config.searchTileSize = EditorGUILayout.IntSlider(AI.Config.searchTileSize, 50, 300, GUILayout.Width(150));
                        if (EditorGUI.EndChangeCheck())
                        {
                            _lastTileSizeChange = DateTime.Now;
                            AI.SaveConfig();
                        }
                    }

                    GUILayout.FlexibleSpace();
                    if (_pageCount > 1)
                    {
                        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageUp) SetPage(1);
                        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageDown) SetPage(_pageCount);

                        EditorGUI.BeginDisabledGroup(_curPage <= 1);
                        if ((!_showSettings && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow) ||
                            GUILayout.Button("<", GUILayout.ExpandWidth(false))) SetPage(_curPage - 1);
                        EditorGUI.EndDisabledGroup();

                        if (EditorGUILayout.DropdownButton(UIStyles.Content($"Page {_curPage:N0}/{_pageCount:N0}", $"{_resultCount:N0} results in total"), FocusType.Keyboard, UIStyles.centerPopup, GUILayout.MinWidth(100)))
                        {
                            DropDownUI pageUI = new DropDownUI();
                            pageUI.Init(1, _pageCount, _curPage, "Page ", null, SetPage);
                            PopupWindow.Show(_pageButtonRect, pageUI);
                        }
                        if (Event.current.type == EventType.Repaint) _pageButtonRect = GUILayoutUtility.GetLastRect();

                        EditorGUI.BeginDisabledGroup(_curPage >= _pageCount);
                        if ((!_showSettings && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow) ||
                            GUILayout.Button(">", GUILayout.ExpandWidth(false))) SetPage(_curPage + 1);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"{_resultCount:N0} results", UIStyles.centerLabel, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.FlexibleSpace();
                    if (!hideDetailsPane && AI.Config.showSearchDetailsBar)
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent("Settings", "|Show/Hide Settings Tab"))) _showSettings = !_showSettings;
                    }
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                else
                {
                    if (!_lockSelection) _selectedEntry = null;
                    GUILayout.Label("No matching results", UIStyles.whiteCenter, GUILayout.MinHeight(AI.Config.searchTileSize));

                    bool isIndexing = AI.IndexingInProgress;
                    bool hasHiddenExtensions = AI.Config.searchType == 0 && !string.IsNullOrWhiteSpace(AI.Config.excludedExtensions);
                    bool hasHiddenPreviews = AI.Config.previewVisibility > 0;
                    if (isIndexing || hasHiddenExtensions || hasHiddenPreviews)
                    {
                        GUILayout.Label("Search result is potentially limited", EditorStyles.centeredGreyMiniLabel);
                        if (isIndexing) GUILayout.Label("Index is currently being updated", EditorStyles.centeredGreyMiniLabel);
                        if (hasHiddenExtensions)
                        {
                            EditorGUILayout.Space();
                            GUILayout.Label($"Hidden extensions: {AI.Config.excludedExtensions}", EditorStyles.centeredGreyMiniLabel);
                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Ignore Once", GUILayout.Width(100))) PerformSearch(false, true);
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }
                        if (hasHiddenPreviews) GUILayout.Label("Results depend on preview availability", EditorStyles.centeredGreyMiniLabel);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Settings", "|Show/Hide Settings Tab"))) _showSettings = !_showSettings;
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                GUILayout.EndVertical();

                // inspector
                if (!hideDetailsPane && AI.Config.showSearchDetailsBar)
                {
                    EditorGUILayout.Space();

                    int labelWidth = 95;
                    GUILayout.BeginVertical();
                    if (_sgrid.selectionCount <= 1)
                    {
                        GUILayout.BeginVertical("Details Inspector", "window", GUILayout.Width(UIStyles.INSPECTOR_WIDTH));
                        EditorGUILayout.Space();
                        _inspectorScrollPos = GUILayout.BeginScrollView(_inspectorScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
                        if (_selectedEntry == null || string.IsNullOrEmpty(_selectedEntry.SafeName))
                        {
                            // will happen after script reload
                            EditorGUILayout.HelpBox("Select an asset for details", MessageType.Info);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("File", EditorStyles.largeLabel);
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(UIStyles.Content("Name", $"Internal Id: {_selectedEntry.Id}\nGuid: {_selectedEntry.Guid}\nPreview State: {_selectedEntry.PreviewState.ToString()}"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                            if (_selectedEntry.AssetSource == Asset.Source.AssetManager)
                            {
                                if (GUILayout.Button(UIStyles.Content(Path.GetFileName(_selectedEntry.GetPath(true))), UIStyles.wrappedLinkLabel, GUILayout.ExpandWidth(true)))
                                {
                                    Application.OpenURL(_selectedEntry.GetAMAssetUrl());
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField(UIStyles.Content(Path.GetFileName(_selectedEntry.GetPath(true)), _selectedEntry.GetPath(true)), EditorStyles.wordWrappedLabel);
                            }
                            GUILayout.EndHorizontal();
                            if (_selectedEntry.AssetSource == Asset.Source.Directory) UIBlock("asset.location", () => GUILabelWithText("Location", $"{Path.GetDirectoryName(_selectedEntry.GetPath(true))}", 95, null, true));
                            if (!string.IsNullOrWhiteSpace(_selectedEntry.FileStatus)) UIBlock("asset.status", () => GUILabelWithText("Status", $"{_selectedEntry.FileStatus}"));
                            UIBlock("asset.size", () => GUILabelWithText("Size", EditorUtility.FormatBytes(_selectedEntry.Size)));
                            if (_selectedEntry.Width > 0) UIBlock("asset.dimensions", () => GUILabelWithText("Dimensions", $"{_selectedEntry.Width}x{_selectedEntry.Height} px"));
                            if (_selectedEntry.Length > 0) UIBlock("asset.length", () => GUILabelWithText("Length", $"{_selectedEntry.Length:N2} seconds"));
                            if (ShowAdvanced() || _selectedEntry.InProject) GUILabelWithText("In Project", _selectedEntry.InProject ? "Yes" : "No");
                            if (_selectedEntry.IsDownloaded)
                            {
                                bool needsDependencyScan = false;
                                if (_selectedEntry.AssetSource == Asset.Source.AssetManager || DependencyAnalysis.NeedsScan(_selectedEntry.Type))
                                {
                                    UIBlock("asset.dependencies", () =>
                                    {
                                        switch (_selectedEntry.DependencyState)
                                        {
                                            case AssetInfo.DependencyStateOptions.Unknown:
                                                needsDependencyScan = true;
                                                GUILayout.BeginHorizontal();
                                                EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                                                EditorGUI.BeginDisabledGroup(_previewInProgress);
                                                if (GUILayout.Button("Calculate", GUILayout.ExpandWidth(false)))
                                                {
                                                    // must run in same thread
                                                    _ = CalculateDependencies(_selectedEntry);
                                                }
                                                EditorGUI.EndDisabledGroup();
                                                GUILayout.EndHorizontal();
                                                break;

                                            case AssetInfo.DependencyStateOptions.Calculating:
                                                GUILabelWithText("Dependencies", "Calculating...");
                                                break;

                                            case AssetInfo.DependencyStateOptions.NotPossible:
                                                GUILabelWithText("Dependencies", "Cannot determine (binary)");
                                                break;

                                            case AssetInfo.DependencyStateOptions.Failed:
                                                GUILabelWithText("Dependencies", "Failed to determine");
                                                break;

                                            case AssetInfo.DependencyStateOptions.Done:
                                                GUILayout.BeginHorizontal();
                                                if (ShowAdvanced())
                                                {
                                                    string scriptDeps = _selectedEntry.ScriptDependencies?.Count > 0 ? $" + {_selectedEntry.ScriptDependencies?.Count} scripts" : string.Empty;
                                                    GUILabelWithText("Dependencies", $"{_selectedEntry.MediaDependencies?.Count}{scriptDeps} ({EditorUtility.FormatBytes(_selectedEntry.DependencySize)})");
                                                }
                                                else
                                                {
                                                    GUILabelWithText("Dependencies", $"{_selectedEntry.Dependencies?.Count}");
                                                }
                                                if (_selectedEntry.Dependencies.Count > 0 && GUILayout.Button("Show..."))
                                                {
                                                    DependenciesUI depUI = DependenciesUI.ShowWindow();
                                                    depUI.Init(_selectedEntry);
                                                }

                                                GUILayout.EndHorizontal();
                                                break;
                                        }
                                    });
                                }

                                if (!searchMode)
                                {
                                    if (!_selectedEntry.InProject && string.IsNullOrEmpty(_importFolder))
                                    {
                                        EditorGUILayout.Space();
                                        EditorGUILayout.LabelField("Select a folder in Project View for import options", EditorStyles.centeredGreyMiniLabel);
                                        EditorGUI.BeginDisabledGroup(true);
                                        GUILayout.Button("Import File");
                                        EditorGUI.EndDisabledGroup();
                                    }
                                    else
                                    {
                                        if (ShowAdvanced())
                                        {
                                            EditorGUI.BeginDisabledGroup(_previewInProgress);
                                            if ((!_selectedEntry.InProject || ShowAdvanced()) && !string.IsNullOrEmpty(_importFolder))
                                            {
                                                string command = _selectedEntry.InProject ? "Reimport" : "Import";
                                                GUILabelWithText($"{command} To", _importFolder, 95, null, true);
                                                EditorGUILayout.Space();
                                                if (needsDependencyScan)
                                                {
                                                    EditorGUILayout.LabelField("Dependency scan needed to determine import options.", EditorStyles.centeredGreyMiniLabel);
                                                    EditorGUI.BeginDisabledGroup(true);
                                                    GUILayout.Button("Import File");
                                                    EditorGUI.EndDisabledGroup();
                                                }
                                                else
                                                {
                                                    if (GUILayout.Button($"{command} File" + (_selectedEntry.DependencySize > 0 ? " Only" : ""))) CopyTo(_selectedEntry, _importFolder, false, false, true, false, _selectedEntry.InProject);
                                                    if (_selectedEntry.DependencySize > 0 && DependencyAnalysis.NeedsScan(_selectedEntry.Type))
                                                    {
                                                        if (GUILayout.Button($"{command} With Dependencies")) CopyTo(_selectedEntry, _importFolder, true, false, true, false, _selectedEntry.InProject);
                                                        if (_selectedEntry.ScriptDependencies.Count > 0)
                                                        {
                                                            if (GUILayout.Button($"{command} With Dependencies + Scripts")) CopyTo(_selectedEntry, _importFolder, true, true, true, false, _selectedEntry.InProject);
                                                        }

                                                        EditorGUILayout.Space();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            EditorGUILayout.Space();
                                            if (!_selectedEntry.InProject)
                                            {
                                                EditorGUI.BeginDisabledGroup(_previewInProgress);
                                                if (GUILayout.Button("Import")) CopyTo(_selectedEntry, _importFolder, true);
                                                EditorGUI.EndDisabledGroup();
                                            }
                                        }
                                    }
                                }

#if !ASSET_INVENTORY_NOAUDIO
                                if (isAudio)
                                {
                                    UIBlock("asset.actions.audiopreview", () =>
                                    {
                                        bool isPreviewClipPlaying = EditorAudioUtility.IsPreviewClipPlaying();

                                        GUILayout.BeginHorizontal();
                                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_PlayButton", "|Play"), GUILayout.Width(40))) PlayAudio(_selectedEntry);
                                        EditorGUI.BeginDisabledGroup(!isPreviewClipPlaying);
                                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_PreMatQuad", "|Stop"), GUILayout.Width(40))) EditorAudioUtility.StopAllPreviewClips();
                                        EditorGUI.EndDisabledGroup();
                                        EditorGUILayout.Space();
                                        EditorGUI.BeginChangeCheck();
                                        AI.Config.autoPlayAudio = GUILayout.Toggle(AI.Config.autoPlayAudio, "Auto-Play");
                                        AI.Config.loopAudio = GUILayout.Toggle(AI.Config.loopAudio, "Loop");
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            AI.SaveConfig();
                                            if (AI.Config.autoPlayAudio) PlayAudio(_selectedEntry);
                                        }
                                        GUILayout.EndHorizontal();

                                        // scrubbing (Unity 2020.1+)
                                        if (isPreviewClipPlaying)
                                        {
                                            AudioClip currentClip = EditorAudioUtility.LastPlayedPreviewClip;
                                            EditorGUI.BeginChangeCheck();
                                            float newVal = EditorGUILayout.Slider(EditorAudioUtility.GetPreviewClipPosition(), 0, currentClip.length);
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                AI.StopAudio();
                                                EditorAudioUtility.PlayPreviewClip(currentClip, Mathf.RoundToInt(currentClip.samples * newVal / currentClip.length), false);
                                            }
                                        }
                                        EditorGUILayout.Space();
                                    });
                                }
#endif

                                if (_selectedEntry.InProject && !AI.Config.pingSelected)
                                {
                                    UIBlock("asset.actions.ping", () =>
                                    {
                                        if (GUILayout.Button("Ping")) PingAsset(_selectedEntry);
                                    });
                                }

                                if (!searchMode)
                                {
                                    UIBlock("asset.actions.open", () =>
                                    {
                                        if (GUILayout.Button(UIStyles.Content("Open", "Open the file with the assigned system application"))) Open(_selectedEntry);
                                    });
                                    UIBlock("asset.actions.openexplorer", () =>
                                    {
                                        if (GUILayout.Button(Application.platform == RuntimePlatform.OSXEditor ? "Show in Finder" : "Show in Explorer")) OpenExplorer(_selectedEntry);
                                    });
                                    EditorGUI.BeginDisabledGroup(_previewInProgress);
                                    UIBlock("asset.actions.recreatepreview", () =>
                                    {
                                        if ((ShowAdvanced() || _selectedEntry.PreviewState == AssetFile.PreviewOptions.Error || _selectedEntry.PreviewState == AssetFile.PreviewOptions.None || _selectedEntry.PreviewState == AssetFile.PreviewOptions.Redo)
                                            && GUILayout.Button("Recreate Preview"))
                                        {
                                            RecreatePreviews(new List<AssetInfo> {_selectedEntry});
                                        }
                                    });
                                    UIBlock("asset.actions.recreateaicaption", () =>
                                    {
                                        if (ShowAdvanced() && AI.Config.createAICaptions && GUILayout.Button(string.IsNullOrWhiteSpace(_selectedEntry.AICaption) ? "Create AI Caption" : "Recreate AI Caption"))
                                        {
                                            RecreateAICaptions(new List<AssetInfo> {_selectedEntry});
                                        }
                                    });
                                    EditorGUI.EndDisabledGroup();

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
                                    if (AI.Config.indexAssetManager)
                                    {
                                        EditorGUI.BeginDisabledGroup(CloudAssetManagement.IsBusy);
                                        EditorGUILayout.Space();
                                        if (_selectedEntry.AssetSource == Asset.Source.AssetManager)
                                        {
                                            if (_selectedEntry.ParentInfo == null)
                                            {
                                                if (GUILayout.Button(UIStyles.Content("Delete from Project", "Delete the file from the Asset Manager project.")))
                                                {
                                                    DeleteAssetsFromProject(new List<AssetInfo> {_selectedEntry});
                                                }
                                            }
                                            else
                                            {
                                                if (GUILayout.Button(UIStyles.Content("Remove from Collection", "Remove the file from the Asset Manager collection.")))
                                                {
                                                    RemoveAssetsFromCollection(new List<AssetInfo> {_selectedEntry});
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (GUILayout.Button("Upload to Asset Manager..."))
                                            {
                                                ProjectSelectionUI projectUI = new ProjectSelectionUI();
                                                projectUI.Init(project =>
                                                {
                                                    AddAssetsToProject(project, new List<AssetInfo> {_selectedEntry});
                                                });
                                                projectUI.SetAssets(_assets);
                                                PopupWindow.Show(_amUploadButtonRect, projectUI);
                                            }
                                            if (Event.current.type == EventType.Repaint) _amUploadButtonRect = GUILayoutUtility.GetLastRect();
                                        }
                                        EditorGUI.EndDisabledGroup();
                                    }
#endif

                                    UIBlock("asset.actions.delete", () =>
                                    {
                                        EditorGUILayout.Space();
                                        if (GUILayout.Button(UIStyles.Content("Delete from Index", "Will delete the indexed file from the database. The package will need to be reindexed in order for it to appear again."))) DeleteFromIndex(_selectedEntry);
                                    });
                                }
                                if (!_selectedEntry.IsMaterialized && !_previewInProgress)
                                {
                                    UIBlock("asset.actions.extraction", () =>
                                    {
                                        if (_selectedEntry.AssetSource == Asset.Source.AssetManager)
                                        {
                                            EditorGUILayout.LabelField($"{EditorUtility.FormatBytes(_selectedEntry.Size)} will be downloaded before actions are performed", EditorStyles.centeredGreyMiniLabel);
                                        }
                                        else
                                        {
                                            EditorGUILayout.LabelField($"{EditorUtility.FormatBytes(_selectedEntry.PackageSize)} will be extracted before actions are performed", EditorStyles.centeredGreyMiniLabel);
                                        }
                                    });
                                }
                            }
                            else if (_selectedEntry.IsLocationUnmappedRelative())
                            {
                                EditorGUILayout.HelpBox("The location of this package is stored relative and no mapping has been done yet for this system in the settings: " + _selectedEntry.Location, MessageType.Info);
                            }

                            if (_previewInProgress)
                            {
                                EditorGUI.EndDisabledGroup();
                                EditorGUILayout.LabelField("Working...", UIStyles.centeredWhiteMiniLabel);
                                EditorGUI.BeginDisabledGroup(_previewInProgress);
                            }

                            if (!string.IsNullOrWhiteSpace(_selectedEntry.AICaption))
                            {
                                EditorGUILayout.LabelField(_selectedEntry.AICaption, EditorStyles.wordWrappedLabel);
                            }

                            if (!searchMode)
                            {
                                UIBlock("asset.actions.tag", () =>
                                {
                                    // tags
                                    DrawAddFileTag(new List<AssetInfo> {_selectedEntry});

                                    if (_selectedEntry.AssetTags != null && _selectedEntry.AssetTags.Count > 0)
                                    {
                                        float x = 0f;
                                        foreach (TagInfo tagInfo in _selectedEntry.AssetTags)
                                        {
                                            x = CalcTagSize(x, tagInfo.Name);
                                            UIStyles.DrawTag(tagInfo, () =>
                                            {
                                                Tagging.RemoveTagAssignment(_selectedEntry, tagInfo, true, true);
                                                _requireAssetTreeRebuild = true;
                                                _requireSearchUpdate = true;
                                            });
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                });
                            }
                            EditorGUILayout.Space();
                            UIStyles.DrawUILine(Color.gray * 0.6f);
                            EditorGUILayout.Space();

                            DrawPackageDetails(_selectedEntry, false, !searchMode, false);
                        }

                        GUILayout.EndScrollView();
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.BeginVertical("Bulk Actions", "window", GUILayout.Width(UIStyles.INSPECTOR_WIDTH));
                        EditorGUILayout.Space();
                        _inspectorScrollPos = GUILayout.BeginScrollView(_inspectorScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
                        UIBlock("asset.bulk.count", () => GUILabelWithText("Selected", $"{_sgrid.selectionCount:N0}"));
                        UIBlock("asset.bulk.packages", () => GUILabelWithText("Packages", $"{_sgrid.selectionPackageCount:N0}"));
                        UIBlock("asset.bulk.size", () => GUILabelWithText("Size", EditorUtility.FormatBytes(_sgrid.selectionSize)));

                        int inProject = _sgrid.selectionItems.Count(item => item.InProject);
                        UIBlock("asset.bulk.inproject", () =>
                        {
                            GUILabelWithText("In Project", $"{inProject:N0}/{_sgrid.selectionCount:N0}");
                        });

                        EditorGUI.BeginDisabledGroup(_previewInProgress);
                        if (!searchMode && !string.IsNullOrEmpty(_importFolder))
                        {
                            if (inProject < _sgrid.selectionCount)
                            {
                                UIBlock("asset.bulk.actions.import", () =>
                                {
                                    string command = "Import";
                                    if (inProject > 0) command += $" {_sgrid.selectionCount - inProject} Remaining";

                                    GUILabelWithText("Import To", _importFolder, 95, null, true);
                                    EditorGUILayout.Space();
                                    if (GUILayout.Button($"{command} Files")) ImportBulkFiles(_sgrid.selectionItems);
                                });
                            }
                        }

                        if (!searchMode)
                        {
                            UIBlock("asset.bulk.actions.open", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Open", "Open the files with the assigned system application"))) _sgrid.selectionItems.ForEach(Open);
                            });
                            UIBlock("asset.bulk.actions.openexplorer", () =>
                            {
                                if (GUILayout.Button(Application.platform == RuntimePlatform.OSXEditor ? "Show in Finder" : "Show in Explorer")) _sgrid.selectionItems.ForEach(OpenExplorer);
                            });
                            UIBlock("asset.bulk.actions.recreatepreviews", () =>
                            {
                                EditorGUI.BeginDisabledGroup(_previewInProgress);
                                if (GUILayout.Button("Recreate Previews")) RecreatePreviews(_sgrid.selectionItems);
                                EditorGUI.EndDisabledGroup();
                            });
                            UIBlock("asset.bulk.actions.recreateaicaptions", () =>
                            {
                                if (ShowAdvanced() && AI.Config.createAICaptions && GUILayout.Button("Recreate AI Captions"))
                                {
                                    RecreateAICaptions(_sgrid.selectionItems);
                                }
                            });

#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
                            if (AI.Config.indexAssetManager)
                            {
                                EditorGUI.BeginDisabledGroup(CloudAssetManagement.IsBusy);
                                EditorGUILayout.Space();
                                if (_assetFileAMProjectCount + _assetFileAMCollectionCount > 0)
                                {
                                    if (_assetFileAMProjectCount > 0)
                                    {
                                        if (GUILayout.Button(UIStyles.Content("Delete from Project", "Delete the files from the Asset Manager project.")))
                                        {
                                            DeleteAssetsFromProject(_sgrid.selectionItems);
                                        }
                                    }
                                    if (_assetFileAMCollectionCount > 0)
                                    {
                                        if (GUILayout.Button(UIStyles.Content("Remove from Collection", "Remove the files from the Asset Manager collection.")))
                                        {
                                            RemoveAssetsFromCollection(_sgrid.selectionItems);
                                        }
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button("Upload to Asset Manager..."))
                                    {
                                        ProjectSelectionUI projectUI = new ProjectSelectionUI();
                                        projectUI.Init(project =>
                                        {
                                            AddAssetsToProject(project, _sgrid.selectionItems);
                                        });
                                        projectUI.SetAssets(_assets);
                                        PopupWindow.Show(_amUploadButtonRect, projectUI);
                                    }
                                    if (Event.current.type == EventType.Repaint) _amUploadButtonRect = GUILayoutUtility.GetLastRect();
                                }
                                EditorGUI.EndDisabledGroup();
                            }
#endif
                            UIBlock("asset.bulk.actions.delete", () =>
                            {
                                EditorGUILayout.Space();
                                if (GUILayout.Button(UIStyles.Content("Delete from Index", "Will delete the indexed files from the database. The package will need to be reindexed in order for it to appear again.")))
                                {
                                    _sgrid.selectionItems.ForEach(DeleteFromIndex);
                                }
                            });
                        }
                        EditorGUI.EndDisabledGroup();
                        if (_previewInProgress) EditorGUILayout.LabelField("Operation in progress...", UIStyles.centeredWhiteMiniLabel);

                        UIBlock("asset.bulk.actions.tag", () =>
                        {
                            // tags
                            DrawAddFileTag(_sgrid.selectionItems);

                            float x = 0f;
                            List<string> toRemove = new List<string>();
                            foreach (KeyValuePair<string, Tuple<int, Color>> bulkTag in _assetFileBulkTags)
                            {
                                string tagName = $"{bulkTag.Key} ({bulkTag.Value.Item1})";
                                x = CalcTagSize(x, tagName);
                                UIStyles.DrawTag(tagName, bulkTag.Value.Item2, () =>
                                {
                                    Tagging.RemoveAssetTagAssignment(_sgrid.selectionItems, bulkTag.Key, true);
                                    toRemove.Add(bulkTag.Key);
                                }, UIStyles.TagStyle.Remove);
                            }
                            toRemove.ForEach(key => _assetFileBulkTags.Remove(key));
                            GUILayout.EndHorizontal();
                        });

                        GUILayout.EndScrollView();
                        GUILayout.EndVertical();
                    }
                    if (_showSettings)
                    {
                        EditorGUILayout.Space();
                        GUILayout.BeginVertical("View Settings", "window", GUILayout.Width(UIStyles.INSPECTOR_WIDTH));
                        EditorGUILayout.Space();

                        EditorGUI.BeginChangeCheck();

                        int width = 135;

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Search In", "Field to use for finding assets when doing plain searches and no expert search."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.searchField = EditorGUILayout.Popup(AI.Config.searchField, _searchFields);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Sort by", "Specify the sort order. Unsorted will result in the fastest experience."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.sortField = EditorGUILayout.Popup(AI.Config.sortField, _sortFields);
                        if (GUILayout.Button(AI.Config.sortDescending ? UIStyles.Content("˅", "Descending") : UIStyles.Content("˄", "Ascending"), GUILayout.Width(17)))
                        {
                            AI.Config.sortDescending = !AI.Config.sortDescending;
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Results", $"Maximum number of results to show. A (configurable) hard limit of {AI.Config.maxResultsLimit} will be enforced to keep Unity responsive."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.maxResults = EditorGUILayout.Popup(AI.Config.maxResults, _resultSizes);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Hide Extensions", "File extensions to hide from search results when searching for all file types, e.g. asset;json;txt. These will still be indexed."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.excludeExtensions = EditorGUILayout.Toggle(AI.Config.excludeExtensions, GUILayout.Width(16));
                        AI.Config.excludedExtensions = EditorGUILayout.DelayedTextField(AI.Config.excludedExtensions);
                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            dirty = true;
                            _curPage = 1;
                            AI.SaveConfig();
                        }

                        EditorGUILayout.Space();
                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Tile Size", "Dimensions of search result previews. Preview images will still be 128x128 max."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.searchTileSize = EditorGUILayout.IntSlider(AI.Config.searchTileSize, 50, 300);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            _lastTileSizeChange = DateTime.Now;
                            AI.SaveConfig();
                        }

                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Tile Text", "Text to be shown on the tile"), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.tileText = EditorGUILayout.Popup(AI.Config.tileText, _tileTitle);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            dirty = true;
                            AI.SaveConfig();
                        }

                        EditorGUILayout.Space();

                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Search While Typing", "Will search immediately while typing and update results constantly."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.searchAutomatically = EditorGUILayout.Toggle(AI.Config.searchAutomatically);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck()) AI.SaveConfig();

                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Sub-Packages", "Will search through sub-packages as well if a filter is set for a specific package."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.searchSubPackages = EditorGUILayout.Toggle(AI.Config.searchSubPackages);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            dirty = true;
                            AI.SaveConfig();
                        }

                        EditorGUI.BeginChangeCheck();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Auto-Play Audio", "Will automatically extract unity packages to play the sound file if they were not extracted yet. This is the most convenient option but will require sufficient hard disk space."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.autoPlayAudio = EditorGUILayout.Toggle(AI.Config.autoPlayAudio);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Ping Selected", "Highlight selected items in the Unity project tree if they are found in the current project."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.pingSelected = EditorGUILayout.Toggle(AI.Config.pingSelected);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Ping Imported", "Highlight items in the Unity project tree after import."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.pingImported = EditorGUILayout.Toggle(AI.Config.pingImported);
                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck()) AI.SaveConfig();

                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Group Lists", "Add a second level hierarchy to dropdowns if they become too long to scroll."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.groupLists = EditorGUILayout.Toggle(AI.Config.groupLists);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            AI.SaveConfig();
                            ReloadLookups();
                        }

                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Double-Click Action", "Define what should happen when double-clicking on search results. Holding ALT will trigger the not selected alternative action."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.doubleClickBehavior = EditorGUILayout.Popup(AI.Config.doubleClickBehavior, _doubleClickOptions);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck()) AI.SaveConfig();

                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Dependency Calc", "Can automatically calculate dependencies for assets that are already extracted."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.autoCalculateDependencies = EditorGUILayout.Popup(AI.Config.autoCalculateDependencies, _dependencyOptions);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Previews", "Optionally restricts search results to those with either preview images available or not."), EditorStyles.boldLabel, GUILayout.Width(width));
                        AI.Config.previewVisibility = EditorGUILayout.Popup(AI.Config.previewVisibility, _previewOptions);
                        GUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck())
                        {
                            dirty = true;
                            AI.SaveConfig();
                        }

                        EditorGUILayout.Space();
                        GUILayout.EndVertical();
                    }
                    if (searchMode)
                    {
                        if (GUILayout.Button("Select", GUILayout.Height(40))) ExecuteSingleAction();
                    }
                    else
                    {
                        if (!ShowAdvanced() && AI.Config.showHints) EditorGUILayout.LabelField("Hold down CTRL for additional options.", EditorStyles.centeredGreyMiniLabel);
                    }

                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                _sgrid.HandleKeyboardCommands();

                if (dirty)
                {
                    _requireSearchUpdate = true;
                    _keepSearchResultPage = false;
                }
                EditorGUIUtility.labelWidth = 0;
            }
        }

        private async void ImportBulkFiles(List<AssetInfo> items)
        {
            _previewInProgress = true;
            foreach (AssetInfo info in items)
            {
                // must be done consecutively to avoid IO conflicts
                await AI.CopyTo(info, _importFolder, true);
            }
            _previewInProgress = false;
        }

        private void DrawAddFileTag(List<AssetInfo> assets)
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(UIStyles.Content("Add Tag..."), GUILayout.Width(70)))
            {
                TagSelectionUI tagUI = new TagSelectionUI();
                tagUI.Init(TagAssignment.Target.Asset, CalculateSearchBulkSelection);
                tagUI.SetAssets(assets);
                PopupWindow.Show(_tag2ButtonRect, tagUI);
            }
            if (Event.current.type == EventType.Repaint) _tag2ButtonRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(15);
        }

        private async void ExecuteSingleAction()
        {
            if (_selectedEntry == null) return;

            List<AssetInfo> files = new List<AssetInfo>();
            Dictionary<string, AssetInfo> identifiedTextures = null;
            if (textureMode)
            {
                identifiedTextures = IdentifyTextures(_selectedEntry);
                files.AddRange(identifiedTextures.Values); // TODO: one file will be duplicate, not an issue but will save time to eliminate it
            }
            else
            {
                files.Add(_selectedEntry);
            }

            foreach (AssetInfo info in files)
            {
                info.CheckIfInProject();
                if (!info.InProject)
                {
                    _previewInProgress = true;
                    _lockSelection = true;

                    // download on-demand
                    if (!info.IsDownloaded)
                    {
                        if (info.IsAbandoned)
                        {
                            Debug.LogError($"Cannot download {info.GetDisplayName()} as it is an abandoned package.");
                            _lockSelection = false;
                            return;
                        }

                        AI.GetObserver().Attach(info);
                        if (info.PackageDownloader.IsDownloadSupported())
                        {
                            _curOperation = $"Downloading {info.GetDisplayName()}...";
                            info.PackageDownloader.Download();
                            do
                            {
                                await Task.Delay(200);

                                info.PackageDownloader.RefreshState();
                                float progress = info.PackageDownloader.GetState().progress * 100f;
                                _curOperation = $"Downloading {info.GetDisplayName()}: {progress:N0}%...";
                            } while (info.IsDownloading());
                            await Task.Delay(3000); // ensure all file operations have finished, can otherwise lead to issues
                            info.Refresh();
                        }
                    }

                    _curOperation = $"Extracting & Importing '{info.FileName}'...";
                    await AI.CopyTo(info, _importFolder, true);
                    _previewInProgress = false;

                    if (!info.InProject)
                    {
                        Debug.LogError("The file could not be materialized into the project.");
                        _lockSelection = false;
                        return;
                    }
                }
            }

            Close();
            AI.StopAudio();

            if (textureMode)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                foreach (KeyValuePair<string, AssetInfo> file in identifiedTextures)
                {
                    result.Add(file.Key, file.Value.ProjectPath);
                }
                searchModeTextureCallback?.Invoke(result);
            }
            else
            {
                searchModeCallback?.Invoke(_selectedEntry.ProjectPath);
            }
            _lockSelection = false;
        }

        private Dictionary<string, AssetInfo> IdentifyTextures(AssetInfo info)
        {
            TextureNameSuggester tns = new TextureNameSuggester();
            Dictionary<string, string> files = tns.SuggestFileNames(info.Path, path =>
            {
                string sep = info.Path.Contains("/") ? "/" : "\\";
                string toCheck = info.Path.Substring(0, info.Path.LastIndexOf(sep) + 1) + Path.GetFileName(path);
                AssetInfo ai = AI.GetAssetByPath(toCheck, info.ToAsset());
                return ai?.Path; // capitalization could be different from actual validation request, so use result
            });

            Dictionary<string, AssetInfo> result = new Dictionary<string, AssetInfo>();
            foreach (KeyValuePair<string, string> file in files)
            {
                AssetInfo ai = AI.GetAssetByPath(file.Value, info.ToAsset());
                if (ai != null) result.Add(file.Key, ai);
            }
            return result;
        }

        private void DeleteFromIndex(AssetInfo info)
        {
            AI.ForgetAssetFile(info);
            _requireSearchUpdate = true;
        }

        private async void RecreatePreviews(List<AssetInfo> infos)
        {
            _previewInProgress = true;
            AssetProgress.CancellationRequested = false;
            if (await new PreviewPipeline().RecreatePreviews(infos, false) > 0) _requireSearchUpdate = true;
            _previewInProgress = false;
        }

        private async void RecreateAICaptions(List<AssetInfo> infos)
        {
            _previewInProgress = true;
            AssetProgress.CancellationRequested = false;
            await new CaptionCreator().Index(infos);
            _requireSearchUpdate = true;
            _previewInProgress = false;
        }

        private void LoadSearch(SavedSearch search)
        {
            _searchPhrase = search.searchPhrase;
            _selectedPackageTypes = search.packageTypes;
            _selectedPackageSRPs = search.packageSRPs;
            _selectedImageType = search.imageType;
            _selectedColorOption = search.colorOption;
            _selectedColor = ImageUtils.FromHex(search.searchColor);
            _searchWidth = search.width;
            _searchHeight = search.height;
            _searchLength = search.length;
            _searchSize = search.size;
            _checkMaxWidth = search.checkMaxWidth;
            _checkMaxHeight = search.checkMaxHeight;
            _checkMaxLength = search.checkMaxLength;
            _checkMaxSize = search.checkMaxSize;

            AI.Config.searchType = Mathf.Max(0, Array.FindIndex(_types, s => s == search.type || s.EndsWith($"/{search.type}")));
            _selectedPublisher = Mathf.Max(0, Array.FindIndex(_publisherNames, s => s == search.publisher || s.EndsWith($"/{search.publisher}")));
            _selectedAsset = Mathf.Max(0, Array.FindIndex(_assetNames, s => s == search.package || s.EndsWith($"/{search.package}")));
            _selectedCategory = Mathf.Max(0, Array.FindIndex(_categoryNames, s => s == search.category || s.EndsWith($"/{search.category}")));
            _selectedPackageTag = Mathf.Max(0, Array.FindIndex(_tagNames, s => s == search.packageTag || s.EndsWith($"/{search.packageTag}")));
            _selectedFileTag = Mathf.Max(0, Array.FindIndex(_tagNames, s => s == search.fileTag || s.EndsWith($"/{search.fileTag}")));

            _requireSearchUpdate = true;
        }

        private void SaveSearch(string value)
        {
            SavedSearch spec = new SavedSearch();
            spec.name = value;
            spec.searchPhrase = _searchPhrase;
            spec.packageTypes = _selectedPackageTypes;
            spec.packageSRPs = _selectedPackageSRPs;
            spec.imageType = _selectedImageType;
            spec.colorOption = _selectedColorOption;
            spec.searchColor = "#" + ColorUtility.ToHtmlStringRGB(_selectedColor);
            spec.width = _searchWidth;
            spec.height = _searchHeight;
            spec.length = _searchLength;
            spec.size = _searchSize;
            spec.checkMaxWidth = _checkMaxWidth;
            spec.checkMaxHeight = _checkMaxHeight;
            spec.checkMaxLength = _checkMaxLength;
            spec.checkMaxSize = _checkMaxSize;
            spec.color = ColorUtility.ToHtmlStringRGB(Random.ColorHSV());

            if (AI.Config.searchType > 0 && _types.Length > AI.Config.searchType)
            {
                spec.type = _types[AI.Config.searchType].Split('/').LastOrDefault();
            }

            if (_selectedPublisher > 0 && _publisherNames.Length > _selectedPublisher)
            {
                spec.publisher = _publisherNames[_selectedPublisher].Split('/').LastOrDefault();
            }

            if (_selectedAsset > 0 && _assetNames.Length > _selectedAsset)
            {
                spec.package = _assetNames[_selectedAsset].Split('/').LastOrDefault();
            }

            if (_selectedCategory > 0 && _categoryNames.Length > _selectedCategory)
            {
                spec.category = _categoryNames[_selectedCategory].Split('/').LastOrDefault();
            }

            if (_selectedPackageTag > 0 && _tagNames.Length > _selectedPackageTag)
            {
                spec.packageTag = _tagNames[_selectedPackageTag].Split('/').LastOrDefault();
            }

            if (_selectedFileTag > 0 && _tagNames.Length > _selectedFileTag)
            {
                spec.fileTag = _tagNames[_selectedFileTag].Split('/').LastOrDefault();
            }

            AI.Config.searches.Add(spec);
            AI.SaveConfig();
        }

        private async void PlayAudio(AssetInfo info)
        {
            // play instantly if no extraction is required
            if (_previewInProgress)
            {
                if (AI.IsMaterialized(info.ToAsset(), info)) await AI.PlayAudio(info);
                return;
            }

            _previewInProgress = true;

            await AI.PlayAudio(info);

            _previewInProgress = false;
        }

        private async void PingAsset(AssetInfo info)
        {
            if (disablePings) return;

            // requires pauses in-between to allow editor to catch up
            EditorApplication.ExecuteMenuItem("Window/General/Project");
            await Task.Yield();

            Selection.activeObject = null;
            await Task.Yield();

            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(info.ProjectPath);
            if (Selection.activeObject == null) info.ProjectPath = null; // probably got deleted again
        }

        private async Task CalculateDependencies(AssetInfo info)
        {
            _previewInProgress = true;
            await AI.CalculateDependencies(info);
            _previewInProgress = false;
        }

        private async void Open(AssetInfo info)
        {
            if (!info.IsDownloaded) return;

            _previewInProgress = true;
            string targetPath;
            if (info.InProject)
            {
                targetPath = info.ProjectPath;
            }
            else
            {
                targetPath = await AI.EnsureMaterializedAsset(info);
                if (info.Id == 0) _requireSearchUpdate = true; // was deleted
            }

            if (targetPath != null) EditorUtility.OpenWithDefaultApp(targetPath);
            _previewInProgress = false;
        }

        private async void OpenExplorer(AssetInfo info)
        {
            if (!info.IsDownloaded) return;

            _previewInProgress = true;
            string targetPath;
            if (info.InProject)
            {
                targetPath = info.ProjectPath;
            }
            else
            {
                targetPath = await AI.EnsureMaterializedAsset(info);
                if (info.Id == 0) _requireSearchUpdate = true; // was deleted
            }

            if (targetPath != null) EditorUtility.RevealInFinder(IOUtils.ToShortPath(targetPath));
            _previewInProgress = false;
        }

        private async void CopyTo(AssetInfo info, string targetFolder, bool withDependencies = false, bool withScripts = false, bool autoPing = true, bool fromDragDrop = false, bool reimport = false)
        {
            _previewInProgress = true;

            string mainFile = await AI.CopyTo(info, targetFolder, withDependencies, withScripts, fromDragDrop, false, reimport);
            if (autoPing && mainFile != null)
            {
                if (AI.Config.pingImported) PingAsset(new AssetInfo().WithProjectPath(mainFile));
                if (AI.Config.statsImports == 5) ShowInterstitial();
            }

            _previewInProgress = false;
        }

        private void SetPage(int newPage)
        {
            SetPage(newPage, false);
        }

        private void SetPage(int newPage, bool ignoreExcludedExtensions)
        {
            newPage = Mathf.Clamp(newPage, 1, _pageCount);
            if (newPage != _curPage)
            {
                _curPage = newPage;
                _sgrid.DeselectAll();
                _searchScrollPos = Vector2.zero;
                if (_curPage > 0) PerformSearch(true, ignoreExcludedExtensions);
            }
        }

        private bool IsFilterApplicable(string filterName)
        {
            string searchType = GetRawSearchType();
            if (searchType == null) return true;
            if (AI.FilterRestriction.TryGetValue(filterName, out string[] restrictions))
            {
                return restrictions.Contains(searchType);
            }

            return true;
        }

        private string GetRawSearchType()
        {
            int searchType = _fixedSearchTypeIdx >= 0 ? _fixedSearchTypeIdx : AI.Config.searchType;
            return searchType > 0 && _types.Length > searchType ? _types[searchType] : null;
        }

        private void PerformSearch(bool keepPage = false, bool ignoreExcludedExtensions = false)
        {
            if (AI.DEBUG_MODE) Debug.LogWarning("Perform Search");

            _requireSearchUpdate = false;
            _keepSearchResultPage = true;
            int lastCount = _resultCount; // a bit of a heuristic but works great and is very performant
            string selectedSize = _resultSizes[AI.Config.maxResults];
            int.TryParse(selectedSize, out int maxResults);
            if (maxResults <= 0 || maxResults > AI.Config.maxResultsLimit) maxResults = AI.Config.maxResultsLimit;
            List<string> wheres = new List<string>();
            List<object> args = new List<object>();
            string packageTagJoin = "";
            string fileTagJoin = "";
            string computedFields = "";
            string lastWhere = null;

            wheres.Add("Asset.Exclude=0");

            // only add detail filters if section is open to not have confusing search results
            if (!AI.Config.filterOnlyIfBarVisible || AI.Config.showSearchFilterBar)
            {
                // numerical conditions first
                switch (_selectedPackageSRPs)
                {
                    case 1:
                        wheres.Add("Asset.BIRPCompatible=1");
                        break;

                    case 2:
                        wheres.Add("Asset.URPCompatible=1");
                        break;

                    case 3:
                        wheres.Add("Asset.HDRPCompatible=1");
                        break;
                }

                if (IsFilterApplicable("Width") && !string.IsNullOrWhiteSpace(_searchWidth))
                {
                    if (int.TryParse(_searchWidth, out int width) && width > 0)
                    {
                        string widthComp = _checkMaxWidth ? "<=" : ">=";
                        wheres.Add($"AssetFile.Width > 0 and AssetFile.Width {widthComp} ?");
                        args.Add(width);
                    }
                }

                if (IsFilterApplicable("Height") && !string.IsNullOrWhiteSpace(_searchHeight))
                {
                    if (int.TryParse(_searchHeight, out int height) && height > 0)
                    {
                        string heightComp = _checkMaxHeight ? "<=" : ">=";
                        wheres.Add($"AssetFile.Height > 0 and AssetFile.Height {heightComp} ?");
                        args.Add(height);
                    }
                }

                if (IsFilterApplicable("Length") && !string.IsNullOrWhiteSpace(_searchLength))
                {
                    if (float.TryParse(_searchLength, out float length) && length > 0)
                    {
                        string lengthComp = _checkMaxLength ? "<=" : ">=";
                        wheres.Add($"AssetFile.Length > 0 and AssetFile.Length {lengthComp} ?");
                        args.Add(length);
                    }
                }

                if (!string.IsNullOrWhiteSpace(_searchSize))
                {
                    if (int.TryParse(_searchSize, out int size) && size > 0)
                    {
                        string sizeComp = _checkMaxSize ? "<=" : ">=";
                        wheres.Add($"AssetFile.Size > 0 and AssetFile.Size {sizeComp} ?");
                        args.Add(size * 1024);
                    }
                }

                if (_selectedPackageTag == 1)
                {
                    wheres.Add("not exists (select tap.Id from TagAssignment as tap where Asset.Id = tap.TargetId and tap.TagTarget = 0)");
                }
                else if (_selectedPackageTag > 1 && _tagNames.Length > _selectedPackageTag)
                {
                    string[] arr = _tagNames[_selectedPackageTag].Split('/');
                    string tag = arr[arr.Length - 1];
                    wheres.Add("tap.TagId = ?");
                    args.Add(_tags.First(t => t.Name == tag).Id);

                    packageTagJoin = "inner join TagAssignment as tap on (Asset.Id = tap.TargetId and tap.TagTarget = 0)";
                }

                if (_selectedFileTag == 1)
                {
                    wheres.Add("not exists (select taf.Id from TagAssignment as taf where AssetFile.Id = taf.TargetId and taf.TagTarget = 1)");
                }
                else if (_selectedFileTag > 1 && _tagNames.Length > _selectedFileTag)
                {
                    string[] arr = _tagNames[_selectedFileTag].Split('/');
                    string tag = arr[arr.Length - 1];
                    wheres.Add("taf.TagId = ?");
                    args.Add(_tags.First(t => t.Name == tag).Id);

                    fileTagJoin = "inner join TagAssignment as taf on (AssetFile.Id = taf.TargetId and taf.TagTarget = 1)";
                }

                switch (_selectedPackageTypes)
                {
                    case 1:
                        wheres.Add("Asset.AssetSource != ?");
                        args.Add(Asset.Source.RegistryPackage);
                        break;

                    case 2:
                        wheres.Add("Asset.AssetSource = ?");
                        args.Add(Asset.Source.AssetStorePackage);
                        break;

                    case 3:
                        wheres.Add("Asset.AssetSource = ?");
                        args.Add(Asset.Source.RegistryPackage);
                        break;

                    case 4:
                        wheres.Add("Asset.AssetSource = ?");
                        args.Add(Asset.Source.CustomPackage);
                        break;

                    case 5:
                        wheres.Add("Asset.AssetSource = ?");
                        args.Add(Asset.Source.Directory);
                        break;

                    case 6:
                        wheres.Add("Asset.AssetSource = ?");
                        args.Add(Asset.Source.Archive);
                        break;

                    case 7:
                        wheres.Add("Asset.AssetSource = ?");
                        args.Add(Asset.Source.AssetManager);
                        break;

                }

                if (_selectedPublisher > 0 && _publisherNames.Length > _selectedPublisher)
                {
                    string[] arr = _publisherNames[_selectedPublisher].Split('/');
                    string publisher = arr[arr.Length - 1];
                    wheres.Add("Asset.SafePublisher = ?");
                    args.Add($"{publisher}");
                }

                if (_selectedAsset > 0 && _assetNames.Length > _selectedAsset)
                {
                    string[] arr = _assetNames[_selectedAsset].Split('/');
                    string asset = arr[arr.Length - 1];
                    if (asset.LastIndexOf('[') > 0)
                    {
                        string assetId = asset.Substring(asset.LastIndexOf('[') + 1);
                        assetId = assetId.Substring(0, assetId.Length - 1);
                        if (AI.Config.searchSubPackages)
                        {
                            wheres.Add("(Asset.Id = ? or Asset.ParentId = ?)");
                            args.Add(int.Parse(assetId));
                            args.Add(int.Parse(assetId));
                        }
                        else
                        {
                            wheres.Add("Asset.Id = ?");
                            args.Add(int.Parse(assetId));
                        }
                    }
                    else
                    {
                        wheres.Add("Asset.SafeName = ?");
                        args.Add($"{asset}");
                    }
                }

                if (_selectedCategory > 0 && _categoryNames.Length > _selectedCategory)
                {
                    string[] arr = _categoryNames[_selectedCategory].Split('/');
                    string category = arr[arr.Length - 1];
                    wheres.Add("Asset.SafeCategory = ?");
                    args.Add($"{category}");
                }

                if (_selectedColorOption > 0)
                {
                    wheres.Add("AssetFile.Hue >= ?");
                    wheres.Add("AssetFile.Hue <= ?");
                    args.Add(_selectedColor.ToHue() - AI.Config.hueRange / 2f);
                    args.Add(_selectedColor.ToHue() + AI.Config.hueRange / 2f);
                }

                if (IsFilterApplicable("ImageType") && _selectedImageType > 0)
                {
                    computedFields = ", CASE WHEN INSTR(AssetFile.FileName, '.') > 0 THEN Lower(SUBSTR(AssetFile.FileName, 1, INSTR(AssetFile.FileName, '.') - 1)) ELSE Lower(AssetFile.FileName) END AS FileNameWithoutExtension";
                    string[] patterns = TextureNameSuggester.suffixPatterns[_imageTypeOptions[_selectedImageType].ToLowerInvariant()];

                    // concatenate all patterns manually into an or condition and add as a single where
                    List<string> patternWheres = new List<string>();
                    foreach (string pattern in patterns)
                    {
                        if (string.IsNullOrWhiteSpace(pattern)) continue;

                        patternWheres.Add("FileNameWithoutExtension like ? ESCAPE '\\'");
                        args.Add("%" + pattern.Replace("_", "\\_"));
                    }
                    wheres.Add("(" + string.Join(" or ", patternWheres) + ")");
                }
            }

            if (!string.IsNullOrWhiteSpace(_searchPhrase))
            {
                string phrase = _searchPhrase;
                string searchField = "AssetFile.Path";

                switch (AI.Config.searchField)
                {
                    case 1:
                        searchField = "AssetFile.FileName";
                        break;

                    case 2:
                        searchField = "AssetFile.AICaption";
                        break;
                }

                // check for sqlite escaping requirements
                string escape = "";
                if (phrase.Contains("_"))
                {
                    if (!phrase.StartsWith("=")) phrase = phrase.Replace("_", "\\_");
                    escape = "ESCAPE '\\'";
                }

                if (phrase.StartsWith("=")) // expert mode
                {
                    if (phrase.Length > 1)
                    {
                        phrase = StringUtils.EscapeSQL(phrase);
                        lastWhere = phrase.Substring(1);
                    }
                }
                else if (phrase.StartsWith("~")) // exact mode
                {
                    string term = phrase.Substring(1);
                    wheres.Add($"{searchField} like ? {escape}");
                    args.Add($"%{term}%");
                }
                else
                {
                    string[] fuzzyWords = phrase.Split(' ');
                    foreach (string fuzzyWord in fuzzyWords.Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        if (fuzzyWord.StartsWith("+"))
                        {
                            wheres.Add($"{searchField} like ? {escape}");
                            args.Add($"%{fuzzyWord.Substring(1)}%");
                        }
                        else if (fuzzyWord.StartsWith("-"))
                        {
                            wheres.Add($"{searchField} not like ? {escape}");
                            args.Add($"%{fuzzyWord.Substring(1)}%");
                        }
                        else
                        {
                            wheres.Add($"{searchField} like ? {escape}");
                            args.Add($"%{fuzzyWord}%");
                        }
                    }
                }
            }

            string rawType = GetRawSearchType();
            if (rawType != null)
            {
                string[] type = rawType.Split('/');
                if (type.Length > 1)
                {
                    wheres.Add("AssetFile.Type = ?");
                    args.Add(type.Last());
                }
                else if (AI.TypeGroups.TryGetValue(rawType, out string[] group))
                {
                    // optimize SQL slightly for cases where only one type is checked
                    if (group.Length == 1)
                    {
                        wheres.Add("AssetFile.Type = ?");
                        args.Add(group[0]);
                    }
                    else
                    {
                        // sqlite does not support binding lists, parameters must be spelled out
                        List<string> paramCount = new List<string>();
                        foreach (string t in group)
                        {
                            paramCount.Add("?");
                            args.Add(t);
                        }

                        wheres.Add("AssetFile.Type in (" + string.Join(",", paramCount) + ")");
                    }
                }
            }

            if (!ignoreExcludedExtensions && AI.Config.excludeExtensions && AI.Config.searchType == 0 && !string.IsNullOrWhiteSpace(AI.Config.excludedExtensions))
            {
                string[] extensions = AI.Config.excludedExtensions.Split(';');
                List<string> paramCount = new List<string>();
                foreach (string ext in extensions)
                {
                    paramCount.Add("?");
                    args.Add(ext.Trim());
                }

                wheres.Add("AssetFile.Type not in (" + string.Join(",", paramCount) + ")");
            }

            switch (AI.Config.previewVisibility)
            {
                case 2:
                    wheres.Add("AssetFile.PreviewState in (1, 3)");
                    break;

                case 3:
                    wheres.Add("AssetFile.PreviewState not in (1, 3)");
                    break;
            }

            // ordering, can only be done on DB side since post-processing results would only work on the paged results which is incorrect
            string orderBy = "order by ";
            switch (AI.Config.sortField)
            {
                case 0:
                    orderBy += "AssetFile.Path";
                    break;

                case 1:
                    orderBy += "AssetFile.FileName";
                    break;

                case 2:
                    orderBy += "AssetFile.Size";
                    break;

                case 3:
                    orderBy += "AssetFile.Type";
                    break;

                case 4:
                    orderBy += "AssetFile.Length";
                    break;

                case 5:
                    orderBy += "AssetFile.Width";
                    break;

                case 6:
                    orderBy += "AssetFile.Height";
                    break;

                case 7:
                    orderBy += "AssetFile.Hue";
                    wheres.Add("AssetFile.Hue >=0");
                    break;

                case 8:
                    orderBy += "Asset.DisplayCategory";
                    break;

                case 9:
                    orderBy += "Asset.LastRelease";
                    break;

                case 10:
                    orderBy += "Asset.AssetRating";
                    break;

                case 11:
                    orderBy += "Asset.RatingCount";
                    break;

                default:
                    orderBy = "";
                    break;
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                orderBy += " COLLATE NOCASE";
                if (AI.Config.sortDescending) orderBy += " desc";
                orderBy += ", AssetFile.Path"; // always sort by path in case of equality of first level sorting
            }
            if (!string.IsNullOrEmpty(lastWhere)) wheres.Add(lastWhere);

            string where = wheres.Count > 0 ? "where " + string.Join(" and ", wheres) : "";
            string baseQuery = $"from AssetFile inner join Asset on Asset.Id = AssetFile.AssetId {packageTagJoin} {fileTagJoin} {where}";
            string countQuery = $"select count(*){computedFields} {baseQuery}";
            string dataQuery = $"select *, AssetFile.Id as Id{computedFields} {baseQuery} {orderBy}";
            if (maxResults > 0) dataQuery += $" limit {maxResults} offset {(_curPage - 1) * maxResults}";
            try
            {
                _searchError = null;
                _resultCount = DBAdapter.DB.ExecuteScalar<int>($"{countQuery}", args.ToArray());
                _files = DBAdapter.DB.Query<AssetInfo>($"{dataQuery}", args.ToArray());
            }
            catch (SQLiteException e)
            {
                _searchError = e.Message;
            }

            // pagination
            _sgrid.contents = _files.Select(file =>
            {
                string text = "";
                int tileTextToUse = AI.Config.tileText;
                if (tileTextToUse == 5 && string.IsNullOrEmpty(file.AICaption))
                {
                    tileTextToUse = 0;
                }
                if (tileTextToUse == 0) // intelligent
                {
                    if (AI.Config.searchTileSize < 70)
                    {
                        tileTextToUse = 6;
                    }
                    else if (AI.Config.searchTileSize < 90)
                    {
                        tileTextToUse = 4;
                    }
                    else if (AI.Config.searchTileSize < 150)
                    {
                        tileTextToUse = 3;
                    }
                    else
                    {
                        tileTextToUse = 2;
                    }
                }
                switch (tileTextToUse)
                {
                    case 2:
                        text = file.ShortPath;
                        break;

                    case 3:
                        text = file.FileName;
                        break;

                    case 4:
                        text = Path.GetFileNameWithoutExtension(file.FileName);
                        break;

                    case 5:
                        text = file.AICaption;
                        break;

                }
                text = text == null ? "" : text.Replace('/', Path.DirectorySeparatorChar);

                return new GUIContent(text);
            }).ToArray();
            _sgrid.enlargeTiles = AI.Config.enlargeTiles;
            _sgrid.centerTiles = AI.Config.centerTiles;
            _sgrid.Init(_assets, _files, CalculateSearchBulkSelection);

            AI.ResolveParents(_files, _assets);

            _pageCount = AssetUtils.GetPageCount(_resultCount, maxResults);
            if (!keepPage && lastCount != _resultCount)
            {
                SetPage(1, ignoreExcludedExtensions);
            }
            else
            {
                SetPage(_curPage, ignoreExcludedExtensions);
            }

            // preview images
            _textureLoading?.Cancel();
            _textureLoading = new CancellationTokenSource();
            LoadTextures(false, _textureLoading.Token); // TODO: should be true once pages endless scrolling is in

            _searchDone = true;
        }

        private async void LoadAnimTexture(AssetInfo info)
        {
            _curAnimFrame = 1;
            _animTexture = null;

            string animPreviewFile = info.GetPreviewFile(AI.GetPreviewFolder(), true);
            if (!File.Exists(animPreviewFile)) return;

            _animTexture = await AssetUtils.LoadLocalTexture(animPreviewFile, false, (AI.Config.upscalePreviews && !AI.Config.upscaleLossless) ? AI.Config.upscaleSize : 0);
            _animFrames = CreateUVs(AI.Config.animationGrid, AI.Config.animationGrid);
        }

        private List<Rect> CreateUVs(int columns, int rows)
        {
            List<Rect> rects = new List<Rect>();

            float frameWidth = 1f / columns;
            float frameHeight = 1f / rows;

            for (int y = rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < columns; x++)
                {
                    Rect rect = new Rect(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
                    rects.Add(rect);
                }
            }

            return rects;
        }

        private Texture2D ExtractFrame(Texture2D sourceTexture, Rect uvRect)
        {
            int x = Mathf.RoundToInt(uvRect.x * sourceTexture.width);
            int y = Mathf.RoundToInt(uvRect.y * sourceTexture.height);
            int width = Mathf.RoundToInt(uvRect.width * sourceTexture.width);
            int height = Mathf.RoundToInt(uvRect.height * sourceTexture.height);

            // Flip the y-coordinate because Unity's texture origin is at the bottom-left
            y = sourceTexture.height - y - height;

            // Create a new Texture2D to hold the frame
            Texture2D frameTexture = new Texture2D(width, height, sourceTexture.format, false);
            frameTexture.SetPixels(sourceTexture.GetPixels(x, y, width, height));
            frameTexture.Apply();

            return frameTexture;
        }

        private async void LoadTextures(bool firstPageOnly, CancellationToken ct)
        {
            int chunkSize = AI.Config.previewChunkSize;

            List<AssetInfo> files = _files.Take(firstPageOnly ? 20 * 8 : _files.Count).ToList();

            for (int i = 0; i < files.Count; i += chunkSize)
            {
                if (ct.IsCancellationRequested) return;

                List<Task> tasks = new List<Task>();

                int chunkEnd = Math.Min(i + chunkSize, files.Count);
                for (int idx = i; idx < chunkEnd; idx++)
                {
                    int localIdx = idx; // capture value
                    AssetInfo info = files[localIdx];

                    tasks.Add(ProcessAssetInfoAsync(info, localIdx, ct));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task ProcessAssetInfoAsync(AssetInfo info, int idx, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            string previewFile = null;
            if (info.HasPreview(true)) previewFile = AssetImporter.ValidatePreviewFile(info, AI.GetPreviewFolder());
            if (previewFile == null || !info.HasPreview(true))
            {
                if (!AI.Config.showIconsForMissingPreviews) return;

                // check if well-known extension
                if (_staticPreviews.TryGetValue(info.Type, out string preview))
                {
                    _sgrid.contents[idx].image = EditorGUIUtility.IconContent(preview).image;
                }
                else
                {
                    _sgrid.contents[idx].image = EditorGUIUtility.IconContent("d_DefaultAsset Icon").image;
                }
                return;
            }

            Texture2D texture = await AssetUtils.LoadLocalTexture(
                previewFile,
                false,
                (AI.Config.upscalePreviews && !AI.Config.upscaleLossless) ? AI.Config.upscaleSize : 0
            );

            if (texture == null)
            {
                info.PreviewState = AssetFile.PreviewOptions.None;
                DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", info.PreviewState, info.Id);
            }
            else if (_sgrid.contents.Length > idx)
            {
                _sgrid.contents[idx].image = texture;
            }
        }

        private void CalculateSearchBulkSelection()
        {
            _assetFileBulkTags.Clear();
            _sgrid.selectionItems.ForEach(info => info.AssetTags?.ForEach(t =>
            {
                if (!_assetFileBulkTags.ContainsKey(t.Name)) _assetFileBulkTags.Add(t.Name, new Tuple<int, Color>(0, t.GetColor()));
                _assetFileBulkTags[t.Name] = new Tuple<int, Color>(_assetFileBulkTags[t.Name].Item1 + 1, _assetFileBulkTags[t.Name].Item2);
            }));
            _assetFileAMProjectCount = _sgrid.selectionItems.Count(info => info.AssetSource == Asset.Source.AssetManager && string.IsNullOrEmpty(info.Location));
            _assetFileAMCollectionCount = _sgrid.selectionItems.Count(info => info.AssetSource == Asset.Source.AssetManager && !string.IsNullOrEmpty(info.Location));
        }

        private void OpenInSearch(AssetInfo info, bool force = false)
        {
            if (info.Id <= 0) return;
            if (!force && info.FileCount <= 0) return;
            AssetInfo oldEntry = _selectedEntry;

            if (info.Exclude)
            {
                if (!EditorUtility.DisplayDialog("Package is Excluded", "This package is currently excluded from the search. Should it be included again?", "Include Again", "Cancel"))
                {
                    return;
                }
                AI.SetAssetExclusion(info, false);
                ReloadLookups();
            }
            ResetSearch(false, true);
            if (force) _selectedEntry = oldEntry;

            AI.Config.tab = 0;

            // search for exact match first
            string displayName = info.GetDisplayName().Replace("/", " ");
            if (info.SafeName == Asset.NONE)
            {
                _selectedAsset = 1;
            }
            else
            {
                _selectedAsset = Array.IndexOf(_assetNames, _assetNames.FirstOrDefault(a => a == displayName + $" [{info.AssetId}]"));
            }
            if (_selectedAsset < 0) _selectedAsset = Array.IndexOf(_assetNames, _assetNames.FirstOrDefault(a => a == displayName.Substring(0, 1) + "/" + displayName + $" [{info.AssetId}]"));
            if (_selectedAsset < 0) _selectedAsset = Array.IndexOf(_assetNames, _assetNames.FirstOrDefault(a => a.EndsWith(displayName + $" [{info.AssetId}]")));

            if (info.AssetSource == Asset.Source.RegistryPackage && _selectedPackageTypes == 1) _selectedPackageTypes = 0;
            _requireSearchUpdate = true;
            _curPage = 1;
            AI.Config.showSearchFilterBar = true;
        }

        private void ResetSearch(bool filterBarOnly, bool keepAssetType)
        {
            if (!filterBarOnly)
            {
                _searchPhrase = "";
                if (!keepAssetType) AI.Config.searchType = 0;
            }

            _selectedEntry = null;
            _selectedAsset = 0;
            _selectedPackageTypes = 1;
            _selectedPackageSRPs = 0;
            _selectedImageType = 0;
            _selectedColorOption = 0;
            _selectedColor = Color.clear;
            _selectedPackageTag = 0;
            _selectedFileTag = 0;
            _selectedPublisher = 0;
            _selectedCategory = 0;
            _searchHeight = "";
            _checkMaxHeight = false;
            _searchWidth = "";
            _checkMaxWidth = false;
            _searchLength = "";
            _checkMaxLength = false;
            _searchSize = "";
            _checkMaxSize = false;
        }

        private async Task PerformCopyTo(AssetInfo info, string path, bool fromDragDrop = false)
        {
            if (info.InProject) return;
            if (string.IsNullOrEmpty(path)) return;

            while (info.DependencyState == AssetInfo.DependencyStateOptions.Calculating) await Task.Yield();
            if (info.DependencyState == AssetInfo.DependencyStateOptions.Unknown) await CalculateDependencies(info);
            if (info.DependencySize > 0 && DependencyAnalysis.NeedsScan(info.Type))
            {
                CopyTo(info, path, true, false, false, fromDragDrop);
            }
            else
            {
                CopyTo(info, path, false, false, true, fromDragDrop);
            }
        }

        private static bool DragDropAvailable()
        {
#if UNITY_2021_2_OR_NEWER
            return true;
#else
            return false;
#endif
        }

        private void InitDragAndDrop()
        {
#if UNITY_2021_2_OR_NEWER
            DragAndDrop.ProjectBrowserDropHandler dropHandler = OnProjectWindowDrop;
            if (!DragAndDrop.HasHandler("ProjectBrowser".GetHashCode(), dropHandler))
            {
                DragAndDrop.AddDropHandler(dropHandler);
            }
#endif
        }

        private void DeinitDragAndDrop()
        {
#if UNITY_2021_2_OR_NEWER
            DragAndDrop.ProjectBrowserDropHandler dropHandler = OnProjectWindowDrop;
            if (DragAndDrop.HasHandler("ProjectBrowser".GetHashCode(), dropHandler))
            {
                DragAndDrop.RemoveDropHandler(dropHandler);
            }
#endif
        }

        private DragAndDropVisualMode OnProjectWindowDrop(int dragInstanceId, string dropUponPath, bool perform)
        {
            if (perform && _dragging)
            {
                _dragging = false;
                DeinitDragAndDrop();

                List<AssetInfo> infos = (List<AssetInfo>)DragAndDrop.GetGenericData("AssetInfo");
                if (infos != null && infos.Count > 0) // can happen in some edge asynchronous scenarios
                {
                    if (File.Exists(dropUponPath)) dropUponPath = Path.GetDirectoryName(dropUponPath);
                    PerformCopyToBulk(infos, dropUponPath);
                }
                DragAndDrop.AcceptDrag();
            }
            return DragAndDropVisualMode.Copy;
        }

        private async void PerformCopyToBulk(List<AssetInfo> infos, string targetPath)
        {
            if (infos.Count == 0) return;

            foreach (AssetInfo info in infos)
            {
                await PerformCopyTo(info, targetPath, true);
            }
            if (AI.Config.pingImported) PingAsset(infos[0]);
        }

#if UNITY_2021_2_OR_NEWER
        private DragAndDropVisualMode OnSceneDrop(Object dropUpon, Vector3 worldPosition, Vector2 viewportPosition, Transform parentForDraggedObjects, bool perform)
        {
            if (perform) StopDragDrop();
            return DragAndDropVisualMode.None;
        }

        private DragAndDropVisualMode OnHierarchyDrop(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            if (perform) StopDragDrop();
            return DragAndDropVisualMode.None;
        }

        private DragAndDropVisualMode OnProjectBrowserDrop(int dragInstanceId, string dropUponPath, bool perform)
        {
            if (perform) StopDragDrop();
            return DragAndDropVisualMode.None;
        }

        private DragAndDropVisualMode OnInspectorDrop(Object[] targets, bool perform)
        {
            if (perform) StopDragDrop();
            return DragAndDropVisualMode.None;
        }
#endif

        private void HandleDragDrop()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                    if (!_mouseOverSearchResultRect) return;
                    if (!_draggingPossible || _dragging || _selectedEntry == null) return;

                    _dragging = true;

                    InitDragAndDrop();
                    DragAndDrop.PrepareStartDrag();

                    if (_sgrid.selectionCount > 0)
                    {
                        DragAndDrop.SetGenericData("AssetInfo", _sgrid.selectionItems);
                        DragAndDrop.objectReferences = _sgrid.selectionItems
                            .Where(item => !string.IsNullOrWhiteSpace(item.ProjectPath))
                            .Select(item => AssetDatabase.LoadMainAssetAtPath(item.ProjectPath))
                            .ToArray();
                    }
                    else
                    {
                        DragAndDrop.SetGenericData("AssetInfo", new List<AssetInfo> {_selectedEntry});
                        if (!string.IsNullOrWhiteSpace(_selectedEntry.ProjectPath))
                        {
                            DragAndDrop.objectReferences = new[] {AssetDatabase.LoadMainAssetAtPath(_selectedEntry.ProjectPath)};
                        }
                    }
                    DragAndDrop.StartDrag("Dragging " + _selectedEntry);
                    Event.current.Use();
                    break;

                case EventType.MouseDown:
                    _draggingPossible = _mouseOverSearchResultRect;
                    break;

                case EventType.MouseUp:
                    _draggingPossible = false;
                    StopDragDrop();
                    break;
            }
        }

        private void StopDragDrop()
        {
            if (_dragging)
            {
                _dragging = false;
                GUIUtility.hotControl = 0; // otherwise scene gizmos are still blocked
                DeinitDragAndDrop();
            }
        }

        private void SearchUpdateLoop()
        {
            if (Time.realtimeSinceStartup > _nextAnimTime
                && _animTexture != null && _selectedEntry != null
                && _sgrid.selectionTile >= 0 && _sgrid.contents != null)
            {
                if (_curAnimFrame > _animFrames.Count) _curAnimFrame = 1;
                Rect frameRect = _animFrames[_curAnimFrame - 1];

                // Extract the current frame as a Texture2D
                Texture2D curTexture = ExtractFrame(_animTexture, frameRect);
                _sgrid.contents[_sgrid.selectionTile].image = curTexture;

                _nextAnimTime = Time.realtimeSinceStartup + AI.Config.animationSpeed;
                _curAnimFrame++;
                if (_curAnimFrame > AI.Config.animationGrid * AI.Config.animationGrid) _curAnimFrame = 1;
            }
        }
    }
}
