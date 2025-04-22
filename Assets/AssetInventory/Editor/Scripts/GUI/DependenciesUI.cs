using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public sealed class DependenciesUI : BasicEditorUI
    {
        private Vector2 _scrollPos;
        private AssetInfo _info;
        private string _dependencyTypes;

        public static DependenciesUI ShowWindow()
        {
            DependenciesUI window = GetWindow<DependenciesUI>("Asset Dependencies");
            window.minSize = new Vector2(500, 200);

            return window;
        }

        public void Init(AssetInfo info)
        {
            _info = info;
            _info.Dependencies.ForEach(i => i.CheckIfInProject());
            _dependencyTypes = string.Join(", ", _info.Dependencies
                .OrderBy(f => f.Type).GroupBy(f => f.Type)
                .Select(g => g.Count() + " " + g.Key + " (" + EditorUtility.FormatBytes(g.Sum(f => f.Size)) + ")"));
        }

        public override void OnGUI()
        {
            if (_info == null || _info.Id == 0)
            {
                EditorGUILayout.HelpBox("Select an asset and trigger the dependency scan to see its dependencies broken down here.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"'{_info.FileName}' in asset '{_info.GetDisplayName()}'", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            int labelWidth = 120;

            if (_info.CrossPackageDependencies.Count > 1)
            {
                GUILabelWithTextNoMax("Dependencies", $"{_info.Dependencies.Count:N0} across {_info.CrossPackageDependencies.Count + 1:N0} packages", labelWidth);
            }
            else
            {
                GUILabelWithTextNoMax("Dependencies", $"{_info.Dependencies.Count:N0}", labelWidth);
            }

            if (_info.SRPSupportPackage != null && _info.SRPSupportPackage.Id > 0)
            {
                GUILabelWithTextNoMax("SRP Support", _info.SRPSupportPackage.DisplayName, labelWidth);
            }

            if (ShowAdvanced()) GUILabelWithTextNoMax("File Types", _dependencyTypes, labelWidth);
            GUILabelWithTextNoMax("Asset Size", EditorUtility.FormatBytes(_info.Size), labelWidth);
            GUILabelWithTextNoMax("Dependencies Size", EditorUtility.FormatBytes(_info.Dependencies.Sum(f => f.Size)), labelWidth);

            if (_info.Dependencies.Any(f => f.InProject))
            {
                GUILabelWithTextNoMax("Remaining", EditorUtility.FormatBytes(_info.Dependencies.Where(f => !f.InProject).Sum(f => f.Size)), labelWidth);
            }

            EditorGUILayout.Space();

            int curAssetId = -1;
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true));

            foreach (AssetFile info in _info.Dependencies.OrderBy(f => f.AssetId).ThenBy(f => f.Path).ThenBy(f => f.Type))
            {
                if (info.AssetId != curAssetId)
                {
                    curAssetId = info.AssetId;
                    Asset curAsset = _info.CrossPackageDependencies.FirstOrDefault(f => f.Id == curAssetId);
                    if (curAsset == null) curAsset = _info.ToAsset();

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"{curAsset.DisplayName}", EditorStyles.miniLabel);
                }

                GUILayout.BeginHorizontal();
                if (info.InProject)
                {
                    EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Installed", "|Already in project"), GUILayout.Width(20));
                }
                else
                {
                    EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Import", "|Needs to be imported"), GUILayout.Width(20));
                }
                bool fromSupport = _info.SRPSupportPackage != null && _info.SRPSupportPackage.Id == info.AssetId;
                EditorGUILayout.LabelField(
                    new GUIContent(info.Path + " (" + EditorUtility.FormatBytes(info.Size) + (fromSupport ? ", SRP Override" : "") + ")", info.Guid),
                    _info.ScriptDependencies.Contains(info) ? UIStyles.ColoredText(Color.yellow) : EditorStyles.label);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
}
