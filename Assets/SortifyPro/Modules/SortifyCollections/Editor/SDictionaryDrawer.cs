#if UNITY_EDITOR && SORTIFY_COLLECTIONS
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(SDictionary<,>), true)]
    public class SDictionaryDrawer : PropertyDrawer
    {
        private class PropertyData
        {
            public ReorderableList ReorderableList;
            public SerializedProperty EntriesProperty;
            public bool IsAddingNewItem;
            public object NewKey;
            public string ErrorMessage;
            public Type KeyType;
            public Type ValueType;
        }

        private Dictionary<string, PropertyData> _propertyData = new Dictionary<string, PropertyData>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var data = GetPropertyData(property);
            InitializeTypes(property, data);

            EditorGUI.BeginProperty(position, label, property);

            float y = position.y;
            Rect foldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EnsureReorderableList(property, data);

                if (data.ReorderableList == null)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                float headerHeight = CalculateHeaderHeight(data);
                data.ReorderableList.headerHeight = headerHeight * 1.1f;

                Rect listRect = new Rect(position.x, y, position.width, data.ReorderableList.GetHeight());
                data.ReorderableList.DoList(listRect);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var data = GetPropertyData(property);
            EnsureReorderableList(property, data);

            if (data.ReorderableList == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float headerHeight = CalculateHeaderHeight(data);
            data.ReorderableList.headerHeight = headerHeight;

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + data.ReorderableList.GetHeight();
        }

        private void InitializeTypes(SerializedProperty property, PropertyData data)
        {
            if (data.KeyType == null || data.ValueType == null)
            {
                if (fieldInfo == null)
                {
                    Debug.LogError("fieldInfo is null. Unable to determine generic types.");
                    return;
                }

                Type genericType = fieldInfo.FieldType;
                if (!genericType.IsGenericType)
                    genericType = fieldInfo.FieldType.BaseType;

                if (genericType != null && genericType.IsGenericType)
                {
                    Type[] genericArgs = genericType.GetGenericArguments();
                    data.KeyType = genericArgs[0];
                    data.ValueType = genericArgs[1];
                }
                else
                {
                    Debug.LogError("SDictionary is not generic.");
                }
            }
        }

        private void EnsureReorderableList(SerializedProperty property, PropertyData data)
        {
            if (data.EntriesProperty == null)
            {
                data.EntriesProperty = property.FindPropertyRelative("_entries");
                if (data.EntriesProperty == null)
                {
                    Debug.LogError("Entries property is null. Ensure your SDictionary has a serialized 'entries' field.");
                    return;
                }
            }

            if (data.ReorderableList == null)
                InitializeReorderableList(data, property.displayName);
        }

        private float CalculateHeaderHeight(PropertyData data)
        {
            float headerHeight = EditorGUIUtility.singleLineHeight;

            if (data.IsAddingNewItem)
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
                headerHeight += lineHeight + verticalSpacing;

                if (!string.IsNullOrEmpty(data.ErrorMessage))
                    headerHeight += lineHeight + verticalSpacing;

                headerHeight += lineHeight + verticalSpacing;
            }

            return headerHeight;
        }

        private PropertyData GetPropertyData(SerializedProperty property)
        {
            string propertyKey = property.propertyPath;

            if (!_propertyData.TryGetValue(propertyKey, out PropertyData data))
            {
                data = new PropertyData();
                _propertyData[propertyKey] = data;
            }

            return data;
        }

        private void InitializeReorderableList(PropertyData data, string displayName)
        {
            if (data.EntriesProperty == null)
            {
                Debug.LogError("Entries property is null during ReorderableList initialization.");
                return;
            }

            data.ReorderableList = new ReorderableList(data.EntriesProperty.serializedObject, data.EntriesProperty, true, true, false, false);
            data.ReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                float buttonWidth = 18f;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

                Rect labelRect = new Rect(rect.x, rect.y, rect.width - buttonWidth * 2 - 10f, lineHeight);
                EditorGUI.LabelField(labelRect, displayName, EditorStyles.boldLabel);

                Rect addButtonRect = new Rect(rect.x + rect.width - buttonWidth * 2 - 5f, rect.y, buttonWidth, lineHeight - 2);
                if (GUI.Button(addButtonRect, "+", EditorStyles.miniButton))
                {
                    data.IsAddingNewItem = true;
                    data.NewKey = Activator.CreateInstance(data.KeyType);
                    data.ErrorMessage = null;
                }

                Rect removeButtonRect = new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, lineHeight - 2);
                if (GUI.Button(removeButtonRect, "-", EditorStyles.miniButton))
                {
                    if (data.ReorderableList.index >= 0 && data.ReorderableList.index < data.EntriesProperty.arraySize)
                    {
                        data.EntriesProperty.DeleteArrayElementAtIndex(data.ReorderableList.index);
                        data.EntriesProperty.serializedObject.ApplyModifiedProperties();
                    }
                }

                if (data.IsAddingNewItem)
                {
                    float yOffset = lineHeight + verticalSpacing;
                    Rect boxRect = new Rect(rect.x - 6, rect.y + yOffset - 2, rect.width + 12, 70);
                    GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

                    Rect keyLabelRect = new Rect(rect.x, rect.y + yOffset, 150, lineHeight);
                    Rect keyFieldRect = new Rect(rect.x + rect.width / 4f, rect.y + yOffset, rect.width - (rect.width / 4f + 5) , lineHeight);
                    EditorGUI.LabelField(keyLabelRect, "Key:");
                    data.NewKey = DrawFieldForType(keyFieldRect, data.KeyType, data.NewKey, "");

                    yOffset += lineHeight + verticalSpacing;

                    bool isDuplicateKey = false;
                    for (int i = 0; i < data.EntriesProperty.arraySize; i++)
                    {
                        SerializedProperty element = data.EntriesProperty.GetArrayElementAtIndex(i);
                        SerializedProperty keyProp = element.FindPropertyRelative("Key");
                        object existingKey = GetPropertyValue(keyProp, data.KeyType);

                        if (Equals(existingKey, data.NewKey))
                        {
                            isDuplicateKey = true;
                            data.ErrorMessage = "Duplicate key";
                            break;
                        }
                        else
                        {
                            data.ErrorMessage = string.Empty;
                        }
                    }

                    if (!string.IsNullOrEmpty(data.ErrorMessage))
                    {
                        Rect errorRect = new Rect(rect.x, rect.y + yOffset, rect.width, lineHeight);
                        EditorGUI.HelpBox(errorRect, data.ErrorMessage, MessageType.Error);
                        yOffset += lineHeight + verticalSpacing;
                    }

                    float buttonSpacing = 10f;
                    Rect cancelButtonRect = new Rect(rect.x, rect.y + yOffset, (rect.width - buttonSpacing) / 2, lineHeight);
                    Rect confirmButtonRect = new Rect(cancelButtonRect.x + cancelButtonRect.width + buttonSpacing, cancelButtonRect.y, cancelButtonRect.width, lineHeight);

                    if (GUI.Button(cancelButtonRect, "Cancel"))
                        data.IsAddingNewItem = false;

                    if (GUI.Button(confirmButtonRect, "Add") && !isDuplicateKey)
                    {
                        data.EntriesProperty.arraySize++;
                        SerializedProperty newElement = data.EntriesProperty.GetArrayElementAtIndex(data.EntriesProperty.arraySize - 1);
                        SetPropertyValue(newElement.FindPropertyRelative("Key"), data.NewKey, data.KeyType);
                        data.EntriesProperty.serializedObject.ApplyModifiedProperties();

                        data.IsAddingNewItem = false;
                    }
                }
            };

            data.ReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = data.EntriesProperty.GetArrayElementAtIndex(index);
                if (element == null)
                    return;

                SerializedProperty keyProp = element.FindPropertyRelative("Key");
                SerializedProperty valueProp = element.FindPropertyRelative("Value");

                float padding = 2f;
                float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

                float keyHeight = EditorGUI.GetPropertyHeight(keyProp, GUIContent.none);
                float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);

                Rect keyRect = new Rect(rect.x, rect.y + padding, rect.width, keyHeight);
                EditorGUI.PropertyField(keyRect, keyProp, new GUIContent("Key"));

                Rect valueRect = new Rect(rect.x, keyRect.y + keyHeight + verticalSpacing, rect.width, valueHeight);

                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = indent + 1;
                EditorGUI.PropertyField(valueRect, valueProp, new GUIContent("Value"), true);
                EditorGUI.indentLevel = indent;
            };

            data.ReorderableList.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = data.EntriesProperty.GetArrayElementAtIndex(index);
                SerializedProperty keyProp = element.FindPropertyRelative("Key");
                SerializedProperty valueProp = element.FindPropertyRelative("Value");

                float keyHeight = EditorGUI.GetPropertyHeight(keyProp, GUIContent.none);
                float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);

                float padding = 2f;
                float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

                return padding + keyHeight + verticalSpacing + valueHeight + padding;
            };
        }

        private object DrawFieldForType(Rect rect, Type type, object value, string label)
        {
            if (type == typeof(int))
            {
                return EditorGUI.IntField(rect, label, value != null ? (int)value : 0);
            }
            else if (type == typeof(float))
            {
                return EditorGUI.FloatField(rect, label, value != null ? (float)value : 0f);
            }
            else if (type == typeof(string))
            {
                return EditorGUI.TextField(rect, label, value != null ? (string)value : "");
            }
            else if (type == typeof(bool))
            {
                return EditorGUI.Toggle(rect, label, value != null ? (bool)value : false);
            }
            else if (type.IsEnum)
            {
                return EditorGUI.EnumPopup(rect, label, value != null ? (Enum)value : (Enum)Enum.GetValues(type).GetValue(0));
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUI.Vector2Field(rect, label, value != null ? (Vector2)value : Vector2.zero);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUI.Vector3Field(rect, label, value != null ? (Vector3)value : Vector3.zero);
            }
            else if (type == typeof(GameObject))
            {
                return EditorGUI.ObjectField(rect, label, value as GameObject, typeof(GameObject), true);
            }
            else if (type == typeof(Transform))
            {
                return EditorGUI.ObjectField(rect, label, value as Transform, typeof(Transform), true);
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                return EditorGUI.ObjectField(rect, label, value as ScriptableObject, type, false);
            }
            else if (type == typeof(Color))
            {
                return EditorGUI.ColorField(rect, label, value != null ? (Color)value : Color.white);
            }
            else if (type == typeof(AnimationCurve))
            {
                return EditorGUI.CurveField(rect, label, value != null ? (AnimationCurve)value : new AnimationCurve());
            }
            else if (type == typeof(Rect))
            {
                return EditorGUI.RectField(rect, label, value != null ? (Rect)value : new Rect());
            }
            else
            {
                EditorGUI.LabelField(rect, label, "Unsupported Type");
                return value;
            }
        }

        private object GetPropertyValue(SerializedProperty property, Type type)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Enum:
                    return Enum.GetValues(type).GetValue(property.enumValueIndex);
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                default:
                    return null;
            }
        }

        private void SetPropertyValue(SerializedProperty property, object value, Type type)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = value as string;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = Convert.ToBoolean(value);
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = Array.IndexOf(Enum.GetValues(type), value);
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)value;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as UnityEngine.Object;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect)value;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = value as AnimationCurve;
                    break;
                default:
                    break;
            }
        }
    }
}
#endif
