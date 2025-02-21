#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarAttributeDrawer : PropertyDrawer
    {
        private const string EditorPrefsColorKey = "ProgressBarColor_";
        private const string EditorPrefsGradientColorKey = "ProgressBarGradientColor_";
        private bool showColorPicker = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ProgressBarAttribute progressBar = (ProgressBarAttribute)attribute;
            float value = property.floatValue;
            float fillAmount = Mathf.Clamp01((value - progressBar.Min) / (progressBar.Max - progressBar.Min));

            string colorKey = EditorPrefsColorKey + property.propertyPath;
            Color barColor = GetStoredColor(colorKey, Color.gray);
            Color gradientColor = barColor;

            if (progressBar.IsGradient)
            {
                string gradientColorKey = EditorPrefsGradientColorKey + property.propertyPath;
                gradientColor = GetStoredColor(gradientColorKey, Color.green);
            }

            Rect barPosition = new Rect(position.x, position.y, position.width - 25, EditorGUIUtility.singleLineHeight);
            value = DrawProgressBarWithInteraction(barPosition, value, progressBar.Min, progressBar.Max, barColor, gradientColor, progressBar.IsGradient, property.displayName);
            property.floatValue = value;

            GUIStyle changeColorButtonStyle = new GUIStyle()
            {
                fixedHeight = 14,
                fixedWidth = 14
            };
            Rect colorPickerButtonRect = new Rect(position.x + position.width - 20, position.y, 20, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(colorPickerButtonRect, EditorGUIUtility.IconContent("d_eyeDropper.Large"), changeColorButtonStyle))
                showColorPicker = !showColorPicker;

            if (showColorPicker)
            {
                Rect colorFieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 5, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginChangeCheck();
                barColor = EditorGUI.ColorField(colorFieldRect, "Pick Bar Color", barColor);
                if (progressBar.IsGradient)
                {
                    Rect gradientColorFieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2 + 5, position.width, EditorGUIUtility.singleLineHeight);
                    gradientColor = EditorGUI.ColorField(gradientColorFieldRect, "Pick Gradient Color", gradientColor);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    StoreColor(colorKey, barColor);
                    if (progressBar.IsGradient)
                    {
                        string gradientColorKey = EditorPrefsGradientColorKey + property.propertyPath;
                        StoreColor(gradientColorKey, gradientColor);
                    }
                }
            }
        }

        private float DrawProgressBarWithInteraction(Rect position, float value, float min, float max, Color barColor, Color gradientColor, bool isGradient, string label)
        {
            float fillAmount = Mathf.Clamp01((value - min) / (max - min));

            Color finalColor = isGradient ? Color.Lerp(barColor, gradientColor, fillAmount) : barColor;

            EditorGUI.DrawRect(new Rect(position.x, position.y, position.width * fillAmount, position.height), finalColor);

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (position.Contains(Event.current.mousePosition))
                {
                    value = Mathf.Lerp(min, max, (Event.current.mousePosition.x - position.x) / position.width);
                    Event.current.Use();
                }
            }

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.white }
            };

            EditorGUI.LabelField(position, $"{label} ({Mathf.Round(value)}/{max})", style);
            return value;
        }

        private void StoreColor(string key, Color color)
        {
            EditorPrefs.SetFloat(key + "_R", color.r);
            EditorPrefs.SetFloat(key + "_G", color.g);
            EditorPrefs.SetFloat(key + "_B", color.b);
            EditorPrefs.SetFloat(key + "_A", color.a);
        }

        private Color GetStoredColor(string key, Color defaultColor)
        {
            float r = EditorPrefs.GetFloat(key + "_R", defaultColor.r);
            float g = EditorPrefs.GetFloat(key + "_G", defaultColor.g);
            float b = EditorPrefs.GetFloat(key + "_B", defaultColor.b);
            float a = EditorPrefs.GetFloat(key + "_A", defaultColor.a);
            return new Color(r, g, b, a);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return showColorPicker ? EditorGUIUtility.singleLineHeight * (IsGradientEnabled(property) ? 3 : 2) + 5 : EditorGUIUtility.singleLineHeight;
        }

        private bool IsGradientEnabled(SerializedProperty property)
        {
            ProgressBarAttribute progressBar = (ProgressBarAttribute)attribute;
            return progressBar.IsGradient;
        }
    }
}
#endif
