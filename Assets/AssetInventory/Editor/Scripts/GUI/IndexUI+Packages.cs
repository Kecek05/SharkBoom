using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.PackageManager;
using UnityEngine;
using static AssetInventory.AssetTreeViewControl;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace AssetInventory
{
    public partial class IndexUI
    {
        private static readonly ProfilerMarker ProfileMarkerBulk = new ProfilerMarker("Bulk Download State");

        private int _visiblePackageCount;
        private int _deprecatedAssetsCount;
        private int _abandonedAssetsCount;
        private int _excludedAssetsCount;
        private int _registryPackageCount;
        private int _subPackageCount;
        private int _customPackageCount;
        private int _selectedMedia;
        private string _assetSearchPhrase;
        private Vector2 _assetsScrollPos;
        private Vector2 _bulkScrollPos;
        private Vector2 _imageScrollPos;
        private Rect _mediaRect;
        private float _nextAssetSearchTime;
        private Rect _versionButtonRect;
        private Rect _sampleButtonRect;
        private bool _mouseOverPackageTreeRect;

        private Vector2 _packageScrollPos;
        private GridControl PGrid
        {
            get
            {
                if (_pgrid == null)
                {
                    _pgrid = new GridControl();
                    _pgrid.OnDoubleClick += OnPackageGridDoubleClicked;
                }
                return _pgrid;
            }
        }
        private GridControl _pgrid;

        private SearchField AssetSearchField => _assetSearchField = _assetSearchField ?? new SearchField();
        private SearchField _assetSearchField;

        [SerializeField] private MultiColumnHeaderState assetMchState;
        private Rect AssetTreeRect => new Rect(20, 0, position.width - 40, position.height - 60);
        private TreeViewWithTreeModel<AssetInfo> AssetTreeView
        {
            get
            {
                if (_assetTreeViewState == null) _assetTreeViewState = new TreeViewState();

                MultiColumnHeaderState headerState = CreateDefaultMultiColumnHeaderState(AssetTreeRect.width);
                if (AI.Config.visiblePackageTreeColumns != null && AI.Config.visiblePackageTreeColumns.Length > 0)
                {
                    headerState.visibleColumns = AI.Config.visiblePackageTreeColumns;
                }
                else
                {
                    headerState.visibleColumns = new[] {(int)Columns.Name, (int)Columns.Tags, (int)Columns.Version, (int)Columns.Indexed};
                }
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(assetMchState, headerState)) MultiColumnHeaderState.OverwriteSerializedFields(assetMchState, headerState);
                assetMchState = headerState;

                if (_assetTreeView == null)
                {
                    MultiColumnHeader mch = new MultiColumnHeader(headerState);
                    mch.canSort = true;
                    mch.height = MultiColumnHeader.DefaultGUI.minimumHeight;
                    mch.visibleColumnsChanged += OnVisibleAssetTreeColumnsChanged;
                    mch.sortingChanged += OnAssetTreeSortingChanged;
                    mch.ResizeToFit();

                    _assetTreeView = new AssetTreeViewControl(_assetTreeViewState, mch, AssetTreeModel);
                    _assetTreeView.OnSelectionChanged += OnAssetTreeSelectionChanged;
                    _assetTreeView.OnDoubleClickedItem += OnAssetTreeDoubleClicked;
                    _assetTreeView.Reload();
                }
                return _assetTreeView;
            }
        }

        private void OnAssetTreeSortingChanged(MultiColumnHeader mch)
        {
            AI.Config.assetSorting = mch.sortedColumnIndex;
            AI.Config.sortAssetsDescending = !mch.IsSortedAscending(mch.sortedColumnIndex);
            AI.SaveConfig();
            CreateAssetTree();
        }

        private void OnVisibleAssetTreeColumnsChanged(MultiColumnHeader mch)
        {
            mch.ResizeToFit();

            AI.Config.visiblePackageTreeColumns = mch.state.visibleColumns;
            AI.SaveConfig();
        }

        private TreeViewWithTreeModel<AssetInfo> _assetTreeView;
        private TreeViewState _assetTreeViewState;

        private TreeModel<AssetInfo> AssetTreeModel
        {
            get
            {
                if (_assetTreeModel == null) _assetTreeModel = new TreeModel<AssetInfo>(new List<AssetInfo> {new AssetInfo().WithTreeData("Root", depth: -1)});
                return _assetTreeModel;
            }
        }
        private TreeModel<AssetInfo> _assetTreeModel;

        private AssetInfo _selectedTreeAsset;
        private List<AssetInfo> _selectedTreeAssets;

        private long _assetTreeSelectionSize;
        private long _assetTreeSubPackageCount;
        private float _assetTreeSelectionTotalCosts;
        private float _assetTreeSelectionStoreCosts;
        private readonly Dictionary<string, Tuple<int, Color>> _assetBulkTags = new Dictionary<string, Tuple<int, Color>>();
        private int _packageDetailsTab;

        private void OnPackageListUpdated()
        {
            if (_assets == null) return;

            _requireAssetTreeRebuild = true;

            Dictionary<string, PackageInfo> packages = AssetStore.GetAllPackages();
            bool hasChanges = false;
            foreach (KeyValuePair<string, PackageInfo> package in packages)
            {
                AssetInfo info = _assets.FirstOrDefault(a => a.AssetSource == Asset.Source.RegistryPackage && a.SafeName == package.Value.name);
                if (info == null)
                {
                    // new package found, persist
                    if (PackageImporter.Persist(package.Value))
                    {
                        hasChanges = true;
                    }
                    continue;
                }

                info.Refresh();
                if (package.Value.versions.latestCompatible != info.LatestVersion && !package.Value.versions.latestCompatible.ToLowerInvariant().Contains("pre"))
                {
                    AI.SetPackageVersion(info, package.Value);
                    hasChanges = true;
                }
            }
            if (hasChanges)
            {
                _requireLookupUpdate = ChangeImpact.Write;
                _requireAssetTreeRebuild = true;
            }
        }

        private void OnTagsChanged()
        {
            _tags = Tagging.LoadTags();
            _tagNames = AI.ExtractTagNames(_tags);

            _requireAssetTreeRebuild = true;
        }

        private void OnIndexingDone()
        {
            ReloadLookups();
            _requireAssetTreeRebuild = true;
        }

        private void DrawPackageDownload(AssetInfo info, bool updateMode = false)
        {
            if (!string.IsNullOrEmpty(info.OriginalLocation))
            {
                if (!updateMode)
                {
                    if (info.IsLocationUnmappedRelative())
                    {
                        EditorGUILayout.HelpBox("The location of this package is stored relative and no mapping has been done yet in the settings for this system.", MessageType.Warning);
                    }
                    else if (string.IsNullOrWhiteSpace(info.DownloadedActual))
                    {
                        if (info.PackageSize > 0)
                        {
                            EditorGUILayout.HelpBox($"Not cached currently. Download the asset to access its content ({EditorUtility.FormatBytes(info.PackageSize)}).", MessageType.Warning);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Not cached currently. Download the asset to access its content.", MessageType.Warning);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"Cache currently contains version {info.DownloadedActual} of a different listing for this package. Download this package to override it.", MessageType.Warning);
                    }
                }

                if (!info.IsDownloadedCompatible)
                {
                    if (info.IsCurrentUnitySupported())
                    {
                        UIBlock("package.hints.incompatibledownload", () => EditorGUILayout.HelpBox($"The cached package is meant for a newer version of Unity ({info.DownloadededUnityVersion}) and might be incompatible. Try downloading the package again to get a compatible version if available.", MessageType.Warning));
                    }
                    else
                    {
                        UIBlock("package.hints.incompatibledownload", () => EditorGUILayout.HelpBox($"The cached package is meant for a newer version of Unity ({info.DownloadededUnityVersion}) and might be incompatible.", MessageType.Warning));
                    }
                }

                if (info.ParentId == 0 && info.PackageDownloader != null)
                {
                    AssetDownloadState state = info.PackageDownloader.GetState();
                    switch (state.state)
                    {
                        case AssetDownloader.State.Downloading:
                            GUILayout.BeginHorizontal();
                            UIStyles.DrawProgressBar(state.progress, $"{EditorUtility.FormatBytes(state.bytesDownloaded)}");
                            if (GUILayout.Button(EditorGUIUtility.IconContent("d_PauseButton On", "|Pause"), GUILayout.ExpandWidth(false))) info.PackageDownloader.PauseDownload(false);
                            if (GUILayout.Button(EditorGUIUtility.IconContent("d_PreMatQuad", "|Abort"), GUILayout.ExpandWidth(false))) info.PackageDownloader.PauseDownload(true);
                            GUILayout.EndHorizontal();
                            break;

                        case AssetDownloader.State.Unavailable:
                            if (info.PackageDownloader.IsDownloadSupported() && GUILayout.Button("Download")) info.PackageDownloader.Download();
                            break;

                        case AssetDownloader.State.Paused:
                            if (info.PackageDownloader.IsDownloadSupported() && GUILayout.Button("Resume Download" + (info.PackageSize > 0 ? $" ({EditorUtility.FormatBytes(info.PackageSize - info.PackageDownloader.GetState().bytesDownloaded)})" : ""))) info.PackageDownloader.Download();
                            break;

                        case AssetDownloader.State.UpdateAvailable:
                            if (info.PackageDownloader.IsDownloadSupported() && GUILayout.Button("Download Update" + (info.PackageSize > 0 ? $" ({EditorUtility.FormatBytes(info.PackageSize)})" : "")))
                            {
                                info.WasOutdated = true;
                                info.PackageDownloader.SetAsset(info);
                                info.PackageDownloader.Download();
                            }
                            break;

                        case AssetDownloader.State.Downloaded:
                            if (!info.IsDownloadedCompatible && info.IsCurrentUnitySupported())
                            {
                                info.PackageDownloader.SetAsset(info);
                                if (info.PackageDownloader.IsDownloadSupported() && GUILayout.Button("Download")) info.PackageDownloader.Download();
                            }
                            break;
                    }
                }
            }
            else
            {
                if (!updateMode)
                {
                    if (info.IsLocationUnmappedRelative())
                    {
                        EditorGUILayout.HelpBox("The location of this package is stored relative and no mapping has been done yet in the settings for this system.", MessageType.Warning);
                    }
                    else if (info.AssetSource == Asset.Source.CustomPackage && !File.Exists(info.GetLocation(true)))
                    {
                        EditorGUILayout.HelpBox("The custom package has been deleted and is not available anymore.", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This package is new and metadata has not been collected yet. Update the index to have all metadata up to date.", MessageType.Warning);
                        if (GUILayout.Button(UIStyles.Content("Load Metadata"))) FetchAssetDetails(true, info.AssetId);
                    }
                }
                else if (info.AssetSource == Asset.Source.CustomPackage)
                {
                    UIBlock("package.hints.noautoupdate", () => EditorGUILayout.HelpBox("Automatic update not possible since package is not from the Asset Store.", MessageType.Info));
                }
            }
        }

        private void DrawPackageDetails(AssetInfo info, bool showMaintenance = false, bool showActions = true, bool startNewSection = true)
        {
            if (info.AssetId == 0)
            {
                EditorGUILayout.HelpBox("This asset has no package association anymore. Use the maintenance wizard to clean up such orphaned files.", MessageType.Error);
                return;
            }

            bool showExpanded = AI.Config.expandPackageDetails && AI.Config.tab == 1;
            int labelWidth = 95;
            if (startNewSection)
            {
                GUILayout.BeginVertical("Package Details", "window", GUILayout.Width(GetInspectorWidth()), GUILayout.ExpandWidth(false));
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.LabelField("Package", EditorStyles.largeLabel);
            }
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            if (info.AssetSource == Asset.Source.AssetManager)
            {
                UIBlock("package.organization", () =>
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Organization", EditorStyles.boldLabel, GUILayout.Width(labelWidth - 2));
                    if (GUILayout.Button(UIStyles.Content(info.OriginalLocation), UIStyles.wrappedLinkLabel, GUILayout.ExpandWidth(true))) Application.OpenURL(info.GetAMOrganizationUrl());
                    GUILayout.EndHorizontal();
                });

                UIBlock("package.project", () =>
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Project", EditorStyles.boldLabel, GUILayout.Width(labelWidth - 2));
                    if (GUILayout.Button(UIStyles.Content(info.ToAsset().GetRootAsset().DisplayName), UIStyles.wrappedLinkLabel, GUILayout.ExpandWidth(true))) Application.OpenURL(info.GetAMProjectUrl());
                    GUILayout.EndHorizontal();
                });

                if (info.ParentId > 0)
                {
                    UIBlock("package.collection", () =>
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Collection", EditorStyles.boldLabel, GUILayout.Width(labelWidth - 2));
                        if (GUILayout.Button(UIStyles.Content(info.GetDisplayName()), UIStyles.wrappedLinkLabel, GUILayout.ExpandWidth(true))) Application.OpenURL(info.GetAMCollectionUrl());
                        GUILayout.EndHorizontal();
                    });
                }
            }
            else
            {
                GUILabelWithText("Name", info.GetDisplayName(), labelWidth, info.Location, true);
            }
            if (info.AssetSource == Asset.Source.RegistryPackage)
            {
                UIBlock("package.id", () => GUILabelWithText("Id", info.SafeName, labelWidth, info.SafeName, true, true));

                if (info.PackageSource == PackageSource.Local)
                {
                    string version = info.GetVersion(true);
                    UIBlock("package.version", () => GUILabelWithText("Version", string.IsNullOrWhiteSpace(version) ? "-none-" : version, labelWidth));
                }
                else
                {
                    UIBlock("package.version", () =>
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Version", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        if (EditorGUILayout.DropdownButton(AssetStore.IsInstalled(info) ? UIStyles.Content(info.InstalledPackageVersion(), "Version to use") : UIStyles.Content("Not installed, select version"), FocusType.Keyboard, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300)))
                        {
                            VersionSelectionUI versionUI = new VersionSelectionUI();
                            versionUI.Init(info, newVersion =>
                            {
                                InstallPackage(info, newVersion);
                            });
                            PopupWindow.Show(_versionButtonRect, versionUI);
                        }
                        if (Event.current.type == EventType.Repaint) _versionButtonRect = GUILayoutUtility.GetLastRect();
                        if (AssetStore.IsInstalled(info))
                        {
                            string changeLogURL = info.GetChangeLogURL(info.InstalledPackageVersion());
                            if (!string.IsNullOrWhiteSpace(changeLogURL))
                            {
                                if (GUILayout.Button(UIStyles.Content("?", "Changelog"), GUILayout.Width(20)))
                                {
                                    Application.OpenURL(changeLogURL);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    });

                    UIBlock("package.updatestrategy", () =>
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Updates", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        EditorGUI.BeginChangeCheck();
                        info.UpdateStrategy = (Asset.Strategy)EditorGUILayout.EnumPopup(info.UpdateStrategy, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(300));
                        if (EditorGUI.EndChangeCheck())
                        {
                            AI.SetAssetUpdateStrategy(info, info.UpdateStrategy);
                            _requireAssetTreeRebuild = true;
                        }
                        GUILayout.EndHorizontal();
                    });
                }
            }
            if (!string.IsNullOrWhiteSpace(info.License))
            {
                UIBlock("package.license", () =>
                {
                    if (!string.IsNullOrWhiteSpace(info.LicenseLocation))
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("License", EditorStyles.boldLabel, GUILayout.Width(labelWidth - 2));
                        if (GUILayout.Button(UIStyles.Content(info.License), UIStyles.wrappedLinkLabel, GUILayout.ExpandWidth(true))) Application.OpenURL(info.LicenseLocation);
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILabelWithText("License", $"{info.License}");
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(info.GetDisplayPublisher()))
            {
                UIBlock("package.publisher", () =>
                {
                    if (info.PublisherId > 0)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Publisher", EditorStyles.boldLabel, GUILayout.Width(labelWidth - 2));
                        if (GUILayout.Button(UIStyles.Content(info.GetDisplayPublisher()), UIStyles.wrappedLinkLabel, GUILayout.ExpandWidth(true))) Application.OpenURL(info.GetPublisherLink());
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILabelWithText("Publisher", $"{info.GetDisplayPublisher()}", 95, null, true);
                    }
                });
            }
            if (!string.IsNullOrWhiteSpace(info.GetDisplayCategory())) UIBlock("package.category", () => GUILabelWithText("Category", $"{info.GetDisplayCategory()}", 95, null, true));
            if (info.PackageSize > 0) UIBlock("package.size", () => GUILabelWithText("Size", EditorUtility.FormatBytes(info.PackageSize)));
            if (!string.IsNullOrWhiteSpace(info.SupportedUnityVersions))
            {
                if ((info.AssetSource != Asset.Source.AssetStorePackage && info.AssetSource != Asset.Source.CustomPackage) || info.IsCurrentUnitySupported())
                {
                    UIBlock("package.unityversions", () => GUILabelWithText("Unity", info.SupportedUnityVersions, labelWidth, null, true));
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Unity", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                    EditorGUILayout.LabelField(info.SupportedUnityVersions, EditorStyles.wordWrappedLabel, GUILayout.Width(108));
                    EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon@2x", "|Package is potentially incompatible to the current Unity version"), GUILayout.Width(24));
                    GUILayout.EndHorizontal();
                }
            }
            UIBlock("package.srps", () =>
            {
                if (info.BIRPCompatible || info.URPCompatible || info.HDRPCompatible)
                {
                    GUILabelWithText("SRPs", (info.BIRPCompatible ? "BIRP " : "") + (info.URPCompatible ? "URP " : "") + (info.HDRPCompatible ? "HDRP" : ""));
                }
            });
            if (info.FirstRelease.Year > 1) UIBlock("package.releasedate", () => GUILabelWithText("Released", info.FirstRelease.ToString("ddd, MMM d yyyy")));
            if (info.PurchaseDate.Year > 1) UIBlock("package.purchasedate", () => GUILabelWithText("Purchased", info.PurchaseDate.ToString("ddd, MMM d yyyy")));
            if (info.LastRelease.Year > 1)
            {
                UIBlock("package.lastupdate", () => GUILabelWithText("Last Update", info.LastRelease.ToString("ddd, MMM d yyyy") + (!string.IsNullOrEmpty(info.LatestVersion) ? $" ({info.LatestVersion})" : string.Empty)));
            }
            else if (!string.IsNullOrEmpty(info.LatestVersion))
            {
                UIBlock("package.latestversion", () => GUILabelWithText("Latest Version", info.LatestVersion));
            }
            UIBlock("package.price", () =>
            {
                string price = info.GetPrice() > 0 ? info.GetPriceText() : "Free";
                GUILabelWithText("Price", price);
            });
            if (!string.IsNullOrWhiteSpace(info.AssetRating))
            {
                UIBlock("package.rating", () =>
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(UIStyles.Content("Rating", $"Rating given by Asset Store users ({info.AssetRating}, Hot value {info.Hotness})"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                    if (int.TryParse(info.AssetRating, out int rating))
                    {
                        if (rating <= 0)
                        {
                            EditorGUILayout.LabelField("Not enough ratings", GUILayout.MaxWidth(113));
                        }
                        else
                        {
                            Color oldCC = GUI.contentColor;
                            float size = EditorGUIUtility.singleLineHeight;
#if UNITY_2021_1_OR_NEWER
                            // favicon is not gold anymore                    
                            GUI.contentColor = new Color(0.992f, 0.694f, 0.004f);
#endif
                            for (int i = 0; i < rating; i++)
                            {
                                GUILayout.Button(EditorGUIUtility.IconContent("Favorite Icon"), EditorStyles.label, GUILayout.Width(size), GUILayout.Height(size));
                            }
                            GUI.contentColor = oldCC;
                            for (int i = rating; i < 5; i++)
                            {
                                GUILayout.Button(EditorGUIUtility.IconContent("Favorite"), EditorStyles.label, GUILayout.Width(size), GUILayout.Height(size));
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"{info.AssetRating} ");
                    }
                    EditorGUILayout.LabelField($"({info.RatingCount} ratings)", GUILayout.MaxWidth(81));
                    GUILayout.EndHorizontal();
                });
            }
            if (ShowAdvanced() || AI.Config.tab == 1)
            {
                UIBlock("package.indexedfiles", () => GUILabelWithText("Indexed Files", $"{info.FileCount:N0}"), info.AssetSource == Asset.Source.Directory || info.AssetSource == Asset.Source.AssetManager || info.AssetSource == Asset.Source.Archive);
            }
            if (info.ChildInfos.Count > 0)
            {
                UIBlock("package.childcount", () => GUILabelWithText(info.AssetSource == Asset.Source.AssetManager ? "Collections" : "Sub-Packages", $"{info.ChildInfos.Count:N0}" + (info.CurrentState == Asset.State.SubInProcess ? " (reindexing pending)" : "")));
            }

            UIBlock("package.source", () =>
            {
                string packageTooltip = $"IDs: Asset ({info.AssetId}), Foreign ({info.ForeignId}), Upload ({info.UploadId})\n\nCurrent State: {info.CurrentState}\n\nResolved Location: {info.GetLocation(true)}";
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(UIStyles.Content("Source", packageTooltip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                switch (info.AssetSource)
                {
                    case Asset.Source.AssetStorePackage:
                        if (info.ForeignId > 0)
                        {
                            if (GUILayout.Button(UIStyles.Content("Asset Store"), EditorStyles.linkLabel)) Application.OpenURL(info.GetItemLink());
                        }
                        else
                        {
                            EditorGUILayout.LabelField(UIStyles.Content("Asset Store", packageTooltip), UIStyles.GetLabelMaxWidth());
                        }
                        break;

                    case Asset.Source.RegistryPackage:
                        if (info.ForeignId > 0)
                        {
                            if (GUILayout.Button(UIStyles.Content("Asset Store"), EditorStyles.linkLabel)) Application.OpenURL(info.GetItemLink());
                        }
                        else if (info.IsFeaturePackage())
                        {
                            EditorGUILayout.LabelField(UIStyles.Content("Unity Feature (Package Bundle)"), UIStyles.GetLabelMaxWidth());
                        }
                        else
                        {
                            EditorGUILayout.LabelField(UIStyles.Content($"{StringUtils.CamelCaseToWords(info.AssetSource.ToString())} ({info.PackageSource})", info.SafeName), UIStyles.GetLabelMaxWidth());
                        }
                        break;

                    default:
                        EditorGUILayout.LabelField(UIStyles.Content(StringUtils.CamelCaseToWords(info.AssetSource.ToString()), packageTooltip), UIStyles.GetLabelMaxWidth());
                        break;
                }
                GUILayout.EndHorizontal();
            });
            if (info.AssetSource != Asset.Source.AssetStorePackage && info.AssetSource != Asset.Source.RegistryPackage && info.ForeignId > 0)
            {
                UIBlock("package.sourcelink", () =>
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Asset Link", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                    if (GUILayout.Button(UIStyles.Content("Asset Store"), EditorStyles.linkLabel)) Application.OpenURL(info.GetItemLink());
                    GUILayout.EndHorizontal();
                });
            }

            if (showMaintenance)
            {
                if (AI.Config.createBackups && info.AssetSource != Asset.Source.RegistryPackage && info.ParentId == 0)
                {
                    UIBlock("package.backup", () =>
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(UIStyles.Content("Backup", "Activate to create backups for this asset (done after every update cycle)."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                        EditorGUI.BeginChangeCheck();
                        info.Backup = EditorGUILayout.Toggle(info.Backup);
                        if (EditorGUI.EndChangeCheck()) AI.SetAssetBackup(info, info.Backup);
                        GUILayout.EndHorizontal();
                    });
                }
            }

            if (AI.Config.createAICaptions)
            {
                UIBlock("package.aiusage", () =>
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(UIStyles.Content("AI Captions", "Activate to create backups for this asset (done after every update cycle)."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                    EditorGUI.BeginChangeCheck();
                    info.UseAI = EditorGUILayout.Toggle(info.UseAI);
                    if (EditorGUI.EndChangeCheck()) AI.SetAssetAIUse(info, info.UseAI);
                    GUILayout.EndHorizontal();
                });
            }

            if (info.AssetSource == Asset.Source.CustomPackage || info.AssetSource == Asset.Source.Archive || info.AssetSource == Asset.Source.AssetStorePackage)
            {
                UIBlock("package.extract", () =>
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(UIStyles.Content("Extract", "Will keep the package extracted in the cache to minimize access delays at the cost of more hard disk space."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                    EditorGUI.BeginChangeCheck();
                    info.KeepExtracted = EditorGUILayout.Toggle(info.KeepExtracted);
                    if (EditorGUI.EndChangeCheck()) AI.SetAssetExtraction(info, info.KeepExtracted);
                    GUILayout.EndHorizontal();
                });
            }

            UIBlock("package.exclude", () =>
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(UIStyles.Content("Exclude", "Will not index the asset and not show existing index results in the search."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                EditorGUI.BeginChangeCheck();
                info.Exclude = EditorGUILayout.Toggle(info.Exclude);
                if (EditorGUI.EndChangeCheck())
                {
                    AI.SetAssetExclusion(info, info.Exclude);
                    _requireLookupUpdate = ChangeImpact.Write;
                    _requireSearchUpdate = true;
                    _requireAssetTreeRebuild = true;
                }
                GUILayout.EndHorizontal();
            });
            GUILayout.EndVertical();
            if (showExpanded && info.PreviewTexture != null)
            {
                UIBlock("package.topicon", () =>
                {
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    DrawPackagePreview(info);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                });
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (info.SafeName == Asset.NONE) UIBlock("package.hints.noname", () => EditorGUILayout.HelpBox("This is an automatically created package for managing indexed media files that are not associated with any other package.", MessageType.Info));
            if (info.ParentInfo != null)
            {
                UIBlock("package.hints.subpackage", () => EditorGUILayout.HelpBox($"This is a sub-package inside '{info.ParentInfo.GetDisplayName()}'.", MessageType.Info));
                if (AI.Config.tab == 0 && _selectedAsset > 0)
                {
                    UIBlock("package.actions.showparent", () =>
                    {
                        if (GUILayout.Button("Show Parent Package")) OpenInSearch(info.ParentInfo, true);
                    });
                }
            }
            if (info.IsDeprecated) UIBlock("package.hints.deprecation", () => EditorGUILayout.HelpBox("This asset is deprecated.", MessageType.Warning));
            if (info.IsAbandoned) UIBlock("package.hints.abandoned", () => EditorGUILayout.HelpBox("This asset is no longer available for download.", MessageType.Error));
#if !USE_ASSET_MANAGER || !USE_CLOUD_IDENTITY
            if (info.AssetSource == Asset.Source.AssetManager)
            {
                UIBlock("package.hints.noassetmanager", () => EditorGUILayout.HelpBox("This package links to the Unity Asset Manager but the SDK is not installed. No actions will be possible.", MessageType.Info));
            }
#endif
            if (showActions)
            {
                EditorGUI.BeginDisabledGroup(AI.CurrentMain != null || AI.IndexingInProgress);

                bool showDelete = false;
                if (info.CurrentSubState == Asset.SubState.Outdated)
                {
                    AssetDownloader.State? state = info.PackageDownloader?.GetState().state;
                    if (state == AssetDownloader.State.Downloaded || state == AssetDownloader.State.UpdateAvailable)
                    {
                        showDelete = true;
                        UIBlock("package.hints.outdated", () => EditorGUILayout.HelpBox("This asset is outdated in the cache. It is recommended to delete it from the database and the file system.", MessageType.Info));
                    }
                }
                if (info.AssetSource == Asset.Source.AssetStorePackage
                    || info.AssetSource == Asset.Source.CustomPackage
                    || info.AssetSource == Asset.Source.AssetManager
                    || info.AssetSource == Asset.Source.RegistryPackage
                    || info.AssetSource == Asset.Source.Archive
                    || (info.AssetSource == Asset.Source.Directory && info.SafeName != Asset.NONE))
                {
                    EditorGUILayout.Space();
                    if (info.AssetSource == Asset.Source.RegistryPackage)
                    {
                        if (info.IsIndirectPackageDependency())
                        {
                            UIBlock("package.hints.indirectdependency", () =>
                            {
                                EditorGUILayout.HelpBox("This package is an indirect dependency and changing the version will decouple it from the dependency lifecycle which can potentially lead to issues.", MessageType.Info);
                                EditorGUILayout.Space();
                            });
                        }
                        if (info.InstalledPackageVersion() != null)
                        {
                            if (info.TargetPackageVersion() != null)
                            {
                                if (info.InstalledPackageVersion() != info.TargetPackageVersion())
                                {
                                    UIBlock("package.actions.update", () =>
                                    {
                                        EditorGUILayout.BeginHorizontal();

                                        string command;
                                        string tooltip;
                                        if (new SemVer(info.InstalledPackageVersion()) > new SemVer(info.TargetPackageVersion()))
                                        {
                                            command = "Downgrade";
                                            tooltip = "Downgrade package to a compatible version calculated from the selected update strategy.";
                                        }
                                        else
                                        {
                                            command = "Update";
                                            tooltip = "Update package to the version calculated from the selected update strategy.";
                                        }
                                        if (GUILayout.Button(UIStyles.Content($"{command} to {info.TargetPackageVersion()}", tooltip)))
                                        {
                                            ImportUI importUI = ImportUI.ShowWindow();
                                            importUI.Init(new List<AssetInfo> {info}, true);
                                        }
                                        string changeLogURL = info.GetChangeLogURL(info.TargetPackageVersion());
                                        if (!string.IsNullOrWhiteSpace(changeLogURL) && GUILayout.Button(UIStyles.Content("?", "Changelog"), GUILayout.Width(20)))
                                        {
                                            Application.OpenURL(changeLogURL);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    });
                                }
                            }
                            if (info.HasSamples())
                            {
                                UIBlock("package.actions.samples", () =>
                                {
                                    if (GUILayout.Button(UIStyles.Content("Add/Remove Samples...")))
                                    {
                                        SampleSelectionUI samplesUI = new SampleSelectionUI();
                                        samplesUI.Init(info);
                                        PopupWindow.Show(_sampleButtonRect, samplesUI);
                                    }
                                    if (Event.current.type == EventType.Repaint) _sampleButtonRect = GUILayoutUtility.GetLastRect();
                                });
                            }

                            UIBlock("package.actions.remove", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Uninstall Package", "Remove package from current project.")))
                                {
                                    Client.Remove(info.SafeName);
                                    AssetStore.GatherProjectMetadata();
                                }
                                EditorGUILayout.Space();
                            });
                        }
                        else if (info.TargetPackageVersion() != null)
                        {
                            UIBlock("package.actions.install", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content($"Install Version {info.TargetPackageVersion()}", "Installs package into the current project.")))
                                {
                                    ImportUI importUI = ImportUI.ShowWindow();
                                    importUI.Init(new List<AssetInfo> {info}, true);
                                }
                            });
                        }
                        else if (info.PackageSource == PackageSource.Local)
                        {
                            UIBlock("package.actions.install", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Install (link locally)", "Links package to the current project.")))
                                {
                                    ImportUI importUI = ImportUI.ShowWindow();
                                    importUI.Init(new List<AssetInfo> {info}, true);
                                }
                            });
                            UIBlock("package.actions.openlocation", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Open Package Location..."))) ShowInExplorer(info);
                            });
                        }
                        else if (info.PackageSource == PackageSource.Git)
                        {
                            UIBlock("package.actions.install", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Install Indexed Version", info.LatestVersion)))
                                {
                                    InstallPackage(info, info.LatestVersion);
                                }
                            });
                        }
                    }
                    else if (info.AssetSource != Asset.Source.AssetManager && info.IsDownloaded)
                    {
                        if (info.AssetSource != Asset.Source.Directory)
                        {
                            if (showMaintenance && (info.IsUpdateAvailable(_assets) || info.WasOutdated || !info.IsDownloadedCompatible))
                            {
                                DrawPackageDownload(info, true);
                            }
                            if (AssetStore.IsInstalled(info))
                            {
                                UIBlock("package.actions.remove", () =>
                                {
                                    if (GUILayout.Button("Remove Package")) RemovePackage(info);
                                });
                            }
                            else
                            {
                                UIBlock("package.actions.import", () =>
                                {
                                    if (GUILayout.Button(UIStyles.Content("Import Package...", "Open import dialog")))
                                    {
                                        ImportUI importUI = ImportUI.ShowWindow();
                                        importUI.Init(new List<AssetInfo> {info});
                                    }
                                });
                                UIBlock("package.actions.openlocation", () =>
                                {
                                    if (GUILayout.Button(UIStyles.Content("Open Package Location..."))) ShowInExplorer(info);
                                });
                            }
                        }
                        else
                        {
                            UIBlock("package.actions.openlocation", () =>
                            {
                                string locName = info.AssetSource == Asset.Source.Archive ? "Archive" : "Directory";
                                if (GUILayout.Button(UIStyles.Content($"Open {locName} Location..."))) ShowInExplorer(info);
                            });
                        }
                    }
                    if (info.ForeignId > 0 || info.AssetSource == Asset.Source.RegistryPackage)
                    {
                        UIBlock("package.actions.openinpackagemanager", () =>
                        {
                            if (GUILayout.Button(UIStyles.Content("Open in Package Manager...")))
                            {
                                AssetStore.OpenInPackageManager(info);
                            }
                        });
                    }

                    if (AI.Config.tab == 0 && _selectedAsset == 0)
                    {
                        UIBlock("package.actions.filter", () =>
                        {
                            if (GUILayout.Button("Filter for this package only")) OpenInSearch(info, true);
                        });
                    }
                    if (AI.Config.tab != 1)
                    {
                        UIBlock("package.actions.packageview", () =>
                        {
                            if (GUILayout.Button("Show in Package View")) OpenInPackageView(info);
                        });
                    }
                    if (AI.Config.tab > 0 && info.IsIndexed && info.FileCount > 0)
                    {
                        UIBlock("package.actions.openinsearch", () =>
                        {
                            if (GUILayout.Button("Open in Search")) OpenInSearch(info);
                        });
                    }
#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
                    if (showMaintenance)
                    {
                        if (info.AssetSource == Asset.Source.AssetManager)
                        {
                            EditorGUI.BeginDisabledGroup(CloudAssetManagement.IsBusy);
                            EditorGUILayout.Space();
                            UIBlock("package.actions.createcollection", () =>
                            {
                                if (GUILayout.Button("Create Collection..."))
                                {
                                    NameUI nameUI = new NameUI();
                                    nameUI.Init("New Collection", colName => CreateCollection(info, colName));
                                    PopupWindow.Show(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), nameUI);
                                }
                            });
                            if (info.ParentInfo != null)
                            {
                                UIBlock("package.actions.deletecollection", () =>
                                {
                                    if (GUILayout.Button("Delete Collection"))
                                    {
                                        DeleteCollection(info);
                                    }
                                });
                            }
                            if (!ShowAdvanced()) EditorGUILayout.Space();
                            EditorGUI.EndDisabledGroup();
                        }
                    }
#endif
                    if (showMaintenance)
                    {
                        if (AI.Config.tab > 0 && info.IsIndexed && info.FileCount > 0)
                        {
                            UIBlock("package.actions.reindexnextrun", () =>
                            {
                                EditorGUILayout.Space();
                                if (info.IsDownloaded && GUILayout.Button(UIStyles.Content("Reindex Package on Next Run", "Will mark this package as outdated and force a reindex the next time Update Index is called on the Settings tab.")))
                                {
                                    AI.ForgetPackage(info, true);
                                    _requireLookupUpdate = ChangeImpact.Write;
                                    _requireSearchUpdate = true;
                                    _requireAssetTreeRebuild = true;
                                }
                            });
                        }
                        if (info.IsDownloaded)
                        {
                            UIBlock("package.actions.reindexnow", () =>
                            {
                                string reindexCaption = info.IsIndexed ? "Reindex" : "Index";
                                if (GUILayout.Button(UIStyles.Content($"{reindexCaption} Package Now", "Will instantly delete the existing index and reindex the full package.")))
                                {
                                    AI.ForgetPackage(info, true);
                                    AI.RefreshIndex(info);
                                    _requireLookupUpdate = ChangeImpact.Write;
                                    _requireSearchUpdate = true;
                                    _requireAssetTreeRebuild = true;
                                }
                            });
                        }
                        if (info.ForeignId > 0)
                        {
                            UIBlock("package.actions.refreshmetadata", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Refresh Metadata", "Will fetch most up-to-date metadata from the Asset Store."))) FetchAssetDetails(true, info.AssetId);
                            });
                        }
                    }
                    if (info.IsDownloaded)
                    {
                        if (info.IsIndexed && info.FileCount > 0)
                        {
                            UIBlock("package.actions.recreatepreviews", () =>
                            {
                                if (GUILayout.Button("Previews Wizard..."))
                                {
                                    PreviewWizardUI previewsUI = PreviewWizardUI.ShowWindow();
                                    previewsUI.Init(new List<AssetInfo> {info}, _assets);
                                }
                            });
                        }
                    }
                    else if (info.AssetSource == Asset.Source.CustomPackage || info.AssetSource == Asset.Source.Archive || info.AssetSource == Asset.Source.Directory)
                    {
                        showDelete = true;
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("This package does not exist anymore on the file system and was probably deleted.", MessageType.Error);
                    }
                    else if (!info.IsAbandoned)
                    {
                        EditorGUILayout.Space();
                        DrawPackageDownload(info);
                    }
                    if (showMaintenance && (info.AssetSource == Asset.Source.CustomPackage || info.AssetSource == Asset.Source.Archive))
                    {
                        if (info.ForeignId <= 0)
                        {
                            UIBlock("package.actions.connecttoassetstore", () =>
                            {
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Connect to Asset Store..."))
                                {
                                    AssetConnectionUI assetUI = new AssetConnectionUI();
                                    assetUI.Init(details => ConnectToAssetStore(info, details));
                                    PopupWindow.Show(_connectButtonRect, assetUI);
                                }
                                if (Event.current.type == EventType.Repaint) _connectButtonRect = GUILayoutUtility.GetLastRect();
                                if (GUILayout.Button("Edit Data..."))
                                {
                                    PackageUI packageUI = PackageUI.ShowWindow();
                                    packageUI.Init(info, _ =>
                                    {
                                        _requireAssetTreeRebuild = true;
                                        UpdateStatistics(true); // to reload asset data
                                    });
                                }
                                GUILayout.EndHorizontal();
                            });
                        }
                    }
                }
                if (showMaintenance)
                {
                    if (info.AssetSource != Asset.Source.RegistryPackage)
                    {
                        UIBlock("package.actions.export", () =>
                        {
                            if (GUILayout.Button("Export Package..."))
                            {
                                ExportUI exportUI = ExportUI.ShowWindow();
                                exportUI.Init(_selectedTreeAssets, 1);
                            }
                            EditorGUILayout.Space();
                        });
                    }

                    if (info.ForeignId > 0 && (info.AssetSource == Asset.Source.CustomPackage || info.AssetSource == Asset.Source.Archive))
                    {
                        UIBlock("package.actions.removeassetstoreconnection", () =>
                        {
                            if (GUILayout.Button("Remove Asset Store Connection"))
                            {
                                bool removeMetadata = EditorUtility.DisplayDialog("Remove Metadata", "Remove or keep the additional metadata from the Asset Store like ratings, category etc.?", "Remove", "Keep");
                                AI.DisconnectFromAssetStore(info, removeMetadata);
                                _requireAssetTreeRebuild = true;
                            }
                        });
                    }
                    if (info.AssetSource != Asset.Source.RegistryPackage && info.AssetSource != Asset.Source.AssetManager)
                    {
                        if (info.ParentId <= 0 && info.IsDownloaded && info.SafeName != Asset.NONE)
                        {
                            UIBlock("package.actions.delete", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Delete Package...", "Delete the package from the database and optionally the file system.")))
                                {
                                    int removeType = EditorUtility.DisplayDialogComplex("Delete Package", "Do you also want to remove the file from the Unity cache? If not the package will reappear after the next index update.", "Remove only from Database", "Cancel", "Remove also from File System");
                                    if (removeType != 1)
                                    {
                                        AI.RemovePackage(info, removeType == 2);
                                        _requireLookupUpdate = ChangeImpact.Write;
                                        _requireAssetTreeRebuild = true;
                                    }
                                }
                            }, showDelete);
                        }
                        else if (info.ParentId > 0 || !info.IsDownloaded || info.SafeName == Asset.NONE)
                        {
                            UIBlock("package.actions.delete", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Delete Package", "Delete the package from the database.")))
                                {
                                    AI.RemovePackage(info, false);
                                    _requireLookupUpdate = ChangeImpact.Write;
                                    _requireAssetTreeRebuild = true;
                                }
                            });
                        }

                        if (info.ParentId <= 0 && info.IsDownloaded && info.AssetSource != Asset.Source.Directory)
                        {
                            UIBlock("package.actions.deletefile", () =>
                            {
                                if (GUILayout.Button(UIStyles.Content("Delete Package from File System", "Delete the package only from the cache in the file system and leave index intact.")))
                                {
                                    if (File.Exists(info.GetLocation(true)))
                                    {
                                        File.Delete(info.GetLocation(true));
                                        info.SetLocation(null);
                                        info.PackageSize = 0;
                                        info.CurrentState = Asset.State.New;
                                        info.Refresh();
                                        DBAdapter.DB.Execute("update Asset set Location=null, PackageSize=0, CurrentState=? where Id=?", info.AssetId, Asset.State.New);
                                        _requireAssetTreeRebuild = true;
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        UIBlock("package.actions.delete", () =>
                        {
                            if (GUILayout.Button(UIStyles.Content("Delete Package From Index", "Delete the package from the database.")))
                            {
                                AI.RemovePackage(info, false);
                                _requireLookupUpdate = ChangeImpact.Write;
                                _requireAssetTreeRebuild = true;
                            }
                        });
                    }
                }
                EditorGUI.EndDisabledGroup();

                UIBlock("package.actions.tag", () =>
                {
                    DrawAddPackageTag(new List<AssetInfo> {info});
                    if (info.PackageTags != null && info.PackageTags.Count > 0)
                    {
                        float x = 0f;
                        foreach (TagInfo tagInfo in info.PackageTags)
                        {
                            x = CalcTagSize(x, tagInfo.Name);
                            UIStyles.DrawTag(tagInfo, () =>
                            {
                                Tagging.RemoveTagAssignment(info, tagInfo);
                                _requireAssetTreeRebuild = true;
                            });
                        }
                    }
                    GUILayout.EndHorizontal();
                });
            }
            if (showExpanded)
            {
                List<string> sections = new List<string>();
                List<Dependency> pDeps = info.GetPackageDependencies();
                List<AssetInfo> pInvDeps = info.GetPackageUsageDependencies(_assets);
                if (info.Media != null && info.Media.Count > 0) sections.Add("Images");
                if (!string.IsNullOrWhiteSpace(info.Description)) sections.Add("Description");
                if (!string.IsNullOrWhiteSpace(info.ReleaseNotes)) sections.Add("Release Notes");
                if (info.AssetSource == Asset.Source.RegistryPackage || pDeps != null || pInvDeps != null) sections.Add("Dependencies");

                if (sections.Count > 0)
                {
                    EditorGUILayout.Space(20);
                    if (AI.Config.projectDetailTabsMode == 0)
                    {
                        _packageDetailsTab = GUILayout.Toolbar(_packageDetailsTab, sections.ToArray(), GUILayout.Height(32), GUILayout.MinWidth(500));
                        if (_packageDetailsTab > sections.Count - 1) _packageDetailsTab = sections.Count - 1;
                        switch (sections[_packageDetailsTab])
                        {
                            case "Description":
                                ShowDescriptionDetails(info);
                                break;

                            case "Release Notes":
                                ShowReleaseNotesDetails(info);
                                break;

                            case "Images":
                                ShowMediaDetails(info);
                                break;

                            case "Dependencies":
                                ShowDependencyDetails(info);
                                break;

                        }
                    }
                    else
                    {
                        if (sections.Contains("Images"))
                        {
                            UIBlock("package.media", () =>
                            {
                                EditorGUILayout.LabelField("Media", EditorStyles.boldLabel);
                                ShowMediaDetails(info);
                                EditorGUILayout.Space();
                            });
                        }
                        if (sections.Contains("Release Notes"))
                        {
                            UIBlock("package.releasenotes", () =>
                            {
                                EditorGUILayout.LabelField("Release Notes", EditorStyles.boldLabel);
                                ShowReleaseNotesDetails(info);
                                EditorGUILayout.Space();
                            });
                        }
                        if (sections.Contains("Description"))
                        {
                            UIBlock("package.description", () =>
                            {
                                EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                                ShowDescriptionDetails(info);
                                EditorGUILayout.Space();
                            });
                        }
                        if (sections.Contains("Dependencies"))
                        {
                            UIBlock("package.dependencies", () =>
                            {
                                EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);
                                ShowDependencyDetails(info);
                                EditorGUILayout.Space();
                            });
                        }
                    }
                }
            }
            else
            {
                // highly condensed view
                if (info.PreviewTexture != null)
                {
                    UIBlock("package.icon", () =>
                    {
                        EditorGUILayout.Space();
                        GUILayout.FlexibleSpace();
                        DrawPackagePreview(info);
                        GUILayout.FlexibleSpace();
                    });
                }
                else if (info.AssetSource == Asset.Source.RegistryPackage && !string.IsNullOrWhiteSpace(info.Description))
                {
                    UIBlock("package.description", () =>
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(info.Description, EditorStyles.wordWrappedLabel);
                    });
                }
            }

            if (AI.Config.tab == 1) RenderExpandButton();
            if (startNewSection) GUILayout.EndVertical();
        }

        private static void InstallPackage(AssetInfo info, string version)
        {
            info.ForceTargetVersion(version);

            ImportUI importUI = ImportUI.ShowWindow();
            importUI.Init(new List<AssetInfo> {info}, true);
        }

        private void ShowDependencyDetails(AssetInfo info)
        {
            if (info.AssetSource == Asset.Source.RegistryPackage)
            {
                PackageInfo pInfo = AssetStore.GetPackageInfo(info);
                if (!AssetStore.IsMetadataAvailable())
                {
                    EditorGUILayout.HelpBox("Loading data...", MessageType.Info);
                }
                else if (pInfo == null || pInfo.dependencies == null)
                {
                    EditorGUILayout.HelpBox("Could not find matching package metadata.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical(GUILayout.Width(UIStyles.INSPECTOR_WIDTH - 30));
                    EditorGUILayout.LabelField("Is Using", EditorStyles.boldLabel);
                    if (pInfo.dependencies.Length > 0)
                    {
                        foreach (DependencyInfo dependency in pInfo.dependencies.OrderBy(d => d.name))
                        {
                            AssetInfo package = _assets.FirstOrDefault(a => a.SafeName == dependency.name);
                            if (package != null)
                            {
                                if (GUILayout.Button(package.GetDisplayName() + $" - {dependency.version}", GUILayout.ExpandWidth(false)))
                                {
                                    OpenInPackageView(package);
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"{dependency.name} - {dependency.version}");
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("-none-");
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();
                    List<PackageInfo> usedBy = AssetStore.GetPackages().Values.Where(p => p.dependencies.Select(d => d.name).Contains(info.SafeName)).ToList();
                    EditorGUILayout.LabelField("Used By", EditorStyles.boldLabel);
                    if (usedBy.Any())
                    {
                        foreach (PackageInfo dependency in usedBy.OrderBy(d => d.displayName))
                        {
                            AssetInfo package = _assets.FirstOrDefault(a => a.SafeName == dependency.name);
                            if (package != null)
                            {
                                if (GUILayout.Button(package.GetDisplayName() + (package.IsFeaturePackage() ? " (feature)" : "") + $" - {dependency.version}", GUILayout.ExpandWidth(false)))
                                {
                                    OpenInPackageView(package);
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"{dependency.name} - {dependency.version}");
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("-none-");
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
            else if (info.GetPackageDependencies() != null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The following items might be required in order to use the package, but could also be optional", MessageType.Info);
                EditorGUILayout.Space();
                foreach (Dependency dependency in info.GetPackageDependencies().OrderBy(d => d.name).ThenBy(d => d.location))
                {
                    AssetInfo package = _assets.FirstOrDefault(a => a.ForeignId == dependency.id);
                    if (package != null)
                    {
                        if (GUILayout.Button(dependency.name, GUILayout.ExpandWidth(false)))
                        {
                            OpenInPackageView(package);
                        }
                    }
                    else
                    {
                        package = _assets.FirstOrDefault(a => a.SafeName == dependency.location);
                        if (package != null)
                        {
                            if (GUILayout.Button(package.GetDisplayName() + (package.IsFeaturePackage() ? " (feature)" : ""), GUILayout.ExpandWidth(false)))
                            {
                                OpenInPackageView(package);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button((string.IsNullOrWhiteSpace(dependency.name) ? dependency.location : dependency.name) + "*", GUILayout.ExpandWidth(false)))
                            {
                                Application.OpenURL(dependency.location);
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            if (info.GetPackageUsageDependencies(_assets) != null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The following Asset Store packages list this one as a dependency.", MessageType.Info);
                EditorGUILayout.Space();
                foreach (AssetInfo package in info.GetPackageUsageDependencies(_assets).OrderBy(d => d.GetDisplayName()))
                {
                    if (GUILayout.Button(package.GetDisplayName(), GUILayout.ExpandWidth(false)))
                    {
                        OpenInPackageView(package);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void ShowMediaDetails(AssetInfo info)
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_selectedMedia < 0 || _selectedMedia >= info.Media.Count) _selectedMedia = 0;
            GUILayout.Box(info.Media[_selectedMedia].Texture, UIStyles.centerLabel, GUILayout.MaxWidth(GetInspectorWidth() - 20), GUILayout.Height(AI.Config.mediaHeight));
            if (Event.current.type == EventType.Repaint) _mediaRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (_mediaRect.Contains(Event.current.mousePosition))
                {
                    // start process from thread as otherwise GUI reports layouting errors
                    string path = info.ToAsset().GetMediaFile(info.Media[_selectedMedia], AI.GetPreviewFolder());
                    Task _ = Task.Run(() => Process.Start(path));
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            _imageScrollPos = EditorGUILayout.BeginScrollView(_imageScrollPos, false, false, GUILayout.Height(AI.Config.mediaThumbnailHeight + 20));
            GUILayout.BeginHorizontal();
            for (int i = 0; i < info.Media.Count; i++)
            {
                AssetMedia media = info.Media[i];
                Texture2D texture = media.ThumbnailTexture != null ? media.ThumbnailTexture : media.Texture;
                if (GUILayout.Button(
                    UIStyles.Content(texture == null ? "Loading..." : string.Empty, texture),
                    GUILayout.Width(AI.Config.mediaThumbnailWidth),
                    GUILayout.Height(AI.Config.mediaThumbnailHeight + (i == _selectedMedia ? 10 : 0))))
                {
                    if (media.Type == "youtube")
                    {
                        // open URL in browser
                        Application.OpenURL(media.GetUrl());
                    }
                    else
                    {
                        _selectedMedia = i;
                    }
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        private static void ShowReleaseNotesDetails(AssetInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.ReleaseNotes))
            {
                EditorGUILayout.LabelField(StringUtils.ToLabel(info.ReleaseNotes), EditorStyles.wordWrappedLabel);
            }
        }

        private static void ShowDescriptionDetails(AssetInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.Description))
            {
                EditorGUILayout.LabelField(StringUtils.ToLabel(info.Description), EditorStyles.wordWrappedLabel);
            }
        }

        private static void DrawPackagePreview(AssetInfo info)
        {
            GUILayout.Box(info.PreviewTexture, EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(GetInspectorWidth()), GUILayout.MaxHeight(100));
        }

        private static async void ShowInExplorer(AssetInfo info)
        {
            string location = await info.GetLocation(true, true);
            EditorUtility.RevealInFinder(location);
        }

        private static void RemovePackage(AssetInfo info)
        {
            Client.Remove(info.SafeName);
        }

        private async void ConnectToAssetStore(AssetInfo info, AssetDetails details)
        {
            AI.ConnectToAssetStore(info, details);
            await AI.FetchAssetsDetails();
            _requireLookupUpdate = ChangeImpact.Write;
            _requireAssetTreeRebuild = true;
        }

        private float CalcTagSize(float x, string tagName)
        {
            x += UIStyles.tag.CalcSize(UIStyles.Content(tagName)).x + UIStyles.TAG_SIZE_SPACING + EditorGUIUtility.singleLineHeight + UIStyles.tag.margin.right * 2f;
            if (x > GetInspectorWidth() - UIStyles.TAG_OUTER_MARGIN * 3)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(95 + 3);
                x = UIStyles.tag.CalcSize(UIStyles.Content(tagName)).x + UIStyles.TAG_SIZE_SPACING + EditorGUIUtility.singleLineHeight + UIStyles.tag.margin.right * 2f;
            }
            return x;
        }

        private void DrawAddPackageTag(List<AssetInfo> info)
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(UIStyles.Content("Add Tag..."), GUILayout.Width(80)))
            {
                TagSelectionUI tagUI = new TagSelectionUI();
                tagUI.Init(TagAssignment.Target.Package);
                tagUI.SetAssets(info);
                PopupWindow.Show(_tagButtonRect, tagUI);
            }
            if (Event.current.type == EventType.Repaint) _tagButtonRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(15);
        }

        private void DrawPackagesTab()
        {
            if (_packageCount == 0)
            {
                EditorGUILayout.HelpBox("No packages were indexed yet. Start the indexing process to fill this list.", MessageType.Info);
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditorGUIUtility.IconContent("Preset.Context", "|Search Filters")))
            {
                AI.Config.showPackageFilterBar = !AI.Config.showPackageFilterBar;
                AI.SaveConfig();
            }
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            EditorGUI.BeginChangeCheck();
            UIBlock2("package.actions.nameonly", () =>
            {
                if (GUILayout.Button(AI.Config.packageSearchMode == 0 ? UIStyles.Content("=", "Name Only") : UIStyles.Content("+", "Name & Description"), GUILayout.Width(17)))
                {
                    AI.Config.packageSearchMode = AI.Config.packageSearchMode == 0 ? 1 : 0;
                    AI.SaveConfig();
                }
            });
            _assetSearchPhrase = AssetSearchField.OnGUI(_assetSearchPhrase, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
            {
                // delay search to allow fast typing
                _nextAssetSearchTime = Time.realtimeSinceStartup + AI.Config.searchDelay;
            }
            else if (!_allowLogic && _nextAssetSearchTime > 0 && Time.realtimeSinceStartup > _nextAssetSearchTime) // don't do when logic allowed as otherwise there will be GUI errors
            {
                _nextAssetSearchTime = 0;
                _requireAssetTreeRebuild = true;
            }

            EditorGUI.BeginChangeCheck();
            if (AI.Config.assetGrouping == 0 || AI.Config.packageViewMode == 1)
            {
                UIBlock2("package.actions.sort", () =>
                {
                    EditorGUILayout.Space();
                    GUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 50;
                    AI.Config.assetSorting = EditorGUILayout.Popup(UIStyles.Content("Sort by:", "Specify how packages should be sorted"), AI.Config.assetSorting, _packageSortOptions, GUILayout.Width(160));
                    if (GUILayout.Button(AI.Config.sortAssetsDescending ? UIStyles.Content("˅", "Descending") : UIStyles.Content("˄", "Ascending"), GUILayout.Width(17)))
                    {
                        AI.Config.sortAssetsDescending = !AI.Config.sortAssetsDescending;
                    }
                    GUILayout.EndHorizontal();
                });
            }

            if (AI.Config.packageViewMode == 0)
            {
                UIBlock2("package.actions.group", () =>
                {
                    EditorGUILayout.Space();
                    EditorGUIUtility.labelWidth = 60;
                    AI.Config.assetGrouping = EditorGUILayout.Popup(UIStyles.Content("Group by:", "Select if packages should be grouped or not"), AI.Config.assetGrouping, _groupByOptions, GUILayout.Width(140));

                    EditorGUIUtility.labelWidth = 0;

                    if (AI.Config.assetGrouping > 0)
                    {
                        if (GUILayout.Button("Expand All", GUILayout.ExpandWidth(false)))
                        {
                            AssetTreeView.ExpandAll();
                        }
                        if (GUILayout.Button("Collapse All", GUILayout.ExpandWidth(false)))
                        {
                            AssetTreeView.CollapseAll();
                        }
                    }
                });
            }

            UIBlock2("package.actions.typeselector", () =>
            {
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Types:", GUILayout.Width(41));
                AI.Config.packagesListing = GUILayout.Toolbar(AI.Config.packagesListing, _packageListingOptionsShort, GUILayout.Width(400));
                GUILayout.EndHorizontal();
            });

            if (EditorGUI.EndChangeCheck())
            {
                CreateAssetTree();
                AI.SaveConfig();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (AI.Config.showPackageFilterBar)
            {
                GUILayout.BeginVertical("Filter Bar", "window", GUILayout.Width(UIStyles.INSPECTOR_WIDTH));
                EditorGUILayout.Space();

                int labelWidth = 85;

                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Packages", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                AI.Config.packagesListing = EditorGUILayout.Popup(AI.Config.packagesListing, _packageListingOptions, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SRPs", EditorStyles.boldLabel, GUILayout.MaxWidth(labelWidth));
                AI.Config.assetSRPs = EditorGUILayout.Popup(AI.Config.assetSRPs, _srpOptions, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Deprecation", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                AI.Config.assetDeprecation = EditorGUILayout.Popup(AI.Config.assetDeprecation, _deprecationOptions, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(UIStyles.Content("Maintenance", "A collection of various special-purpose filters"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                _selectedMaintenance = EditorGUILayout.Popup(_selectedMaintenance, _maintenanceOptions, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    AI.SaveConfig();
                    _requireAssetTreeRebuild = true;
                }

                EditorGUILayout.Space();
                if ((AI.Config.packagesListing > 0 || AI.Config.assetDeprecation > 0 || _selectedMaintenance > 0 || AI.Config.assetSRPs > 0) && GUILayout.Button("Reset Filters"))
                {
                    ResetPackageFilters();
                }

                GUILayout.EndVertical();
            }

            // packages
            GUILayout.BeginVertical();
            if (AI.Config.packageViewMode == 0)
            {
                int left = AI.Config.showPackageFilterBar ? UIStyles.INSPECTOR_WIDTH + 5 : 0;
                int yStart = (string.IsNullOrEmpty(CloudProjectSettings.accessToken) ? 128 : 80) + (AI.UICustomizationMode ? 30 : 0);
                float width = position.width - GetInspectorWidth() - left - 5;
                if (width < 300) width = 300;
                AssetTreeView.OnGUI(new Rect(left, yStart, width, position.height - yStart - 22));
                GUILayout.FlexibleSpace();
            }
            else
            {
                _packageScrollPos = GUILayout.BeginScrollView(_packageScrollPos, false, false);
                EditorGUI.BeginChangeCheck();
                int inspectorCount = (AI.Config.showPackageFilterBar ? 2 : 1) + (AI.Config.expandPackageDetails ? 1 : 0);
                PGrid.Draw(position.width, inspectorCount, AI.Config.packageTileSize, UIStyles.packageTile, UIStyles.selectedPackageTile);
                if (EditorGUI.EndChangeCheck() || (_allowLogic && _searchDone))
                {
                    // interactions
                    PGrid.HandleMouseClicks();
                    HandleAssetGridSelectionChanged();
                }
                GUILayout.EndScrollView();
            }

            // view settings
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            AI.Config.packageViewMode = GUILayout.Toolbar(AI.Config.packageViewMode, _packageViewOptions, GUILayout.Width(50), GUILayout.Height(20));
            if (EditorGUI.EndChangeCheck())
            {
                CreateAssetTree();
                AI.SaveConfig();
            }

            if (AI.Config.packageViewMode == 1)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                AI.Config.packageTileSize = EditorGUILayout.IntSlider(AI.Config.packageTileSize, 50, 200, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck()) AI.SaveConfig();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{_visiblePackageCount:N0} packages", UIStyles.centerLabel, GUILayout.Width(120));

            EditorGUI.BeginChangeCheck();
            string caption = "In Current Project";
            if (_usageCalculationInProgress) caption += $" ({AssetProgress.MainProgress / (float)AssetProgress.MainCount:P0})";
            AI.Config.onlyInProject = EditorGUILayout.ToggleLeft(UIStyles.Content(caption, "Show only packages that are used inside the current project. Will require a full project scan via the reporting tab, done automatically in the background."), AI.Config.onlyInProject, GUILayout.MinWidth(130));
            if (EditorGUI.EndChangeCheck())
            {
                CreateAssetTree();
                AI.SaveConfig();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.Space();

            // inspector
            GUILayout.BeginVertical(GUILayout.Width(GetInspectorWidth()));
            // FIXME: scrolling is broken for some reason, bar will often overlap
            _assetsScrollPos = GUILayout.BeginScrollView(_assetsScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(false));
            if (!AI.Config.expandPackageDetails || _selectedTreeAsset == null)
            {
                UIBlock("package.overview", () =>
                {
                    GUILayout.BeginVertical("Overview", "window", GUILayout.Width(GetInspectorWidth()), GUILayout.ExpandHeight(false));
                    EditorGUILayout.Space();
                    DrawPackageStats(true);

                    GUILayout.EndVertical();
                    EditorGUILayout.Space();
                });
            }

            if (_selectedTreeAsset != null)
            {
                DrawPackageDetails(_selectedTreeAsset, true);
            }
            else if (_selectedTreeAsset == null && _selectedTreeAssets != null && _selectedTreeAssets.Count > 0)
            {
                DrawBulkPackageActions(_selectedTreeAssets, _assetTreeSubPackageCount, _assetBulkTags, _assetTreeSelectionSize, _assetTreeSelectionTotalCosts, _assetTreeSelectionStoreCosts, true);
            }
            else
            {
                GUILayout.BeginVertical("Package Details", "window", GUILayout.Width(GetInspectorWidth()), GUILayout.ExpandHeight(false));
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Select one or more packages to see details.", MessageType.Info);
                RenderExpandButton();
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            if (!ShowAdvanced() && AI.Config.showHints) EditorGUILayout.LabelField("Hold down CTRL for additional options.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (AI.Config.packageViewMode == 1) PGrid.HandleKeyboardCommands();
        }

        private void ResetPackageFilters()
        {
            AI.Config.packagesListing = 0;
            AI.Config.assetDeprecation = 0;
            AI.Config.assetSRPs = 0;
            _selectedMaintenance = 0;
            _requireAssetTreeRebuild = true;

            AI.SaveConfig();
        }


        private void DrawPackageStats(bool allowCollapse)
        {
            int labelWidth = 130;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Packages", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            EditorGUILayout.LabelField($"{_assets.Count:N0}", EditorStyles.label, GUILayout.Width(50));
            if (allowCollapse)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(AI.Config.showPackageStatsDetails ? "Hide Details" : "Show Details", GUILayout.ExpandWidth(false)))
                {
                    AI.Config.showPackageStatsDetails = !AI.Config.showPackageStatsDetails;
                    AI.SaveConfig();
                }
            }
            GUILayout.EndHorizontal();
            if (!allowCollapse || AI.Config.showPackageStatsDetails)
            {
                GUILabelWithText($"{UIStyles.INDENT}Indexed", $"{_indexedPackageCount:N0}/{_indexablePackageCount:N0}", labelWidth, "Indexable packages depend on configuration settings and package availability. Abandoned packages cannot be downloaded and indexed anymore if they are not already in the cache. If registry packages are not activated for indexing, they will also be left unindexed. If active, they can only be indexed if in the cache, which means they must have been installed at least once sometime in a project on this machine. Switch to the Not Indexed maintenance view to see all non-indexed packages and discover the reason for each.");
                if (_purchasedAssetsCount > 0) GUILabelWithText($"{UIStyles.INDENT}Asset Store", $"{_purchasedAssetsCount:N0}", labelWidth);
                if (_registryPackageCount > 0) GUILabelWithText($"{UIStyles.INDENT}Registries", $"{_registryPackageCount:N0}", labelWidth);
                if (_customPackageCount > 0) GUILabelWithText($"{UIStyles.INDENT}Other Sources", $"{_customPackageCount:N0}", labelWidth);
                if (_deprecatedAssetsCount > 0) GUILabelWithText($"{UIStyles.INDENT}Deprecated", $"{_deprecatedAssetsCount:N0}", labelWidth);
                if (_abandonedAssetsCount > 0) GUILabelWithText($"{UIStyles.INDENT}Abandoned", $"{_abandonedAssetsCount:N0}", labelWidth);
                if (_excludedAssetsCount > 0) GUILabelWithText($"{UIStyles.INDENT}Excluded", $"{_excludedAssetsCount:N0}", labelWidth);
                if (_subPackageCount > 0) GUILabelWithText($"{UIStyles.INDENT}Sub-Packages", $"{_subPackageCount:N0}", labelWidth);
            }
            if (_packageFileCount > 0) GUILabelWithText("Indexed Files", $"{_packageFileCount:N0}", labelWidth);
        }

        private void RenderExpandButton()
        {
            UIBlock("package.actions.expand", () =>
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_ScaleTool", "|Expand/Collapse Details Section"), GUILayout.Width(28), GUILayout.Height(28)))
                {
                    AI.Config.expandPackageDetails = !AI.Config.expandPackageDetails;
                    AI.SaveConfig();
                    LoadMediaOnDemand(_selectedTreeAsset);
                }
                GUILayout.FlexibleSpace();
                if (AI.Config.expandPackageDetails)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Space(10);
                    EditorGUI.BeginChangeCheck();
                    AI.Config.projectDetailTabsMode = GUILayout.Toolbar(AI.Config.projectDetailTabsMode, _packageDetailsViewOptions, GUILayout.Width(50), GUILayout.Height(20));
                    if (EditorGUI.EndChangeCheck()) AI.SaveConfig();
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            });
        }

        private void DrawBulkPackageActions(List<AssetInfo> bulkAssets, long bulkSubAssetCount, Dictionary<string, Tuple<int, Color>> bulkTags, long size, float totalCosts, float storeCosts, bool useScroll)
        {
            int labelWidth = 130;

            GUILayout.BeginVertical("Bulk Info", "window", GUILayout.Width(GetInspectorWidth()), GUILayout.ExpandHeight(false));
            EditorGUILayout.Space();
            UIBlock("package.bulk.count", () => GUILabelWithText("Selected Items", $"{bulkAssets.Count - bulkSubAssetCount:N0}", labelWidth));
            if (bulkSubAssetCount > 0) UIBlock("package.bulk.childcount", () => GUILabelWithText($"{UIStyles.INDENT}Sub-Packages", $"{bulkSubAssetCount:N0}", labelWidth));
            UIBlock("package.bulk.size", () => GUILabelWithText("Size on Disk", EditorUtility.FormatBytes(size), labelWidth));
            if (totalCosts > 0)
            {
                UIBlock("package.bulk.price", () =>
                {
                    EditorGUILayout.Space();
                    GUILabelWithText("Total Price", bulkAssets[0].GetPriceText(totalCosts), labelWidth);
                });
            }
            if (storeCosts > 0 && totalCosts > storeCosts)
            {
                UIBlock("package.bulk.storeprice", () => GUILabelWithText($"{UIStyles.INDENT}Asset Store", bulkAssets[0].GetPriceText(storeCosts), labelWidth));
                UIBlock("package.bulk.otherprice", () => GUILabelWithText($"{UIStyles.INDENT}Other Sources", bulkAssets[0].GetPriceText(totalCosts - storeCosts), labelWidth));
                EditorGUILayout.Space();
            }
            GUILayout.EndVertical();

            labelWidth = 100;
            EditorGUILayout.Space();
            GUILayout.BeginVertical("Bulk Actions", "window", GUILayout.Width(GetInspectorWidth()));
            EditorGUILayout.Space();
            if (useScroll) _bulkScrollPos = GUILayout.BeginScrollView(_bulkScrollPos, false, false);
            UpdateObserver updateObserver = AI.GetObserver();
            if (!updateObserver.PrioInitializationDone)
            {
                int progress = Mathf.RoundToInt(updateObserver.PrioInitializationProgress * 100f);
                EditorGUILayout.HelpBox($"Gathering data (*): {progress}%", MessageType.Info);
                EditorGUILayout.Space();
            }
            if (AI.Config.createBackups)
            {
                UIBlock("package.bulk.actions.backup", () =>
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Backup", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                    if (GUILayout.Button("All", GUILayout.ExpandWidth(false)))
                    {
                        bulkAssets.ForEach(info => AI.SetAssetBackup(info, true));
                    }
                    if (GUILayout.Button("None", GUILayout.ExpandWidth(false)))
                    {
                        bulkAssets.ForEach(info => AI.SetAssetBackup(info, false));
                    }
                    GUILayout.EndHorizontal();
                });
            }

            UIBlock("package.bulk.actions.extract", () =>
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(UIStyles.Content("Extract", "Will keep the package extracted in the cache to minimize access delays at the cost of more hard disk space."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                if (GUILayout.Button("All", GUILayout.ExpandWidth(false)))
                {
                    bulkAssets.ForEach(info => AI.SetAssetExtraction(info, true));
                }
                if (GUILayout.Button("None", GUILayout.ExpandWidth(false)))
                {
                    bulkAssets.ForEach(info => AI.SetAssetExtraction(info, false));
                }
                GUILayout.EndHorizontal();
            });

            UIBlock("package.bulk.actions.exclude", () =>
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Exclude", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                if (GUILayout.Button("All", GUILayout.ExpandWidth(false)))
                {
                    bulkAssets.ForEach(info => AI.SetAssetExclusion(info, true));
                    _requireLookupUpdate = ChangeImpact.Write;
                    _requireSearchUpdate = true;
                    _requireAssetTreeRebuild = true;
                }
                if (GUILayout.Button("None", GUILayout.ExpandWidth(false)))
                {
                    bulkAssets.ForEach(info => AI.SetAssetExclusion(info, false));
                    _requireLookupUpdate = ChangeImpact.Write;
                    _requireSearchUpdate = true;
                    _requireAssetTreeRebuild = true;
                }
                GUILayout.EndHorizontal();
            });

            UIBlock("package.bulk.actions.aiusage", () =>
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(UIStyles.Content("AI Captions", "Activate to create backups for this asset (done after every update cycle)."), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                if (GUILayout.Button("All", GUILayout.ExpandWidth(false)))
                {
                    bulkAssets.ForEach(info => AI.SetAssetAIUse(info, true, false));
                    AI.TriggerPackageRefresh();
                }
                if (GUILayout.Button("None", GUILayout.ExpandWidth(false)))
                {
                    bulkAssets.ForEach(info => AI.SetAssetAIUse(info, false, false));
                    AI.TriggerPackageRefresh();
                }
                GUILayout.EndHorizontal();
            });

            // determine download status, a bit expensive but happens only in bulk selections
            ProfileMarkerBulk.Begin();
            int notDownloaded = 0;
            int updateAvailable = 0;
            int packageUpdateAvailable = 0;
            int updateAvailableButCustom = 0;
            int downloading = 0;
            int paused = 0;
            long remainingBytes = 0;
            foreach (AssetInfo info in bulkAssets.Where(a => a.WasOutdated || !a.IsDownloaded || a.IsUpdateAvailable(_assets, false)))
            {
                if (info.AssetSource == Asset.Source.RegistryPackage)
                {
                    if (info.IsUpdateAvailable()) packageUpdateAvailable++;
                }
                else
                {
                    AssetDownloadState state = info.PackageDownloader.GetState();
                    switch (state.state)
                    {
                        case AssetDownloader.State.Unavailable:
                            notDownloaded++;
                            break;

                        case AssetDownloader.State.Downloading:
                            downloading++;
                            remainingBytes += state.bytesTotal - state.bytesDownloaded;
                            break;

                        case AssetDownloader.State.Paused:
                            paused++;
                            break;

                        case AssetDownloader.State.UpdateAvailable:
                            updateAvailable++;
                            break;

                        case AssetDownloader.State.Unknown:
                            if (info.AssetSource == Asset.Source.CustomPackage && info.IsUpdateAvailable(_assets))
                            {
                                updateAvailableButCustom++;
                            }
                            break;
                    }
                }
            }
            ProfileMarkerBulk.End();

            string initializing = updateObserver.PrioInitializationDone ? "" : "*";
            if (notDownloaded > 0)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Not Cached" + initializing, EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                if (GUILayout.Button($"Download remaining {notDownloaded}", GUILayout.ExpandWidth(false)))
                {
                    foreach (AssetInfo info in bulkAssets.Where(a => !a.IsDownloaded))
                    {
                        info.PackageDownloader.Download();
                    }
                }
                GUILayout.EndHorizontal();
            }
            if (updateAvailable > 0)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Cache Updates" + initializing, EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                if (GUILayout.Button("Download " + (downloading > 0 ? "remaining " : "") + updateAvailable, GUILayout.ExpandWidth(false)))
                {
                    foreach (AssetInfo info in bulkAssets.Where(a => a.IsUpdateAvailable(_assets) && a.PackageDownloader != null))
                    {
                        if (info.PackageDownloader.GetState().state == AssetDownloader.State.UpdateAvailable)
                        {
                            info.WasOutdated = true;
                            info.PackageDownloader.Download();
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            if (packageUpdateAvailable > 0)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Packages" + initializing, EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                if (GUILayout.Button($"Update {packageUpdateAvailable} registry packages", GUILayout.ExpandWidth(false)))
                {
                    List<AssetInfo> bulkList = bulkAssets
                        .Where(a => a.AssetSource == Asset.Source.RegistryPackage && a.IsUpdateAvailable())
                        .ToList();
                    ImportUI importUI = ImportUI.ShowWindow();
                    importUI.Init(bulkList, true);
                }
                GUILayout.EndHorizontal();
            }
            if (updateAvailableButCustom > 0)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox($"{updateAvailableButCustom}{initializing} updates cannot be performed since the assets are local custom packages and not from the Asset Store.", MessageType.Info);
                GUILayout.EndHorizontal();
            }

            if (downloading > 0)
            {
                GUILabelWithText("Downloading" + initializing, $"{downloading}", labelWidth);
                GUILabelWithText("Remaining" + initializing, $"{EditorUtility.FormatBytes(remainingBytes)}", labelWidth);
            }
            if (paused > 0)
            {
                GUILabelWithText("Paused", $"{paused}", labelWidth);
            }
            EditorGUILayout.Space();

            UIBlock("package.bulk.actions.import", () =>
            {
                if (GUILayout.Button("Import..."))
                {
                    ImportUI importUI = ImportUI.ShowWindow();
                    importUI.Init(bulkAssets);
                }
            });
            UIBlock("package.bulk.actions.openlocation", () =>
            {
                if (GUILayout.Button(UIStyles.Content("Open Package Locations...")))
                {
                    bulkAssets.ForEach(info => { EditorUtility.RevealInFinder(info.GetLocation(true)); });
                }
            });

            UIBlock("package.bulk.actions.reindexnextrun", () =>
            {
                if (GUILayout.Button(UIStyles.Content("Reindex Packages on Next Run", "Will mark packages as outdated and force a reindex the next time Update Index is called on the Settings tab.")))
                {
                    bulkAssets.ForEach(info => AI.ForgetPackage(info, true));
                    _requireLookupUpdate = ChangeImpact.Write;
                    _requireSearchUpdate = true;
                    _requireAssetTreeRebuild = true;
                }
            });
            UIBlock("package.bulk.actions.refreshmetadata", () =>
            {
                if (GUILayout.Button(UIStyles.Content("Refresh Metadata", "Will fetch most up-to-date metadata from the Asset Store.")))
                {
                    bulkAssets.ForEach(info => FetchAssetDetails(true, info.AssetId, true));
                }
            });
            UIBlock("package.bulk.actions.recreatepreviews", () =>
            {
                if (GUILayout.Button("Previews Wizard..."))
                {
                    PreviewWizardUI previewsUI = PreviewWizardUI.ShowWindow();
                    previewsUI.Init(bulkAssets, _assets);
                }
            });
            UIBlock("package.bulk.actions.export", () =>
            {
                if (GUILayout.Button("Export packages..."))
                {
                    ExportUI exportUI = ExportUI.ShowWindow();
                    exportUI.Init(bulkAssets, 1);
                }
            });

            UIBlock("package.bulk.actions.delete", () =>
            {
                EditorGUILayout.Space();
                if (GUILayout.Button(UIStyles.Content("Delete Packages...", "Delete the packages from the database and optionally the file system.")))
                {
                    bool removeFiles = bulkAssets.Any(a => a.IsDownloaded);
                    int removeType = EditorUtility.DisplayDialogComplex("Delete Packages", "Do you also want to remove the files from the Unity cache? If not the packages will reappear after the next index update.", "Remove only from Database", "Cancel", "Remove also from File System");
                    if (removeType != 1)
                    {
                        bulkAssets.ForEach(info => AI.RemovePackage(info, removeFiles && removeType == 2));
                        _requireLookupUpdate = ChangeImpact.Write;
                        _requireAssetTreeRebuild = true;
                        _requireSearchUpdate = true;
                    }
                }
            });

            UIBlock("package.bulk.actions.deletefile", () =>
            {
                if (GUILayout.Button(UIStyles.Content("Delete Packages from File System", "Delete the packages directly from the cache in the file system.")))
                {
                    bulkAssets.ForEach(info =>
                    {
                        if (File.Exists(info.GetLocation(true)))
                        {
                            File.Delete(info.GetLocation(true));
                            info.Refresh();
                        }
                    });
                    _requireSearchUpdate = true;
                }
            });

            UIBlock("package.bulk.actions.tag", () =>
            {
                DrawAddPackageTag(bulkAssets);

                float x = 0f;
                foreach (KeyValuePair<string, Tuple<int, Color>> bulkTag in bulkTags)
                {
                    string tagName = $"{bulkTag.Key} ({bulkTag.Value.Item1})";
                    x = CalcTagSize(x, tagName);
                    UIStyles.DrawTag(tagName, bulkTag.Value.Item2, () =>
                    {
                        Tagging.RemovePackageTagAssignment(bulkAssets, bulkTag.Key, true);
                        _requireAssetTreeRebuild = true;
                    }, UIStyles.TagStyle.Remove);
                }
                GUILayout.EndHorizontal();
            });

            RenderExpandButton();

            if (useScroll) GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void CreateAssetTree()
        {
            _requireAssetTreeRebuild = false;
            _visiblePackageCount = 0;
            List<AssetInfo> data = new List<AssetInfo>();
            AssetInfo root = new AssetInfo().WithTreeData("Root", depth: -1);
            data.Add(root);

            // apply filters
            IEnumerable<AssetInfo> filteredAssets = _assets;
            if (_selectedMaintenance != 15) filteredAssets = filteredAssets.Where(a => a.ParentId == 0);

            switch (AI.Config.assetSRPs)
            {
                case 1:
                    filteredAssets = filteredAssets.Where(a => a.BIRPCompatible);
                    break;

                case 2:
                    filteredAssets = filteredAssets.Where(a => a.URPCompatible);
                    break;

                case 3:
                    filteredAssets = filteredAssets.Where(a => a.HDRPCompatible);
                    break;

            }
            switch (AI.Config.assetDeprecation)
            {
                case 1:
                    filteredAssets = filteredAssets.Where(a => !a.IsDeprecated && !a.IsAbandoned);
                    break;

                case 2:
                    filteredAssets = filteredAssets.Where(a => a.IsDeprecated || a.IsAbandoned);
                    break;
            }
            switch (_selectedMaintenance)
            {
                case 1:
                    filteredAssets = filteredAssets.Where(a => a.IsUpdateAvailable(_assets, false) || a.WasOutdated);
                    break;

                case 2:
                    filteredAssets = filteredAssets.Where(a => a.CurrentSubState == Asset.SubState.Outdated);
                    break;

                case 3:
                    filteredAssets = filteredAssets.Where(a => (a.AssetSource == Asset.Source.AssetStorePackage || (a.AssetSource == Asset.Source.RegistryPackage && a.ForeignId > 0)) && a.OfficialState == "disabled");
                    break;

                case 4:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.CustomPackage && a.ForeignId > 0);
                    break;

                case 5:
                    filteredAssets = filteredAssets.Where(a => a.FileCount > 0);
                    break;

                case 6:
                    filteredAssets = filteredAssets.Where(a => a.FileCount == 0);
                    break;

                case 7:
                    filteredAssets = filteredAssets.Where(a => !string.IsNullOrEmpty(a.Registry) && a.Registry != "Unity");
                    break;

                case 8:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.AssetStorePackage && a.IsDownloaded);
                    break;

                case 9:
                    filteredAssets = filteredAssets.Where(a => a.IsDownloading());
                    break;

                case 10:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.AssetStorePackage && !a.IsDownloaded);
                    break;

                case 11:
                    List<int> duplicates = filteredAssets.Where(a => a.ForeignId > 0).GroupBy(a => a.ForeignId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                    filteredAssets = filteredAssets.Where(a => duplicates.Contains(a.ForeignId));
                    break;

                case 12:
                    filteredAssets = filteredAssets.Where(a => a.Backup);
                    break;

                case 13:
                    filteredAssets = filteredAssets.Where(a => !a.Backup);
                    break;

                case 14:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource != Asset.Source.AssetStorePackage && a.AssetSource != Asset.Source.RegistryPackage && !a.IsDownloaded);
                    break;

                case 15:
                    filteredAssets = filteredAssets.Where(a => a.Exclude);
                    break;

                case 16:
                    filteredAssets = _assets.Where(a => a.ParentId > 0);
                    break;

                case 17:
                    filteredAssets = filteredAssets.Where(a => (a.AssetSource == Asset.Source.AssetStorePackage || a.AssetSource == Asset.Source.CustomPackage) && !a.IsDownloadedCompatible);
                    break;

                case 18:
                    filteredAssets = filteredAssets.Where(a => (a.AssetSource == Asset.Source.AssetStorePackage || a.AssetSource == Asset.Source.CustomPackage) && !a.IsDownloadedCompatible && a.IsCurrentUnitySupported());
                    break;

                case 19:
                    filteredAssets = filteredAssets.Where(a => (a.AssetSource == Asset.Source.AssetStorePackage || a.AssetSource == Asset.Source.CustomPackage) && !a.IsDownloadedCompatible && !a.IsCurrentUnitySupported());
                    break;

            }
            if (_selectedMaintenance != 15) filteredAssets = filteredAssets.Where(a => !a.Exclude);

            // filter after maintenance selection to enable queries like "duplicate but only custom packages shown"
            switch (AI.Config.packagesListing)
            {
                case 1:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource != Asset.Source.RegistryPackage);
                    break;

                case 2:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.AssetStorePackage || (a.AssetSource == Asset.Source.RegistryPackage && a.ForeignId > 0));
                    break;

                case 3:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.RegistryPackage);
                    break;

                case 4:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.CustomPackage);
                    break;

                case 5:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.Directory);
                    break;

                case 6:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.Archive);
                    break;

                case 7:
                    filteredAssets = filteredAssets.Where(a => a.AssetSource == Asset.Source.AssetManager);
                    break;

            }

            if (!string.IsNullOrWhiteSpace(_assetSearchPhrase))
            {
                bool searchDescription = AI.Config.packageSearchMode > 0;

                if (_assetSearchPhrase.StartsWith("~")) // exact mode
                {
                    string term = _assetSearchPhrase.Substring(1);
                    filteredAssets = filteredAssets.Where(a =>
                    {
                        string phrase = term.ToLowerInvariant();
                        return a.GetDisplayName().ToLowerInvariant().Contains(phrase)
                            || (searchDescription && a.Description != null && a.Description.ToLowerInvariant().Contains(phrase));
                    });
                }
                else
                {
                    string[] fuzzyWords = _assetSearchPhrase.Split(' ');
                    foreach (string fuzzyWord in fuzzyWords.Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        if (fuzzyWord.StartsWith("+"))
                        {
                            filteredAssets = filteredAssets.Where(a =>
                            {
                                string phrase = fuzzyWord.Substring(1).ToLowerInvariant();
                                return a.GetDisplayName().ToLowerInvariant().Contains(phrase)
                                    || (searchDescription && a.Description != null && a.Description.ToLowerInvariant().Contains(phrase));
                            });
                        }
                        else if (fuzzyWord.StartsWith("-"))
                        {
                            filteredAssets = filteredAssets.Where(a =>
                            {
                                string phrase = fuzzyWord.Substring(1).ToLowerInvariant();
                                return !a.GetDisplayName().ToLowerInvariant().Contains(phrase)
                                    && (!searchDescription || a.Description == null || !a.Description.ToLowerInvariant().Contains(phrase));
                            });
                        }
                        else
                        {
                            filteredAssets = filteredAssets.Where(a =>
                            {
                                string phrase = fuzzyWord.ToLowerInvariant();
                                return a.GetDisplayName().ToLowerInvariant().Contains(phrase)
                                    || (searchDescription && a.Description != null && a.Description.ToLowerInvariant().Contains(phrase));
                            });
                        }
                    }
                }
            }

            if (AI.Config.onlyInProject)
            {
                if (!_usageCalculationDone || _usedPackages == null)
                {
                    CalculateAssetUsage();
                }
                else
                {
                    filteredAssets = filteredAssets.Where(a => _usedPackages.ContainsKey(a.AssetId));
                }
            }

            string[] lastGroups = Array.Empty<string>();
            int catIdx = 0;
            IOrderedEnumerable<AssetInfo> orderedAssets;

            // grouping not supported for grid view
            int usedGrouping = AI.Config.packageViewMode == 0 ? AI.Config.assetGrouping : 0;
            switch (usedGrouping)
            {
                case 0: // none
                    orderedAssets = AddPackageOrdering(filteredAssets);
                    orderedAssets.ToList().ForEach(a => data.Add(a.WithTreeData(a.GetDisplayName(), a.AssetId)));
                    break;

                case 2: // category
                    orderedAssets = filteredAssets.OrderBy(a => a.GetDisplayCategory(), StringComparer.OrdinalIgnoreCase)
                        .ThenBy(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase);

                    string[] noCat = {"-no category-"};
                    foreach (AssetInfo info in orderedAssets)
                    {
                        // create hierarchy
                        string[] cats = string.IsNullOrEmpty(info.GetDisplayCategory()) ? noCat : info.GetDisplayCategory().Split('/');

                        lastGroups = AddCategorizedItem(cats, lastGroups, data, info, ref catIdx);
                    }
                    break;

                case 3: // publisher
                    IOrderedEnumerable<AssetInfo> orderedAssetsPub = filteredAssets.OrderBy(a => a.GetDisplayPublisher(), StringComparer.OrdinalIgnoreCase)
                        .ThenBy(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase);

                    string[] noPub = {"-no publisher-"};
                    foreach (AssetInfo info in orderedAssetsPub)
                    {
                        // create hierarchy
                        string[] pubs = string.IsNullOrEmpty(info.GetDisplayPublisher()) ? noPub : new[] {info.GetDisplayPublisher()};

                        lastGroups = AddCategorizedItem(pubs, lastGroups, data, info, ref catIdx);
                    }
                    break;

                case 4: // tags
                    List<Tag> tags = Tagging.LoadTags();
                    foreach (Tag tag in tags)
                    {
                        IOrderedEnumerable<AssetInfo> taggedAssets = filteredAssets
                            .Where(a => a.PackageTags != null && a.PackageTags.Any(t => t.Name == tag.Name))
                            .OrderBy(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase);

                        string[] cats = {tag.Name};
                        foreach (AssetInfo info in taggedAssets)
                        {
                            // create hierarchy
                            lastGroups = AddCategorizedItem(cats, lastGroups, data, info, ref catIdx);
                        }
                    }

                    IOrderedEnumerable<AssetInfo> remainingAssets = filteredAssets
                        .Where(a => a.PackageTags == null || a.PackageTags.Count == 0)
                        .OrderBy(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase);
                    string[] untaggedCat = {"-untagged-"};
                    foreach (AssetInfo info in remainingAssets)
                    {
                        lastGroups = AddCategorizedItem(untaggedCat, lastGroups, data, info, ref catIdx);
                    }
                    break;

                case 5: // state
                    IOrderedEnumerable<AssetInfo> orderedAssetsState = filteredAssets.OrderBy(a => a.OfficialState, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase);

                    string[] noState = {"-no state-"};
                    foreach (AssetInfo info in orderedAssetsState)
                    {
                        // create hierarchy
                        string[] pubs = string.IsNullOrEmpty(info.OfficialState) ? noState : new[] {info.OfficialState};

                        lastGroups = AddCategorizedItem(pubs, lastGroups, data, info, ref catIdx);
                    }
                    break;

                case 6: // location
                    IOrderedEnumerable<AssetInfo> orderedAssetsLocation = filteredAssets.OrderBy(a => GetLocationDirectory(a.Location), StringComparer.OrdinalIgnoreCase)
                        .ThenBy(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase);

                    string[] noLocation = {"-no location-"};
                    foreach (AssetInfo info in orderedAssetsLocation)
                    {
                        // create hierarchy
                        string[] pubs = string.IsNullOrEmpty(GetLocationDirectory(info.Location)) ? noLocation : new[] {GetLocationDirectory(info.Location)};

                        lastGroups = AddCategorizedItem(pubs, lastGroups, data, info, ref catIdx);
                    }
                    break;
            }

            _textureLoading2?.Cancel();
            _textureLoading2 = new CancellationTokenSource();

            if (AI.Config.packageViewMode == 0)
            {
                // re-add parents to sub-packages if they were filtered out
                ReAddMissingParents(filteredAssets, data);

                // reorder sub-packages
                ReorderSubPackages(data);

                if (_selectedMaintenance != 15)
                {
                    // add sub-packages from _assets where missing in data since we filtered them out initially
                    AddSubPackagesToTree(data);
                    AddSubPackagesToFeatures(data);
                }

                AssetTreeModel.SetData(data, AI.Config.assetGrouping > 0);
                AssetTreeView.Reload();
                HandleAssetTreeSelectionChanged(AssetTreeView.GetSelection());

                AssetUtils.LoadTextures(data, _textureLoading2.Token);
                _visiblePackageCount = data.Count(a => a.AssetId > 0 && a.ParentId == 0);
            }
            else
            {
                // grid does not support grouping or sub-packages
                List<AssetInfo> visiblePackages = data.Where(a => a.AssetId > 0 && a.ParentId == 0).ToList();
                PGrid.contents = visiblePackages.Select(a => new GUIContent(a.GetDisplayName())).ToArray();
                PGrid.noTextBelow = AI.Config.noPackageTileTextBelow;
                PGrid.enlargeTiles = AI.Config.enlargeTiles;
                PGrid.centerTiles = AI.Config.centerTiles;
                PGrid.Init(_assets, visiblePackages, HandleAssetGridSelectionChanged, info => info.GetDisplayName());

                AssetUtils.LoadTextures(visiblePackages, _textureLoading2.Token, (idx, texture) =>
                {
                    // validate in case dataset changed in the meantime
                    if (PGrid.contents.Length > idx) PGrid.contents[idx].image = texture != null ? texture : PGrid.packages[idx].GetFallbackIcon();
                });
                _visiblePackageCount = visiblePackages.Count;
            }
        }

        private void ReAddMissingParents(IEnumerable<AssetInfo> filteredAssets, List<AssetInfo> data)
        {
            foreach (AssetInfo info in filteredAssets.Where(a => a.ParentId > 0 && !data.Any(d => d.AssetId == a.ParentId)))
            {
                AssetInfo parent = _assets.FirstOrDefault(a => a.AssetId == info.ParentId);
                if (parent != null)
                {
                    data.Add(parent.WithTreeData(parent.GetDisplayName(), parent.AssetId));
                }
            }
        }

        private void AddSubPackagesToTree(List<AssetInfo> data)
        {
            if (_assets.Count == 0) return; // will cause invalid operation exception otherwise

            int maxChildDepth = _assets.Max(a => a.GetChildDepth());
            HashSet<int> existingAssetIds = new HashSet<int>(data.Select(d => d.AssetId));

            for (int depth = 1; depth <= maxChildDepth; depth++)
            {
                Dictionary<int, List<AssetInfo>> subAssets = _assets
                    .Where(a => !a.Exclude && a.GetChildDepth() == depth && !existingAssetIds.Contains(a.AssetId))
                    .OrderByDescending(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase)
                    .GroupBy(a => a.ParentId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (KeyValuePair<int, List<AssetInfo>> pair in subAssets)
                {
                    int parentIndex = data.FindIndex(a => a.AssetId == pair.Key);
                    if (parentIndex >= 0)
                    {
                        foreach (AssetInfo asset in pair.Value)
                        {
                            asset.Depth = data[parentIndex].Depth + 1;
                            AssetInfo newAsset = asset.WithTreeData(asset.GetDisplayName(), asset.AssetId, asset.Depth);
                            data.Insert(parentIndex + 1, newAsset);
                            existingAssetIds.Add(newAsset.AssetId);
                        }
                    }
                }
            }
        }

        private void AddSubPackagesToFeatures(List<AssetInfo> data)
        {
            if (!AssetStore.IsMetadataAvailable()) return;

            for (int i = 0; i < data.Count; i++)
            {
                AssetInfo info = data[i];
                if (!info.IsFeaturePackage()) continue;

                PackageInfo pInfo = AssetStore.GetPackageInfo(info);
                if (pInfo?.dependencies == null) continue; // in case not loaded yet

                foreach (DependencyInfo dependency in pInfo.dependencies.OrderByDescending(d => d.name))
                {
                    AssetInfo package = _assets.FirstOrDefault(a => !a.Exclude && a.SafeName == dependency.name);
                    if (package != null)
                    {
                        AssetInfo newAsset = new AssetInfo(package.ToAsset()).WithTreeData(package.GetDisplayName(), package.AssetId, package.Depth + 1);
                        data.Insert(i + 1, newAsset);
                    }
                }
            }
        }

        private static void ReorderSubPackages(List<AssetInfo> data)
        {
            int maxChildDepth = data.Max(a => a.GetChildDepth());
            for (int depth = 1; depth <= maxChildDepth; depth++)
            {
                Dictionary<int, List<AssetInfo>> subAssets = data.Where(a => a.GetChildDepth() == depth)
                    .OrderBy(a => a.GetDisplayName(), StringComparer.OrdinalIgnoreCase)
                    .GroupBy(a => a.ParentId).ToDictionary(g => g.Key, g => g.ToList());
                foreach (KeyValuePair<int, List<AssetInfo>> pair in subAssets)
                {
                    // remove items at existing positions
                    pair.Value.ForEach(a =>
                    {
                        data.Remove(a);
                    });

                    // find item with id pair.Key and insert items afterward
                    int idx = data.FindIndex(a => a.AssetId == pair.Key);
                    if (idx >= 0)
                    {
                        pair.Value.ForEach(a =>
                        {
                            a.Depth = data[idx].Depth + 1;
                        });
                        data.InsertRange(idx + 1, pair.Value);
                    }
                }
            }
        }

        private string GetLocationDirectory(string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return null;
            try
            {
                string[] arr = location.Split(Asset.SUB_PATH);
                return Path.GetDirectoryName(arr[0]);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private IOrderedEnumerable<AssetInfo> AddPackageOrdering(IEnumerable<AssetInfo> list)
        {
            IOrderedEnumerable<AssetInfo> result = null;
            bool asc = !AI.Config.sortAssetsDescending;
            switch (AI.Config.assetSorting)
            {
                case (int)Columns.AICaptions:
                    result = list.SortBy(a => a.AICaption, asc);
                    break;

                case (int)Columns.Backup:
                    result = list.SortBy(a => a.Backup, asc);
                    break;

                case (int)Columns.Category:
                    result = list.SortBy(a => a.GetDisplayCategory(), asc, StringComparer.OrdinalIgnoreCase);
                    break;

                case (int)Columns.InternalState:
                    result = list.SortBy(a => a.CurrentState, asc);
                    break;

                case (int)Columns.Deprecated:
                    result = list.SortBy(a => a.IsDeprecated, asc);
                    break;

                case (int)Columns.Downloaded:
                    result = list.SortBy(a => a.IsDownloaded, asc);
                    break;

                case (int)Columns.Exclude:
                    result = list.SortBy(a => a.Exclude, asc);
                    break;

                case (int)Columns.Extract:
                    result = list.SortBy(a => a.KeepExtracted, asc);
                    break;

                case (int)Columns.Popularity:
                    result = list.SortBy(a => a.Hotness, asc);
                    break;

                case (int)Columns.Indexed:
                    result = list.SortBy(a => a.IsIndexed, asc);
                    break;

                case (int)Columns.FileCount:
                    result = list.SortBy(a => a.FileCount, asc);
                    break;

                case (int)Columns.License:
                    result = list.SortBy(a => a.License, asc);
                    break;

                case (int)Columns.Location:
                    result = list.SortBy(a => a.Location, asc);
                    break;

                case (int)Columns.Materialized:
                    result = list.SortBy(a => a.IsMaterialized, asc);
                    break;

                case (int)Columns.Name:
                    result = list.SortBy(a => a.GetDisplayName(), asc, StringComparer.OrdinalIgnoreCase);
                    break;

                case (int)Columns.Outdated:
                    result = list.SortBy(a => a.CurrentSubState == Asset.SubState.Outdated, asc);
                    break;

                case (int)Columns.Price:
                    result = list.SortBy(a => a.GetPriceText(), asc);
                    break;

                case (int)Columns.Publisher:
                    result = list.SortBy(a => a.GetDisplayPublisher(), asc, StringComparer.OrdinalIgnoreCase);
                    break;

                case (int)Columns.PurchaseDate:
                    result = list.SortBy(a => a.PurchaseDate, asc);
                    break;

                case (int)Columns.Rating:
                    result = list.SortBy(a => a.RatingCount, asc).ThenSortBy(a => a.AssetRating, asc);
                    break;

                case (int)Columns.RatingCount:
                    result = list.SortBy(a => a.AssetRating, asc).ThenSortBy(a => a.RatingCount, asc);
                    break;

                case (int)Columns.ReleaseDate:
                    result = list.SortBy(a => a.FirstRelease, asc);
                    break;

                case (int)Columns.Size:
                    result = list.SortBy(a => a.PackageSize, asc);
                    break;

                case (int)Columns.Source:
                    result = list.SortBy(a => a.AssetSource, asc);
                    break;

                case (int)Columns.State:
                    result = list.SortBy(a => a.OfficialState, asc);
                    break;

                case (int)Columns.Tags:
                    result = list.SortBy(a => string.Join(", ", a.PackageTags.Select(t => t.Name)), asc);
                    break;

                case (int)Columns.UnityVersions:
                    result = list.SortBy(a => a.SupportedUnityVersions, asc);
                    break;

                case (int)Columns.Update:
                    result = list.SortBy(a => a.IsUpdateAvailable(), asc);
                    break;

                case (int)Columns.UpdateDate:
                    result = list.SortBy(a => a.LastRelease, asc);
                    break;

                case (int)Columns.Version:
                    result = list.SortBy(a => new SemVer(a.GetVersion()), asc);
                    break;

                default:
                    Debug.LogError($"Missing sorting support for column {AI.Config.assetSorting}");
                    break;

            }
            if (result == null) result = list.OrderBy(a => a.LastRelease);

            return result.ThenSortBy(a => a.GetDisplayName(), asc, StringComparer.OrdinalIgnoreCase);
        }

        private static string[] AddCategorizedItem(string[] cats, string[] lastCats, List<AssetInfo> data, AssetInfo info, ref int catIdx)
        {
            // find first difference to previous cat
            if (!ArrayUtility.ArrayEquals(cats, lastCats))
            {
                int firstDiff = 0;
                bool diffFound = false;
                for (int i = 0; i < Mathf.Min(cats.Length, lastCats.Length); i++)
                {
                    if (cats[i] != lastCats[i])
                    {
                        firstDiff = i;
                        diffFound = true;
                        break;
                    }
                }
                if (!diffFound) firstDiff = lastCats.Length;

                for (int i = firstDiff; i < cats.Length; i++)
                {
                    catIdx--;
                    AssetInfo catItem = new AssetInfo().WithTreeData(cats[i], catIdx, i);
                    data.Add(catItem);
                }
            }

            AssetInfo item = info.WithTreeData(info.GetDisplayName(), info.AssetId, cats.Length);
            data.Add(item);

            return cats;
        }

        private async void OpenInPackageView(AssetInfo info)
        {
            AI.Config.tab = 1;
            _assetSearchPhrase = "";
            ResetPackageFilters();
            if (AI.Config.packageViewMode == 0)
            {
                await SelectAndFrame(info);
                HandleAssetTreeSelectionChanged(AssetTreeView.GetSelection());
            }
            else
            {
                PGrid.Select(info);
            }
        }

        private async Task SelectAndFrame(AssetInfo info)
        {
            await Task.Delay(100); // let the tree view update first
            AssetTreeView.SetSelection(new[] {info.AssetId}, TreeViewSelectionOptions.RevealAndFrame);
        }

        private void HandleAssetTreeSelectionChanged(IList<int> ids)
        {
            _selectedTreeAsset = null;
            _selectedTreeAssets = _selectedTreeAssets ?? new List<AssetInfo>();
            _selectedTreeAssets.Clear();

            if (ids.Count == 1 && ids[0] > 0)
            {
                _selectedTreeAsset = AssetTreeModel.Find(ids[0]);
                if (_selectedTreeAsset != null)
                {
                    // refresh immediately for single selections to have all buttons correct at once
                    _selectedTreeAsset.Refresh();
                    _selectedTreeAsset.PackageDownloader?.RefreshState();

                    LoadMediaOnDemand(_selectedTreeAsset);
                }
            }

            // load all selected items but count each only once
            foreach (int id in ids)
            {
                GatherTreeChildren(id, _selectedTreeAssets, AssetTreeModel);
            }
            _selectedTreeAssets = _selectedTreeAssets.Distinct().ToList();

            _assetBulkTags.Clear();

            // initialize download status
            AI.RegisterSelection(_selectedTreeAssets);

            // merge tags
            _selectedTreeAssets.ForEach(info => info.PackageTags?.ForEach(t =>
            {
                if (!_assetBulkTags.ContainsKey(t.Name)) _assetBulkTags.Add(t.Name, new Tuple<int, Color>(0, t.GetColor()));
                _assetBulkTags[t.Name] = new Tuple<int, Color>(_assetBulkTags[t.Name].Item1 + 1, _assetBulkTags[t.Name].Item2);
            }));

            _assetTreeSubPackageCount = _selectedTreeAssets.Count(a => a.ParentId > 0);
            _assetTreeSelectionSize = _selectedTreeAssets.Where(a => a.ParentId == 0).Sum(a => a.PackageSize);
            _assetTreeSelectionTotalCosts = _selectedTreeAssets.Where(a => a.ParentId == 0).Sum(a => a.GetPrice());
            _assetTreeSelectionStoreCosts = _selectedTreeAssets.Where(a => a.ParentId == 0 && a.AssetSource == Asset.Source.AssetStorePackage)
                .Sum(a => a.GetPrice());

            // refresh metadata automatically for single selections
            if (_selectedTreeAsset != null && AI.Config.autoRefreshMetadata && _selectedTreeAsset.ForeignId > 0 && (DateTime.Now - _selectedTreeAsset.LastOnlineRefresh).TotalHours >= AI.Config.metadataTimeout)
            {
                FetchAssetDetails(true, _selectedTreeAsset.AssetId, true); // skip downstream events to avoid hick-ups
            }
        }

        private void LoadMediaOnDemand(AssetInfo info)
        {
            if (info == null) return;

            if (!info.IsMediaLoading() && AI.Config.expandPackageDetails)
            {
                // clear all existing media to conserve memory
                if (AI.Config.packageViewMode == 0)
                {
                    AssetTreeModel.GetData().ForEach(d => d.DisposeMedia());
                }
                else
                {
                    PGrid.packages.ForEach(d => d.DisposeMedia());
                }
                AI.LoadMedia(info);
            }
        }

        private void HandleAssetGridSelectionChanged()
        {
            _selectedTreeAsset = PGrid.selectionItems.Count == 1 ? PGrid.packages[PGrid.selectionTile] : null;
            _selectedTreeAssets = PGrid.selectionItems;

            if (_selectedTreeAsset != null)
            {
                // refresh immediately for single selections to have all buttons correct at once
                _selectedTreeAsset.Refresh();
                _selectedTreeAsset.PackageDownloader?.RefreshState();

                LoadMediaOnDemand(_selectedTreeAsset);
            }

            _assetBulkTags.Clear();

            // initialize download status
            AI.RegisterSelection(_selectedTreeAssets);

            // merge tags
            _selectedTreeAssets.ForEach(info => info.PackageTags?.ForEach(t =>
            {
                if (!_assetBulkTags.ContainsKey(t.Name)) _assetBulkTags.Add(t.Name, new Tuple<int, Color>(0, t.GetColor()));
                _assetBulkTags[t.Name] = new Tuple<int, Color>(_assetBulkTags[t.Name].Item1 + 1, _assetBulkTags[t.Name].Item2);
            }));

            _assetTreeSelectionSize = _selectedTreeAssets.Where(a => a.ParentId == 0).Sum(a => a.PackageSize);
            _assetTreeSelectionTotalCosts = _selectedTreeAssets.Where(a => a.ParentId == 0).Sum(a => a.GetPrice());
            _assetTreeSelectionStoreCosts = _selectedTreeAssets.Where(a => a.ParentId == 0 && a.AssetSource == Asset.Source.AssetStorePackage)
                .Sum(a => a.GetPrice());
        }

        private void OnAssetTreeSelectionChanged(IList<int> ids)
        {
            _selectedMedia = 0;
            HandleAssetTreeSelectionChanged(ids);
        }

        private void OnAssetTreeDoubleClicked(int id)
        {
            if (id <= 0) return;

            AssetInfo info = AssetTreeModel.Find(id);
            OpenInSearch(info);
        }

        private void OnPackageGridDoubleClicked(AssetInfo info)
        {
            OpenInSearch(info);
        }

        private static int GetInspectorWidth()
        {
            return UIStyles.INSPECTOR_WIDTH * (AI.Config.expandPackageDetails && AI.Config.tab == 1 ? 2 : 1);
        }
    }
}
