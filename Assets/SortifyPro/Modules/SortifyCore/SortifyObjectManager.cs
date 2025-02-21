#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sortify
{
    public static class SortifyObjectManager
    {
        public enum StyleType
        {
            Default,
            Header
        }

        private const string OBJECTS_FILE_NAME = "Sortify_Objects.json";

        private static Color _backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
        private static Color _hoverColor = new Color(0.267f, 0.282f, 0.251f, 1f);
        private static Color _selectedColor = new Color(0.188f, 0.361f, 0.518f, 1f);
        private static Color _selectedUnFocusedColor = new Color(0.302f, 0.302f, 0.302f, 1f);

        private static Dictionary<GameObject, string> _cachedIcons = new Dictionary<GameObject, string>();
        private static Dictionary<GameObject, Color> _cachedColors = new Dictionary<GameObject, Color>();
        private static Dictionary<GameObject, StyleType> _cachedStyles = new Dictionary<GameObject, StyleType>();
        private static Dictionary<GameObject, bool> _cachedShowComponents = new Dictionary<GameObject, bool>();

        private static bool _dataLoaded = false;

        static SortifyObjectManager()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            SaveObjectsToFile();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            LoadObjectsDataFromFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            _dataLoaded = false;
        }

        #region Icons
        public static string LoadCustomIcon(GameObject obj)
        {
            LoadDataIfNeeded();
            return _cachedIcons.TryGetValue(obj, out string iconName) ? iconName : string.Empty;
        }

        public static void SaveCustomIcon(GameObject obj, string iconName)
        {
            LoadDataIfNeeded();
            _cachedIcons[obj] = iconName;
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void ResetIcon(GameObject obj)
        {
            LoadDataIfNeeded();
            _cachedIcons.Remove(obj);
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void DrawCustomIcon(GameObject obj, Rect selectionRect)
        {
            Rect fullRowRect = new Rect(0, selectionRect.y, EditorGUIUtility.currentViewWidth, selectionRect.height);
            Rect iconRect = new Rect(selectionRect.x, selectionRect.y, 16, 16);

            bool isHierarchyFocused = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Hierarchy";
            bool isHovering = fullRowRect.Contains(Event.current.mousePosition);
            bool isSelected = Selection.instanceIDs.Contains(obj.GetInstanceID());

            Color currentBackgroundColor = isSelected
                ? (isHierarchyFocused ? _selectedColor : _selectedUnFocusedColor)
                : (isHovering ? _hoverColor : _backgroundColor);

            EditorGUI.DrawRect(iconRect, currentBackgroundColor);

            string iconName = LoadCustomIcon(obj);
            if (!string.IsNullOrEmpty(iconName))
            {
                Texture2D iconTexture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
                if (iconTexture != null)
                    GUI.DrawTexture(iconRect, iconTexture, ScaleMode.ScaleToFit, true);
            }
        }
        #endregion

        #region Colors
        public static Color? LoadColor(GameObject obj)
        {


            LoadDataIfNeeded();
            return _cachedColors.TryGetValue(obj, out Color color) ? (Color?)color : null;
        }

        public static void SaveColor(GameObject obj, Color color)
        {
            LoadDataIfNeeded();
            _cachedColors[obj] = color;
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void ResetColor(GameObject obj)
        {
            LoadDataIfNeeded();
            _cachedColors.Remove(obj);
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void DrawColor(GameObject obj, Rect selectionRect)
        {
            if (_cachedColors.TryGetValue(obj, out Color color))
            {
                color.a = 0.3f;
                selectionRect.xMin -= 28f;
                selectionRect.xMax += 20f;
                EditorGUI.DrawRect(selectionRect, color);
            }
        }
        #endregion

        #region Styles
        public static StyleType LoadStyleType(GameObject obj)
        {
            LoadDataIfNeeded();
            return _cachedStyles.TryGetValue(obj, out StyleType style) ? style : StyleType.Default;
        }

        public static void SaveStyleType(GameObject obj, StyleType styleType)
        {
            LoadDataIfNeeded();
            _cachedStyles[obj] = styleType;
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void ResetStyleType(GameObject obj)
        {
            LoadDataIfNeeded();
            _cachedStyles.Remove(obj);
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void DrawStyleType(GameObject obj, Rect selectionRect)
        {
            var styleType = LoadStyleType(obj);
            switch (styleType)
            {
                case StyleType.Default:
                    break;
                case StyleType.Header:
                    DrawHeader(obj, _cachedColors.TryGetValue(obj, out var color) ? (Color?)color : null, selectionRect);
                    break;
            }
        }

        private static void DrawHeader(GameObject obj, Color? color, Rect selectionRect)
        {
            if (obj != null)
            {
                Color objectColor = color ?? _backgroundColor;
                selectionRect.xMin -= 28f;
                selectionRect.xMax += 20f;
                EditorGUI.DrawRect(selectionRect, objectColor);

                GUIStyle centerStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 2),
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };

                EditorGUI.LabelField(selectionRect, obj.name, centerStyle);
            }
        }
        #endregion

        #region ShowComponents
        public static bool LoadShowComponents(GameObject obj, bool defaultValue = true)
        {
            LoadDataIfNeeded();
            return _cachedShowComponents.TryGetValue(obj, out bool show) ? show : defaultValue;
        }

        public static void SaveShowComponents(GameObject obj, bool value)
        {
            LoadDataIfNeeded();
            _cachedShowComponents[obj] = value;
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }
        #endregion

        private static void LoadDataIfNeeded()
        {
            if (!_dataLoaded)
            {
                LoadObjectsDataFromFile();
                _dataLoaded = true;
            }
        }

        private static void SaveObjectsToFile()
        {
            var iconsData = new List<CustomIconItem>();
            foreach (var item in _cachedIcons)
            {
                if (item.Key != null)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    iconsData.Add(new CustomIconItem(objID, item.Value));
                }
            }

            var colorsData = new List<CustomColorItem>();
            foreach (var item in _cachedColors)
            {
                if (item.Key != null)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    var color = ColorUtility.ToHtmlStringRGBA(item.Value);
                    colorsData.Add(new CustomColorItem(objID, color));
                }
            }

            var stylesData = new List<CustomStyleItem>();
            foreach (var item in _cachedStyles)
            {
                if (item.Key != null)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    stylesData.Add(new CustomStyleItem(objID, item.Value));
                }
            }

            var showComponentsData = new List<CustomShowComponentsItem>();
            foreach (var item in _cachedShowComponents)
            {
                if (item.Key != null)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    showComponentsData.Add(new CustomShowComponentsItem(objID, item.Value));
                }
            }

            var currentSceneName = SceneManager.GetActiveScene().name;
            var currentSceneData = new CustomObjectData(iconsData, colorsData, stylesData, showComponentsData, currentSceneName);
            var loadedData = SortifyFileManager.LoadFromFile<ObjectsDataSerializer>(OBJECTS_FILE_NAME) ?? new ObjectsDataSerializer();
            loadedData.objectsData ??= new List<CustomObjectData>();
            loadedData.objectsData.RemoveAll(data => data.sceneName == currentSceneName);
            loadedData.objectsData.Add(currentSceneData);
            SortifyFileManager.SaveToFile(OBJECTS_FILE_NAME, loadedData);
        }

        private static void LoadObjectsDataFromFile()
        {
            _cachedIcons.Clear();
            _cachedColors.Clear();
            _cachedStyles.Clear();
            _cachedShowComponents.Clear();

            var loadedData = SortifyFileManager.LoadFromFile<ObjectsDataSerializer>(OBJECTS_FILE_NAME);
            if (loadedData == null || loadedData.objectsData == null)
                return;

            var currentSceneName = SceneManager.GetActiveScene().name;
            var currentSceneData = loadedData.objectsData.FirstOrDefault(data => data.sceneName == currentSceneName);

            if (currentSceneData != null)
            {
                if (currentSceneData.icons != null)
                {
                    foreach (var item in currentSceneData.icons)
                    {
                        var obj = SortifyHelper.FindObjectByID(item.objID);
                        if (obj != null)
                            _cachedIcons[obj] = item.iconName;
                    }
                }

                if (currentSceneData.colors != null)
                {
                    foreach (var item in currentSceneData.colors)
                    {
                        var obj = SortifyHelper.FindObjectByID(item.objID);
                        if (obj != null && ColorUtility.TryParseHtmlString("#" + item.colorValue, out Color color))
                            _cachedColors[obj] = color;
                    }
                }

                if (currentSceneData.styles != null)
                {
                    foreach (var item in currentSceneData.styles)
                    {
                        var obj = SortifyHelper.FindObjectByID(item.objID);
                        if (obj != null)
                            _cachedStyles[obj] = item.styleType;
                    }
                }

                if (currentSceneData.showComponents != null)
                {
                    foreach (var item in currentSceneData.showComponents)
                    {
                        var obj = SortifyHelper.FindObjectByID(item.objID);
                        if (obj != null)
                            _cachedShowComponents[obj] = item.show;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class CustomIconItem
    {
        public string objID;
        public string iconName;

        public CustomIconItem() { }
        public CustomIconItem(string objID, string iconName)
        {
            this.objID = objID;
            this.iconName = iconName;
        }
    }

    [System.Serializable]
    public class CustomColorItem
    {
        public string objID;
        public string colorValue;

        public CustomColorItem() { }
        public CustomColorItem(string objID, string colorValue)
        {
            this.objID = objID;
            this.colorValue = colorValue;
        }
    }

    [System.Serializable]
    public class CustomStyleItem
    {
        public string objID;
        public SortifyObjectManager.StyleType styleType;
        public CustomStyleItem() { }
        public CustomStyleItem(string objID, SortifyObjectManager.StyleType styleType)
        {
            this.objID = objID;
            this.styleType = styleType;
        }
    }

    [System.Serializable]
    public class CustomShowComponentsItem
    {
        public string objID;
        public bool show;

        public CustomShowComponentsItem() { }
        public CustomShowComponentsItem(string objID, bool show)
        {
            this.objID = objID;
            this.show = show;
        }
    }

    [System.Serializable]
    public class CustomObjectData
    {
        public List<CustomIconItem> icons;
        public List<CustomColorItem> colors;
        public List<CustomStyleItem> styles;
        public List<CustomShowComponentsItem> showComponents;
        public string sceneName;

        public CustomObjectData() { }
        public CustomObjectData(List<CustomIconItem> icons, List<CustomColorItem> colors, List<CustomStyleItem> styles, List<CustomShowComponentsItem> showComponents, string sceneName)
        {
            this.icons = icons;
            this.colors = colors;
            this.styles = styles;
            this.showComponents = showComponents;
            this.sceneName = sceneName;
        }
    }

    [System.Serializable]
    public class ObjectsDataSerializer
    {
        public List<CustomObjectData> objectsData;

        public ObjectsDataSerializer() { }
        public ObjectsDataSerializer(List<CustomObjectData> objectsData)
        {
            this.objectsData = objectsData;
        }
    }
}
#endif
