using System.Collections.Generic;
using System.Linq;
using AssetInventory;
using UnityEditor;
using UnityEngine;

namespace AssetInventoryUsage
{
    [CustomEditor(typeof (OpenSearch))]
    public class OpenSearchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            #if ASSET_INVENTORY
            GUILayout.Label("UI Examples", EditorStyles.boldLabel);
            if (GUILayout.Button("Search for a car..."))
            {
                ResultPickerUI.Show(path =>
                {
                    EditorUtility.DisplayDialog("Selection", path, "Close");
                }, "Prefabs", "car");
            }
            if (GUILayout.Button("Search with details..."))
            {
                ResultPickerUI window = ResultPickerUI.Show(path =>
                {
                    EditorUtility.DisplayDialog("Selection", path, "Close");
                });
                window.instantSelection = false;
                window.hideDetailsPane = false;
            }
            if (GUILayout.Button("Search for texture sets..."))
            {
                ResultPickerUI window = ResultPickerUI.ShowTextureSelection(path =>
                {
                    EditorUtility.DisplayDialog("Selection", string.Join("\n", path.Select(e => e.Key + ": " + e.Value)), "Close");
                });
                window.instantSelection = false;
                window.hideDetailsPane = false;
            }
            EditorGUILayout.Space();

            GUILayout.Label("Programmatic Examples", EditorStyles.boldLabel);
            if (GUILayout.Button("List all indexed packages"))
            {
                List<AssetInfo> packages = AI.LoadAssets().Where(p => p.IsIndexed && p.SafeName != Asset.NONE).ToList();
                Debug.Log($"Indexed packages: {packages.Count}");
                foreach (AssetInfo package in packages)
                {
                    Debug.Log(package.DisplayName);
                }
            }
            #else
                EditorGUILayout.HelpBox("This feature is only available if Asset Inventory was imported into this project.", MessageType.Info);
            #endif
        }
    }
}