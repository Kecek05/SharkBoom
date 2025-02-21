#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownAttributeDrawer : PropertyDrawer
    {
        private class DropdownOption
        {
            public string Label;
            public object Value;

            public DropdownOption(string label, object value)
            {
                Label = label;
                Value = value;
            }
        }

        private static Dictionary<string, List<DropdownOption>> _cache = new Dictionary<string, List<DropdownOption>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute)attribute;
            UnityEngine.Object targetObject = property.serializedObject.targetObject as UnityEngine.Object;

            if (targetObject == null)
            {
                EditorGUI.LabelField(position, "Target object is not a UnityEngine.Object.");
                return;
            }

            Type targetType = targetObject.GetType();
            MethodInfo method = targetType.GetMethod(dropdownAttribute.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null)
            {
                EditorGUI.LabelField(position, "Method not found: " + dropdownAttribute.MethodName);
                return;
            }

            string cacheKey = $"{targetObject.GetInstanceID()}_{dropdownAttribute.MethodName}";
            List<DropdownOption> options;

            if (!_cache.TryGetValue(cacheKey, out options))
            {
                object result = method.Invoke(targetObject, null);
                options = new List<DropdownOption>();
                bool isObjectReference = typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType);

                if (dropdownAttribute.IsValuePair)
                {
                    if (result is List<KeyValuePair<string, object>> kvpList)
                    {
                        foreach (var kvp in kvpList)
                            options.Add(new DropdownOption(kvp.Key, kvp.Value));
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Method must return List<KeyValuePair<string, object>>");
                        return;
                    }
                }
                else if (isObjectReference)
                {
                    if (result is IEnumerable<UnityEngine.Object> objectList)
                    {
                        foreach (var obj in objectList)
                        {
                            string labelText = obj != null ? obj.name : "None";
                            options.Add(new DropdownOption(labelText, obj));
                        }
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Method must return IEnumerable<UnityEngine.Object>");
                        return;
                    }
                }
                else
                {
                    if (result is IEnumerable<string> stringList)
                    {
                        foreach (var str in stringList)
                            options.Add(new DropdownOption(str, str));
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Method must return IEnumerable<string> or List<KeyValuePair<string, object>>");
                        return;
                    }
                }

                bool hasValidOptions = false;
                foreach (var option in options)
                {
                    if (option.Value != null)
                    {
                        hasValidOptions = true;
                        break;
                    }
                }

                if (hasValidOptions)
                    _cache.Add(cacheKey, options);
            }

            if (!_cache.TryGetValue(cacheKey, out options))
            {
                EditorGUI.LabelField(position, "No valid options available.");
                return;
            }

            if (options.Count == 0)
            {
                EditorGUI.LabelField(position, "No options returned by method.");
                return;
            }

            string[] optionLabels = new string[options.Count];
            for (int i = 0; i < options.Count; i++)
                optionLabels[i] = options[i].Label;

            EditorGUI.BeginChangeCheck();

            int selectedIndex = GetSelectedIndex(property, options);

            if (selectedIndex == -1)
            {
                selectedIndex = 0;
                SetCurrentPropertyValue(property, options[selectedIndex].Value);
            }

            int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, optionLabels);

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex >= 0 && newIndex < options.Count)
                {
                    object selectedValue = options[newIndex].Value;
                    SetCurrentPropertyValue(property, selectedValue);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private int GetSelectedIndex(SerializedProperty property, List<DropdownOption> options)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    {
                        int currentValue = property.intValue;
                        return options.FindIndex(option => option.Value is int val && val == currentValue);
                    }
                case SerializedPropertyType.Boolean:
                    {
                        bool currentValue = property.boolValue;
                        return options.FindIndex(option => option.Value is bool val && val == currentValue);
                    }
                case SerializedPropertyType.Float:
                    {
                        float currentValue = property.floatValue;
                        return options.FindIndex(option => option.Value is float val && val.Equals(currentValue));
                    }
                case SerializedPropertyType.String:
                    {
                        string currentValue = property.stringValue;
                        return options.FindIndex(option => option.Value is string val && val == currentValue);
                    }
                case SerializedPropertyType.Enum:
                    {
                        int currentValue = property.enumValueIndex;
                        return options.FindIndex(option => option.Value is int val && val == currentValue);
                    }
                case SerializedPropertyType.ObjectReference:
                    {
                        UnityEngine.Object currentObject = property.objectReferenceValue;
                        return options.FindIndex(option => option.Value is UnityEngine.Object obj && obj == currentObject);
                    }
                default:
                    return -1;
            }
        }

        private void SetCurrentPropertyValue(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = Convert.ToBoolean(value);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = value.ToString();
                    break;
                case SerializedPropertyType.Enum:
                    if (value is int enumIndex)
                        property.enumValueIndex = enumIndex;
                    break;
                case SerializedPropertyType.ObjectReference:
                    if (value is UnityEngine.Object obj)
                        property.objectReferenceValue = obj;
                    break;
                default:
                    break;
            }
        }
    }
}
#endif
