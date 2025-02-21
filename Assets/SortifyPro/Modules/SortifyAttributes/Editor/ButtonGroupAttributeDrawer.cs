#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ButtonGroupAttribute))]
    public class ButtonGroupAttributeDrawer : PropertyDrawer
    {
        private const float ButtonHeight = 25f;
        private const float ButtonSpacing = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ButtonGroupAttribute buttonGroup = (ButtonGroupAttribute)attribute;
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label.text, "Use with Enum.");
                return;
            }

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 5, 5),
                fixedHeight = ButtonHeight,
            };

            EditorGUI.BeginProperty(position, label, property);
            
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float currentY = indentedPosition.y;
            if (buttonGroup.ShowVariableName)
            {
                float labelHeight = EditorGUIUtility.singleLineHeight;
                Rect labelRect = new Rect(indentedPosition.x, currentY, indentedPosition.width, labelHeight);
                EditorGUI.LabelField(labelRect, label);
                currentY += labelHeight + ButtonSpacing;
            }

            float buttonsWidth = indentedPosition.width;
            string[] enumNames = property.enumDisplayNames;

            if (buttonGroup.IsVertical)
            {
                for (int i = 0; i < enumNames.Length; i++)
                {
                    Rect buttonRect = new Rect(indentedPosition.x, currentY, buttonsWidth, ButtonHeight);
                    if (GUI.Button(buttonRect, enumNames[i], buttonStyle))
                        property.enumValueIndex = i;
            
                    currentY += ButtonHeight + ButtonSpacing;
                }
            }
            else
            {
                int buttonCount = enumNames.Length;
                if (buttonCount > 0)
                {
                    float totalSpacing = ButtonSpacing * (buttonCount - 1);
                    float buttonWidth = (buttonsWidth - totalSpacing) / buttonCount;

                    for (int i = 0; i < buttonCount; i++)
                    {
                        Rect buttonRect = new Rect(indentedPosition.x + i * (buttonWidth + ButtonSpacing), currentY, buttonWidth, ButtonHeight);
                        if (GUI.Button(buttonRect, enumNames[i], buttonStyle))
                            property.enumValueIndex = i;
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ButtonGroupAttribute buttonGroup = (ButtonGroupAttribute)attribute;
            if (property.propertyType != SerializedPropertyType.Enum)
                return EditorGUIUtility.singleLineHeight;

            float totalHeight = 0f;
            if (buttonGroup.ShowVariableName)
                totalHeight += EditorGUIUtility.singleLineHeight + ButtonSpacing;

            if (buttonGroup.IsVertical)
            {
                totalHeight += (ButtonHeight + ButtonSpacing) * property.enumDisplayNames.Length;
            }
            else
            {
                totalHeight += ButtonHeight;
            }

            return totalHeight;
        }
    }
}
#endif
