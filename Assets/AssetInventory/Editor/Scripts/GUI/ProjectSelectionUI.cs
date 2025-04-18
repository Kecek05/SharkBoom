using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public sealed class ProjectSelectionUI : PopupWindowContent
    {
        private Action<AssetInfo> _onSelection;
        private List<AssetInfo> _assetInfo;
        private Vector2 _scrollPos;

        public void Init(Action<AssetInfo> onSelection = null)
        {
            _onSelection = onSelection;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(350, 250);
        }

        public void SetAssets(List<AssetInfo> infos)
        {
            _assetInfo = infos
                .Where(a => a.AssetSource == Asset.Source.AssetManager)
                .OrderBy(a => a.OriginalLocation)
                .ThenBy(a => a.ToAsset().GetRootAsset().DisplayName + (string.IsNullOrWhiteSpace(a.Location) ? string.Empty : "/" + a.Location))
                .ToList();
        }

        public override void OnGUI(Rect rect)
        {
            if (_assetInfo == null || _assetInfo.Count == 0)
            {
                EditorGUILayout.HelpBox("No Asset Manager projects created or indexed yet. You need to update the index (under Settings) to have the most up-to-date info from the Unity Cloud synced.", MessageType.Info);
            }
            else
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true));
                string lastOrg = null;
                foreach (AssetInfo asset in _assetInfo)
                {
                    Asset root = asset.ToAsset().GetRootAsset();

                    if (lastOrg != root.OriginalLocation)
                    {
                        EditorGUILayout.LabelField($"Org: {root.OriginalLocation}");
                        lastOrg = root.OriginalLocation;
                    }

                    string buttonText = $"{root.DisplayName}";
                    if (!string.IsNullOrWhiteSpace(asset.Location)) buttonText += $"/{asset.Location}";
                    if (GUILayout.Button(buttonText))
                    {
                        _onSelection?.Invoke(asset);
                        editorWindow.Close();
                    }
                }
                GUILayout.EndScrollView();
            }
        }
    }
}
