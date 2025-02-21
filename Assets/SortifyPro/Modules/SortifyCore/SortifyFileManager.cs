#if UNITY_EDITOR && SORTIFY
using System.IO;
using UnityEngine;

namespace Sortify
{
    public static class SortifyFileManager
    {
        public static void SaveToFile<T>(string fileName, T data)
        {
            string filePath = GetFilePath(fileName);
            string jsonData = JsonUtility.ToJson(data, true);
            try
            {
                File.WriteAllText(filePath, jsonData);
            }
            catch(IOException ex)
            {
                Debug.LogError($"Error saving file '{fileName}': {ex.Message}");
            }
        }

        public static T LoadFromFile<T>(string fileName) where T : new()
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(jsonData);
            }
            return new T();
        }

        private static string GetFilePath(string fileName)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "SortifyData");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return Path.Combine(folderPath, fileName);
        }
    }
}
#endif
