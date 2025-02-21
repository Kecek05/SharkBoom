#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            bool shouldShow = ShouldShowProperty(property, showIf);
            if (shouldShow)
                EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            bool shouldShow = ShouldShowProperty(property, showIf);
            if (shouldShow)
                return EditorGUI.GetPropertyHeight(property, label, true);

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        private bool ShouldShowProperty(SerializedProperty property, ShowIfAttribute showIf)
        {
            object targetObject = GetTargetObjectWithPath(property, showIf.ConditionName);
            if (targetObject == null)
                return true;

            bool conditionMet = CompareCondition(targetObject, showIf.CompareValue);
            return showIf.Inverted ? !conditionMet : conditionMet;
        }

        private bool CompareCondition(object conditionValue, object compareValue)
        {
            if (compareValue == null)
            {
                if (conditionValue is bool)
                    return (bool)conditionValue;
                return conditionValue != null;
            }

            return conditionValue != null && conditionValue.Equals(compareValue);
        }

        private object GetTargetObjectWithPath(SerializedProperty property, string path)
        {
            object currentObject = property.serializedObject.targetObject;
            string[] parts = path.Split('.');
            foreach (string part in parts)
            {
                if (currentObject == null)
                    return null;

                Type currentType = currentObject.GetType();
                FieldInfo field = currentType.GetField(part, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    currentObject = field.GetValue(currentObject);
                    continue;
                }

                PropertyInfo propertyInfo = currentType.GetProperty(part, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (propertyInfo != null)
                {
                    currentObject = propertyInfo.GetValue(currentObject, null);
                    continue;
                }

                return null;
            }

            return currentObject;
        }
    }
}
#endif
