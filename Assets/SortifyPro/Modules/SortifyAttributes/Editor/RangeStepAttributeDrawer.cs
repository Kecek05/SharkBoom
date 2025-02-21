#if UNITY_EDITOR && SORTIFY_ATTRIBUTES
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomPropertyDrawer(typeof(RangeStepAttribute))]
    public class RangeStepAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangeStepAttribute rangeStep = (RangeStepAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.BeginProperty(position, label, property);

                var value = property.floatValue;

                value = EditorGUI.Slider(position, label, value, rangeStep.Min, rangeStep.Max);
                value = Mathf.Round(value / rangeStep.Step) * rangeStep.Step;
                property.floatValue = value;

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use RangeStep with float.");
            }
        }
    }
}
#endif
