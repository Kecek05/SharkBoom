#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    public class SortifyFavoritesWindow : EditorWindow
    {
        private const int _toolbarButtonSize = 22;
        private const int _padding = 4;

        private List<GameObject> _favoriteObjects = new List<GameObject>();
        private List<GameObject> _filteredFavorites = new List<GameObject>();
        private Vector2 _scrollPosition;
        private string _searchString = "";
        private bool _needsRefresh = true;

        [MenuItem("Window/Sortify/Favorites")]
        public static void ShowWindow()
        {
            var window = GetWindow<SortifyFavoritesWindow>();
            GUIContent titleContent = new GUIContent("Favorites Hierarchy", EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow").image);
            window.titleContent = titleContent;
            window.RefreshFavoriteObjects();
        }

        private void OnGUI()
        {
            DrawHeaderSection();
            EditorGUILayout.Space(2);
            DrawObjectsSection();
        }

        private void OnFocus()
        {
            _needsRefresh = true;
            RefreshFavoriteObjects();
        }

        private void DrawHeaderSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Favorites:", EditorStyles.boldLabel, GUILayout.Height(24), GUILayout.Width(80));
            GUILayout.FlexibleSpace();

            GUIStyle removeAllStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 110,
                fixedHeight = 20,
                margin = new RectOffset(_padding, _padding, _padding, _padding)
            };

            if (GUILayout.Button("Remove All", removeAllStyle))
            {
                SortifyFavoritesManager.RemoveAllFromFavorites();
                _needsRefresh = true;
                RefreshFavoriteObjects();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            _searchString = EditorGUILayout.TextField(_searchString);
            GUIStyle searchClearStyle = new GUIStyle()
            {
                fixedWidth = _toolbarButtonSize,
                fixedHeight = _toolbarButtonSize,
                margin = new RectOffset(_padding, _padding, _padding, _padding)
            };

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), searchClearStyle))
            {
                _searchString = "";
                GUI.FocusControl(null);
                _needsRefresh = true;
                RefreshFavoriteObjects();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawObjectsSection()
        {
            FilterFavorites();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var obj in _filteredFavorites)
            {
                if (obj == null)
                    continue;

                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_GameObject Icon"), GUILayout.Width(16), GUILayout.Height(16));

                GUIStyle objButtonStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 16,
                    padding = new RectOffset(4, 4, 0, 0),
                    normal = { textColor = Color.white }
                };

                if (GUILayout.Button(obj.name, objButtonStyle))
                {
                    Selection.activeGameObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUIStyle.none))
                {
                    Vector2 mousePosition = Event.current.mousePosition;
                    Vector2 screenPosition = GUIUtility.GUIToScreenPoint(mousePosition);
                    Rect popupRect = new Rect(screenPosition.x - 10, screenPosition.y - 145, 0, 0);
                    string objID = SortifyHelper.GetObjectID(obj);
                    PopupWindow.Show(popupRect, new SortifySettingsPopup(obj, objID));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void RefreshFavoriteObjects()
        {
            if (!_needsRefresh)
                return;

            _favoriteObjects.Clear();
            _favoriteObjects = SortifyFavoritesManager.GetFavorites();
            _needsRefresh = false;
            
            FilterFavorites();
            Repaint();
        }

        private void FilterFavorites()
        {
            if (string.IsNullOrEmpty(_searchString))
            {
                _filteredFavorites = new List<GameObject>(_favoriteObjects);
            }
            else
            {
                _filteredFavorites = _favoriteObjects.Where(obj => obj.name.ToLower().Contains(_searchString.ToLower())).ToList();
            }
        }

        private void OnInspectorUpdate() { }
    }
}
#endif
