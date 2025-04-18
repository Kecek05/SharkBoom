using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public sealed class PreviewWizardUI : BasicEditorUI
    {
        private const string BASE_JOIN = "inner join Asset on Asset.Id = AssetFile.AssetId where Asset.Exclude = false";

        private Vector2 _scrollPos;
        private List<AssetInfo> _assets;
        private List<AssetInfo> _allAssets;
        private List<AssetInfo> _allFiles;
        private int _totalFiles;
        private int _providedFiles;
        private int _recreatedFiles;
        private int _erroneousFiles;
        private int _missingFiles;
        private int _noPrevFiles;
        private int _scheduledFiles;
        private int _imageFiles;
        private bool _showAdv;

        public static PreviewWizardUI ShowWindow()
        {
            PreviewWizardUI window = GetWindow<PreviewWizardUI>("Previews Wizard");
            window.minSize = new Vector2(430, 300);
            window.maxSize = new Vector2(window.minSize.x, 500);

            return window;
        }

        public void Init(List<AssetInfo> assets, List<AssetInfo> allAssets)
        {
            _assets = assets;
            _allAssets = allAssets;

            GeneratePreviewOverview();
        }

        private void GeneratePreviewOverview()
        {
            string assetFilter = PreviewPipeline.GetAssetFilter(_assets);
            string countQuery = "select count(*) from AssetFile";

            _totalFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} {assetFilter}");
            _imageFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} {assetFilter} and AssetFile.Type in ('" + string.Join("','", AI.TypeGroups["Images"]) + "')");
            _providedFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} and AssetFile.PreviewState = ? {assetFilter}", AssetFile.PreviewOptions.Provided);
            _recreatedFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} and AssetFile.PreviewState = ? {assetFilter}", AssetFile.PreviewOptions.Custom);
            _erroneousFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} and AssetFile.PreviewState = ? {assetFilter}", AssetFile.PreviewOptions.Error);
            _missingFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} and AssetFile.PreviewState = ? {assetFilter}", AssetFile.PreviewOptions.None);
            _noPrevFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} and AssetFile.PreviewState = ? {assetFilter}", AssetFile.PreviewOptions.NotApplicable);
            _scheduledFiles = DBAdapter.DB.ExecuteScalar<int>($"{countQuery} {BASE_JOIN} and AssetFile.PreviewState = ? {assetFilter}", AssetFile.PreviewOptions.Redo);
        }

        private void Schedule(AssetFile.PreviewOptions state)
        {
            string assetFilter = PreviewPipeline.GetAssetFilter(_assets);
            string query = $"update AssetFile set PreviewState = ? from (select * from Asset where Exclude = false) as Asset where Asset.Id = AssetFile.AssetId and AssetFile.PreviewState = ? {assetFilter}";
            DBAdapter.DB.Execute(query, AssetFile.PreviewOptions.Redo, state);

            GeneratePreviewOverview();
        }

        private void Schedule(string queryExt = "")
        {
            string assetFilter = PreviewPipeline.GetAssetFilter(_assets);
            string query = $"update AssetFile set PreviewState = ? from (select * from Asset where Exclude = false) as Asset where Asset.Id = AssetFile.AssetId {queryExt} {assetFilter}";
            DBAdapter.DB.Execute(query, AssetFile.PreviewOptions.Redo);

            GeneratePreviewOverview();
        }

        public override void OnGUI()
        {
            int labelWidth = 120;
            int buttonWidth = 90;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("This wizard will help you recreate preview images in case they are missing or incorrect.", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                EditorUtility.DisplayDialog("Preview Images Overview", "When indexing Unity packages, preview images are typically bundled with them. These are often good but not always. This can result in empty previews, pink images, dark images and more. Colors and lighting will also differ between Unity versions where the previews were initially created. Audio files will for example have different shades of yellow. Bundled preview images are limited to 128 by 128 pixels.\n\nAsset Inventory can easily recreate preview images and offers advanced options like creating bigger previews.", "OK");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Current Selection", EditorStyles.largeLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Packages", EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            EditorGUILayout.LabelField(_assets != null && _assets.Count > 0 ? (_assets.Count + (_assets.Count == 1 ? $" ({_assets[0].GetDisplayName()})" : "")) : "-Full Database-", EditorStyles.wordWrappedLabel);
            if (_assets != null && _assets.Count > 0 && GUILayout.Button(UIStyles.Content("X", "Clear Selection"), GUILayout.Width(20)))
            {
                _assets = null;
                GeneratePreviewOverview();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILabelWithText("Total Files", $"{_totalFiles:N0}", labelWidth);
            EditorGUI.BeginDisabledGroup(_totalFiles == 0);
            if (GUILayout.Button("Schedule Recreation", GUILayout.ExpandWidth(false))) Schedule();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILabelWithText($"{UIStyles.INDENT}Pre-Provided", $"{_providedFiles:N0}", labelWidth, "Preview images that were provided with the package.");
            EditorGUI.BeginDisabledGroup(_providedFiles == 0);
            if (GUILayout.Button("Schedule Recreation", GUILayout.ExpandWidth(false))) Schedule(AssetFile.PreviewOptions.Provided);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILabelWithText($"{UIStyles.INDENT}Recreated", $"{_recreatedFiles:N0}", labelWidth, "Preview images that were recreated by Asset Inventory.");
            EditorGUI.BeginDisabledGroup(_recreatedFiles == 0);
            if (GUILayout.Button("Schedule Recreation", GUILayout.ExpandWidth(false))) Schedule(AssetFile.PreviewOptions.Custom);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILabelWithText($"{UIStyles.INDENT}Missing", $"{_missingFiles:N0}", labelWidth);
            EditorGUI.BeginDisabledGroup(_missingFiles == 0);
            if (GUILayout.Button("Schedule Recreation", GUILayout.ExpandWidth(false))) Schedule(AssetFile.PreviewOptions.None);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILabelWithText($"{UIStyles.INDENT}Erroneous", $"{_erroneousFiles:N0}", labelWidth, "Preview images where a previous recreation attempt failed.");
            EditorGUI.BeginDisabledGroup(_erroneousFiles == 0);
            if (GUILayout.Button("Schedule Recreation", GUILayout.ExpandWidth(false))) Schedule(AssetFile.PreviewOptions.Error);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILabelWithText($"{UIStyles.INDENT}Not Applicable", $"{_noPrevFiles:N0}", labelWidth, "Files for which typically no previews are created, e.g. documents, scripts, controllers. Only a generic icon will be shown.");
            EditorGUI.BeginDisabledGroup(_noPrevFiles == 0);
            if (ShowAdvanced() && GUILayout.Button("Schedule Recreation", GUILayout.ExpandWidth(false))) Schedule(AssetFile.PreviewOptions.NotApplicable);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILabelWithText("Image Files", $"{_imageFiles:N0}", labelWidth);
            EditorGUI.BeginDisabledGroup(_imageFiles == 0);
            if (GUILayout.Button("Schedule Recreation", GUILayout.ExpandWidth(false)))
            {
                Schedule("and AssetFile.Type in ('" + string.Join("','", AI.TypeGroups["Images"]) + "')");
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILabelWithText("Scheduled", $"{_scheduledFiles:N0}", labelWidth);

            EditorGUILayout.Space();
            _showAdv = EditorGUILayout.Foldout(_showAdv, "Advanced");
            if (_showAdv)
            {
                if (GUILayout.Button("Show Preview Folder", GUILayout.Width(200)))
                {
                    string path = AI.GetPreviewFolder();
                    if (_assets != null && _assets.Count == 1)
                    {
                        path = _assets[0].GetPreviewFolder(AI.GetPreviewFolder());
                    }
                    EditorUtility.RevealInFinder(path);
                }
                EditorGUI.BeginDisabledGroup(AI.IndexingInProgress);
                if (GUILayout.Button("Revert to Provided", GUILayout.Width(200))) RestorePreviews();
                // if (GUILayout.Button("Scan for Missing Preview Files", GUILayout.Width(300))) ;
                // if (GUILayout.Button("Scan Pre-Provided for Errors", GUILayout.Width(300))) ;
                // if (GUILayout.Button("Scan Image Previews for Incorrect Dimensions", GUILayout.Width(300))) ;
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (AI.IndexingInProgress)
            {
                EditorGUILayout.BeginHorizontal();
                UIStyles.DrawProgressBar((float)AssetProgress.SubProgress / AssetProgress.SubCount, $"Progress: {AssetProgress.SubProgress}/{AssetProgress.SubCount}");
                if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false), GUILayout.Height(14)))
                {
                    AssetProgress.CancellationRequested = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(_scheduledFiles == 0);
                if (GUILayout.Button($"Recreate {_scheduledFiles} Scheduled", GUILayout.Height(50)))
                {
                    RecreatePreviews();
                }
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Refresh", GUILayout.Width(buttonWidth), GUILayout.Height(50))) GeneratePreviewOverview();
                EditorGUILayout.EndHorizontal();
            }
        }

        private async void RestorePreviews()
        {
            AI.IndexingInProgress = true;
            AssetProgress.CancellationRequested = false;

            int restored = await new PreviewPipeline().RestorePreviews(_assets, _allAssets);
            Debug.Log($"Previews restored: {restored}");

            AI.IndexingInProgress = false;
            AI.TriggerPackageRefresh();
            GeneratePreviewOverview();
        }

        private async void RecreatePreviews()
        {
            AI.IndexingInProgress = true;
            AssetProgress.CancellationRequested = false;

            int created = await new PreviewPipeline().RecreateScheduledPreviews(_assets, _allAssets);
            Debug.Log($"Preview recreation done: {created} created.");

            AI.IndexingInProgress = false;
            AI.TriggerPackageRefresh();
            GeneratePreviewOverview();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}