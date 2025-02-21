#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(EditableAttribute))]
    public class EditableAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight + 8;
            if (property.isExpanded && property.objectReferenceValue != null)
            {
                SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                serializedObject.Update();
                SerializedProperty iterator = serializedObject.GetIterator();

                if (iterator.NextVisible(true))
                {
                    while (iterator.NextVisible(false))
                    {
                        if (iterator.name == "m_Script")
                            continue;

                        totalHeight += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            return totalHeight + 10f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            GUIStyle boxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(2, 2, 2, 2)
            };

            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + 8);
            GUI.Box(headerRect, GUIContent.none, boxStyle);

            Rect toggleRect = new Rect(headerRect.x + 20, headerRect.y + 2, 15, EditorGUIUtility.singleLineHeight);
            Rect labelRect = new Rect(headerRect.x + 30, headerRect.y + 2, 120, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(headerRect.x + headerRect.width / 2.5f, headerRect.y + 2, headerRect.width - headerRect.width / 2.5f - 5, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(toggleRect, property.isExpanded, GUIContent.none);
            EditorGUI.LabelField(labelRect, label.text, headerStyle);
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);

            if (property.isExpanded && property.objectReferenceValue != null)
            {
                SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                serializedObject.Update();
                SerializedProperty iterator = serializedObject.GetIterator();
                
                float yOffset = headerRect.y + EditorGUIUtility.singleLineHeight + 10;
                if (iterator.NextVisible(true))
                {
                    EditorGUI.indentLevel++;
                    while (iterator.NextVisible(false))
                    {
                        if (iterator.name == "m_Script")
                            continue;

                        float height = EditorGUI.GetPropertyHeight(iterator, true);
                        Rect contentRect = new Rect(position.x + EditorGUI.indentLevel * 15f, yOffset, position.width - EditorGUI.indentLevel * 15f, height);

                        EditorGUI.PropertyField(contentRect, iterator, true);
                        yOffset += height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    EditorGUI.indentLevel--;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif
