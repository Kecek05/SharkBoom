using System;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public sealed class AdvancedSearchUI : EditorWindow
    {
        private static Action<string, string> _onSearchSelection;

        public static AdvancedSearchUI ShowWindow()
        {
            AdvancedSearchUI window = GetWindow<AdvancedSearchUI>("Search Assistant");
            window.minSize = new Vector2(350, 200);
            window.maxSize = window.minSize;

            return window;
        }

        public void Init(Action<string, string> OnSearchSelection)
        {
            _onSearchSelection = OnSearchSelection;
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("Simple Searches", EditorStyles.largeLabel);
            ShowSample("'Car' prefabs", "car", "Prefabs");
            ShowSample("'Books' but not 'book shelves' or 'bookmarks'", "book -shelf -mark", "Prefabs");
            ShowSample("Search for an exact phrase", "~book shelf");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced Searches", EditorStyles.largeLabel);
            ShowSample("Results from free packages only", "=AssetFile.FileName like '%TEXT%' and Asset.PriceEur = 0");
            ShowSample("Audio files between 10-20 seconds in length", "=AssetFile.Length >= 10 and AssetFile.Length <= 20", "Audio");
            ShowSample("Files with an AI caption available", "=AssetFile.AICaption not null");
        }

        private void ShowSample(string text, string searchPhrase, string searchType = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(text, EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Set"))
            {
                _onSearchSelection?.Invoke(searchPhrase, searchType);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
