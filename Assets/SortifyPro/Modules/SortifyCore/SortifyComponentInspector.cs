#if UNITY_EDITOR && SORTIFY
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    public class SortifyComponentInspector : PopupWindowContent
    {
        private const int _toolbarButtonSize = 16;
        private const int _padding = 4;
        private const int _topPadding = 7;

        private Component _component;
        private Editor _componentEditor;
        private float _inspectorHeight;
        private Vector2 _scrollPosition;

        public SortifyComponentInspector(Component component)
        {
            _component = component;
            _componentEditor = Editor.CreateEditor(component);
            _inspectorHeight = 250;
        }

        public static void Show(Component component, Rect activatorRect)
        {
            var popup = new SortifyComponentInspector(component);
            PopupWindow.Show(activatorRect, popup);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(450, _inspectorHeight);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawHeaderSection();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawComponentInspector();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawHeaderSection()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();
            GUILayout.Space(3);
            if (_component is Behaviour behaviourComponent)
                behaviourComponent.enabled = EditorGUILayout.Toggle(behaviourComponent.enabled, GUILayout.Width(16));
            EditorGUILayout.EndVertical();

            string componentName = FormatComponentName(_component.GetType().Name);
            EditorGUILayout.LabelField(componentName, EditorStyles.boldLabel, GUILayout.Height(24));
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            GUILayout.Space(2);
            _inspectorHeight = GUILayout.HorizontalSlider(_inspectorHeight, 250, 700, GUILayout.Width(80));
            EditorGUILayout.EndVertical();

            GUIStyle style = new GUIStyle()
            {
                fixedWidth = _toolbarButtonSize,
                fixedHeight = _toolbarButtonSize,
                margin = new RectOffset(_padding, _padding, _topPadding, _padding)
            };

            EditorGUILayout.EndHorizontal();
        }

        private void DrawComponentInspector()
        {
            _componentEditor.OnInspectorGUI();
        }

        private string FormatComponentName(string componentName)
        {
            return System.Text.RegularExpressions.Regex.Replace(componentName, "(\\B[A-Z])", " $1");
        }
    }
}
#endif
