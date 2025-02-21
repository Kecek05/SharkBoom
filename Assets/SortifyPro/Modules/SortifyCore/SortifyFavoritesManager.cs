#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sortify
{
    [InitializeOnLoad]
    public static class SortifyFavoritesManager
    {
        private const string FAVORITES_FILE_NAME = "Sortify_Favorites.json";

        private static Dictionary<GameObject, bool> _cachedFavorites = new Dictionary<GameObject, bool>();
        private static bool _dataLoaded = false;

        static SortifyFavoritesManager()
        {
            LoadFavoritesFromFile();
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            SaveFavoritesToFile();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            LoadFavoritesFromFile();
        }

        public static bool IsFavorite(GameObject obj)
        {
            LoadFavoritesIfNeeded();
            return _cachedFavorites.TryGetValue(obj, out bool isFavorite) && isFavorite;
        }

        public static List<GameObject> GetFavorites()
        {
            LoadFavoritesIfNeeded();
            return _cachedFavorites.Where(f => f.Value).Select(f => f.Key).ToList();
        }

        public static void AddToFavorites(GameObject obj)
        {
            LoadFavoritesIfNeeded();
            if (_cachedFavorites.ContainsKey(obj))
            {
                _cachedFavorites[obj] = true;
            }
            else
            {
                _cachedFavorites.Add(obj, true);
            }
            SaveFavoritesToFile();
        }

        public static void RemoveFromFavorites(GameObject obj)
        {
            LoadFavoritesIfNeeded();
            if (_cachedFavorites.ContainsKey(obj))
            {
                _cachedFavorites[obj] = false;
                SaveFavoritesToFile();
            }
        }

        public static void RemoveAllFromFavorites()
        {
            LoadFavoritesIfNeeded();
            _cachedFavorites.Clear();
            SaveFavoritesToFile();
        }

        private static void LoadFavoritesIfNeeded()
        {
            if (!_dataLoaded)
            {
                LoadFavoritesFromFile();
                _dataLoaded = true;
            }
        }

        private static void SaveFavoritesToFile()
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            var loadedData = SortifyFileManager.LoadFromFile<FavoritesDataSerializer>(FAVORITES_FILE_NAME);
            if (loadedData == null || loadedData.scenesFavorites == null)
            {
                loadedData = new FavoritesDataSerializer();
                loadedData.scenesFavorites = new List<SceneFavoritesData>();
            }

            loadedData.scenesFavorites.RemoveAll(s => s.sceneName == currentSceneName);
            var favoritesData = new List<FavoriteItem>();
            foreach (var item in _cachedFavorites)
            {
                if (item.Value)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    favoritesData.Add(new FavoriteItem(objID));
                }
            }

            if (favoritesData.Count > 0)
                loadedData.scenesFavorites.Add(new SceneFavoritesData(currentSceneName, favoritesData));

            SortifyFileManager.SaveToFile(FAVORITES_FILE_NAME, loadedData);
        }

        private static void LoadFavoritesFromFile()
        {
            _cachedFavorites.Clear();

            var loadedData = SortifyFileManager.LoadFromFile<FavoritesDataSerializer>(FAVORITES_FILE_NAME);
            if (loadedData == null || loadedData.scenesFavorites == null)
                return;

            var currentSceneName = SceneManager.GetActiveScene().name;

            var currentSceneData = loadedData.scenesFavorites.FirstOrDefault(s => s.sceneName == currentSceneName);

            if (currentSceneData != null && currentSceneData.favorites != null)
            {
                foreach (var favoriteItem in currentSceneData.favorites)
                {
                    var obj = SortifyHelper.FindObjectByID(favoriteItem.objID);
                    if (obj != null)
                        _cachedFavorites[obj] = true;
                }
            }
        }
    }

    [System.Serializable]
    public class FavoriteItem
    {
        public string objID;

        public FavoriteItem() { }

        public FavoriteItem(string objID)
        {
            this.objID = objID;
        }
    }

    [System.Serializable]
    public class SceneFavoritesData
    {
        public string sceneName;
        public List<FavoriteItem> favorites;

        public SceneFavoritesData() { }

        public SceneFavoritesData(string sceneName, List<FavoriteItem> favorites)
        {
            this.sceneName = sceneName;
            this.favorites = favorites;
        }
    }

    [System.Serializable]
    public class FavoritesDataSerializer
    {
        public List<SceneFavoritesData> scenesFavorites;

        public FavoritesDataSerializer() { }
        public FavoritesDataSerializer(List<SceneFavoritesData> scenesFavorites)
        {
            this.scenesFavorites = scenesFavorites;
        }
    }
}
#endif
