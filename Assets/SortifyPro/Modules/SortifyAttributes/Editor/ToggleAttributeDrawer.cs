#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ToggleAttribute))]
    public class ToggleDrawer : PropertyDrawer
    {
        private static Dictionary<string, float> animationProgressDict = new Dictionary<string, float>();
        private static Dictionary<string, bool> targetValuesDict = new Dictionary<string, bool>();
        private static Dictionary<string, float> animationSpeedDict = new Dictionary<string, float>();

        private static bool isUpdateRegistered = false;
        private static double lastUpdateTime = 0.0;

        public ToggleDrawer()
        {
            if (!isUpdateRegistered)
            {
                EditorApplication.update += UpdateAnimations;
                lastUpdateTime = EditorApplication.timeSinceStartup;
                isUpdateRegistered = true;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.LabelField(position, label.text, "Use Toggle with bool.");
                return;
            }

            ToggleAttribute toggleAttribute = (ToggleAttribute)attribute;
            float animationSpeed = toggleAttribute.AnimationSpeed;
            bool currentValue = property.boolValue;

            string propertyPath = property.propertyPath;

            if (!animationProgressDict.ContainsKey(propertyPath))
            {
                animationProgressDict[propertyPath] = currentValue ? 1f : 0f;
                targetValuesDict[propertyPath] = currentValue;
                animationSpeedDict[propertyPath] = animationSpeed;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            Rect toggleRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 45, position.height);

            float animationProgress = animationProgressDict[propertyPath];

            Color backgroundColor = Color.Lerp(new Color(0.25f, 0.25f, 0.25f), new Color(0.4f, 0.4f, 0.4f), animationProgress);
            GUIStyle backgroundStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { background = SortifyHelper.MakeRoundedTexture(128, 74, backgroundColor, 6) }
            };
            GUI.Box(toggleRect, "", backgroundStyle);

            Color handleColor = Color.Lerp(new Color(0.6f, 0.6f, 0.6f), Color.white, animationProgress);
            Rect handleRect = new Rect(
                Mathf.Lerp(toggleRect.x + 2, toggleRect.x + toggleRect.width - 18, animationProgress),
                toggleRect.y + 1, 15, 15);
            Texture2D handleTex = SortifyHelper.MakeRoundedTexture(64, 64, handleColor, 32);
            GUI.DrawTexture(handleRect, handleTex);

            if (Event.current.type == EventType.MouseDown && toggleRect.Contains(Event.current.mousePosition))
            {
                bool newValue = !currentValue;
                property.boolValue = newValue;
                targetValuesDict[propertyPath] = newValue;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                Event.current.Use();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private static void UpdateAnimations()
        {
            bool anyRepaint = false;
            double currentTime = EditorApplication.timeSinceStartup;
            double deltaTime = currentTime - lastUpdateTime;
            lastUpdateTime = currentTime;

            float deltaTimeFloat = Mathf.Clamp((float)deltaTime, 0f, 0.1f); 

            List<string> keys = new List<string>(animationProgressDict.Keys);
            foreach (var key in keys)
            {
                float progress = animationProgressDict[key];
                bool targetValue = targetValuesDict[key];
                float speed = animationSpeedDict[key];

                float targetProgress = targetValue ? 1f : 0f;

                if (!Mathf.Approximately(progress, targetProgress))
                {
                    progress = Mathf.MoveTowards(progress, targetProgress, speed * deltaTimeFloat);
                    animationProgressDict[key] = progress;
                    anyRepaint = true;
                }
            }

            if (anyRepaint)
                RepaintAllInspectorWindows();
        }

        private static void RepaintAllInspectorWindows()
        {
            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window.GetType().Name == "InspectorWindow")
                    window.Repaint();
            }
        }
    }
}
#endif
