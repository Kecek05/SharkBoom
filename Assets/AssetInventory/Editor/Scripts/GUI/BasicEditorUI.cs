using System;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public abstract class BasicEditorUI : EditorWindow
    {
        public static Texture2D Logo
        {
            get
            {
                if (_logo == null) _logo = UIStyles.LoadTexture("asset-inventory-logo");
                return _logo;
            }
        }

        private static Texture2D _logo;

        public virtual void OnGUI()
        {
            EditorGUILayout.Space();
        }

        protected static void GUILabelWithText(string label, string text, int width = 95, string tooltip = null, bool wrappable = false, bool selectable = false)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content(label, string.IsNullOrWhiteSpace(tooltip) ? label : tooltip), EditorStyles.boldLabel, GUILayout.Width(width));
            if (selectable)
            {
                EditorGUILayout.SelectableLabel(text, wrappable ? EditorStyles.wordWrappedLabel : EditorStyles.label, GUILayout.MaxWidth(UIStyles.INSPECTOR_WIDTH - width - 20), GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            else
            {
                EditorGUILayout.LabelField(UIStyles.Content(text, string.IsNullOrWhiteSpace(tooltip) ? text : tooltip), wrappable ? EditorStyles.wordWrappedLabel : EditorStyles.label, GUILayout.MaxWidth(UIStyles.INSPECTOR_WIDTH - width - 20), GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
        }

        protected static void GUILabelWithTextNoMax(string label, string text, int width = 95, string tooltip = null, bool wrappable = false, bool selectable = false)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(UIStyles.Content(label, string.IsNullOrWhiteSpace(tooltip) ? label : tooltip), EditorStyles.boldLabel, GUILayout.Width(width));
            if (selectable)
            {
                EditorGUILayout.SelectableLabel(text, wrappable ? EditorStyles.wordWrappedLabel : EditorStyles.label, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            else
            {
                EditorGUILayout.LabelField(UIStyles.Content(text, string.IsNullOrWhiteSpace(tooltip) ? text : tooltip), wrappable ? EditorStyles.wordWrappedLabel : EditorStyles.label, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
        }

        protected static void BeginIndentBlock(int widthOverride = 0)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.Space(widthOverride > 0 ? widthOverride : UIStyles.INDENT_WIDTH, false);
            GUILayout.BeginVertical();
        }

        protected static void EndIndentBlock(bool autoSpace = true)
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (autoSpace) EditorGUILayout.Space();
        }

        protected static bool UIShown(string key)
        {
            AssetInventorySettings c = AI.Config;
            bool isAdvanced = c.advancedUI.Contains(key);

            return !isAdvanced || ShowAdvanced() || AI.UICustomizationMode;
        }

        protected static void UILine(string key, Action content, bool alwaysShow = false)
        {
            UIBlock(key, content, alwaysShow, true);
        }

        protected static void UIBlock2(string key, Action content, bool alwaysShow = false)
        {
            UIBlock(key, content, alwaysShow, false, false);
        }

        protected static void UIBlock(string key, Action content, bool alwaysShow = false, bool horizontal = false, bool expand = true)
        {
            AssetInventorySettings c = AI.Config;
            bool isAdvanced = c.advancedUI.Contains(key);

            if (AI.UICustomizationMode)
            {
                Color oldCol = GUI.backgroundColor;
                GUI.backgroundColor = isAdvanced ? Color.red : Color.green;
                GUILayout.BeginVertical("box");
                GUI.backgroundColor = oldCol;
                if (horizontal)
                {
                    GUILayout.BeginHorizontal();
                    content();
                }
                else
                {
                    content();
                    GUILayout.BeginHorizontal();
                    if (expand) GUILayout.FlexibleSpace();
                }
                EditorGUILayout.LabelField(isAdvanced ? "Hidden" : "Shown", EditorStyles.miniLabel, GUILayout.Width(40));
                if (GUILayout.Button(isAdvanced ? "Show" : "Hide", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    if (isAdvanced)
                    {
                        c.advancedUI.Remove(key);
                    }
                    else
                    {
                        c.advancedUI.Add(key);
                    }
                    AI.SaveConfig();
                }
                EditorGUILayout.Space(horizontal ? 1 : 12);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            else
            {
                if (!alwaysShow && isAdvanced && !ShowAdvanced()) return;

                content();
            }
        }

        // shortcut
        protected static bool ShowAdvanced() => AI.ShowAdvanced();
    }
}
