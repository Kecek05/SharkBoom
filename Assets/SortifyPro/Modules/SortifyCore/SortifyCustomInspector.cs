#if UNITY_EDITOR && SORTIFY
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace Sortify
{
    public static class SortifyCustomInspector
    {
        public static void ShowWindow(GameObject obj)
        {
            if (obj == null)
                return;

            Type inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            if (inspectorWindowType == null)
            {
                Debug.LogError("[SortifyCustomInspector] Could not find InspectorWindow type.");
                return;
            }

            EditorWindow inspectorWindow = EditorWindow.CreateInstance(inspectorWindowType) as EditorWindow;
            if (inspectorWindow == null)
            {
                Debug.LogError("[SortifyCustomInspector] Could not create InspectorWindow instance.");
                return;
            }

            inspectorWindow.Show();
            EditorApplication.delayCall += () =>
            {
                Selection.activeGameObject = obj;
                PropertyInfo isLockedProperty = inspectorWindowType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (isLockedProperty != null)
                {
                    isLockedProperty.SetValue(inspectorWindow, true, null);
                }
                else
                {
                    Debug.LogWarning("[SortifyCustomInspector] Could not find isLocked property on InspectorWindow.");
                }
            };
        }
    }
}
#endif
