#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ClassDrawerAttribute))]
    public class ClassDrawerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ClassDrawerAttribute customAttribute = (ClassDrawerAttribute)attribute;
            string header = string.IsNullOrEmpty(customAttribute.Header)
                ? SplitCamelCase(property.name)
                : customAttribute.Header;

            GUIStyle headerTextStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                padding = new RectOffset(15, 0, 0, 0)
            };

            GUIStyle headerBoxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(2, 2, 2, 2)
            };

            GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 0, 0)
            };

            float headerHeight = EditorGUIUtility.singleLineHeight + 8;
            float contentHeight = GetPropertyHeight(property, label) - headerHeight;

            Rect boxRect = new Rect(position.x, position.y, position.width, contentHeight + headerHeight);
            GUI.Box(boxRect, GUIContent.none, boxStyle);

            Rect headerRect = new Rect(boxRect.x, boxRect.y, boxRect.width, headerHeight);
            GUI.Box(headerRect, GUIContent.none, headerBoxStyle);

            Rect headerTextRect = new Rect(headerRect.x + 2, headerRect.y + 1, headerRect.width - 4, headerRect.height - 2);
            GUI.Label(headerTextRect, header, headerTextStyle);

            Rect contentPosition = new Rect(position.x + 10, position.y + headerHeight, position.width - 20, position.height - headerHeight);

            EditorGUI.indentLevel++;
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(iterator, endProperty))
                    break;

                float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                Rect propertyRect = new Rect(contentPosition.x, contentPosition.y, contentPosition.width, propertyHeight);

                EditorGUI.PropertyField(propertyRect, iterator, true);
                contentPosition.y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight + 8;

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(iterator, endProperty))
                    break;

                float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                totalHeight += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }

            return totalHeight;
        }

        private string SplitCamelCase(string input)
        {
            string processedInput = Regex.Replace(input, "^(m_|_)", "");
            processedInput = Regex.Replace(processedInput, "(\\B[A-Z])", " $1");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(processedInput);
        }
    }
}
#endif
