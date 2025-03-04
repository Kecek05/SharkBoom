#if UNITY_EDITOR && SORTIFY
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Sortify
{
    public static class SortifyFileManager
    {
        private const string SAVE_IN_PROJECT_KEY = "Sortify_SaveInProject";

        public static void SaveToFile<T>(string fileName, T data)
        {
            bool saveInProject = EditorPrefs.GetBool(SAVE_IN_PROJECT_KEY, false);
            string filePath = saveInProject ? GetProjectFilePath(fileName) : GetLocalFilePath(fileName);
            string jsonData = JsonUtility.ToJson(data, true);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, jsonData);
                if (saveInProject)
                    AssetDatabase.Refresh();
            
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error saving file '{fileName}': {ex.Message}");
            }
        }

        public static T LoadFromFile<T>(string fileName) where T : new()
        {
            bool saveInProject = EditorPrefs.GetBool(SAVE_IN_PROJECT_KEY, false);
            string filePath = saveInProject ? GetProjectFilePath(fileName) : GetLocalFilePath(fileName);

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(jsonData);
            }

            return new T();
        }

        public static void MigrateData(bool saveInProject)
        {
            string sourceFolder = saveInProject ? GetLocalFolder() : GetProjectFolder();
            string destinationFolder = saveInProject ? GetProjectFolder() : GetLocalFolder();

            if (!Directory.Exists(sourceFolder))
            {
                Debug.LogWarning($"Source folder '{sourceFolder}' does not exist. Migration skipped.");
                return;
            }

            Directory.CreateDirectory(destinationFolder);

            foreach (string sourceFile in Directory.GetFiles(sourceFolder))
            {
                if (Path.GetExtension(sourceFile).Equals(".meta", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                string fileName = Path.GetFileName(sourceFile);
                string destinationFile = Path.Combine(destinationFolder, fileName);
                File.Copy(sourceFile, destinationFile, true);
                Debug.Log($"Migrated file '{fileName}' from '{sourceFolder}' to '{destinationFolder}'.");
            }

            if (saveInProject)
                AssetDatabase.Refresh();
        }

        private static string GetLocalFilePath(string fileName) => Path.Combine(GetLocalFolder(), fileName);
        private static string GetLocalFolder()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "SortifyData");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        private static string GetProjectFilePath(string fileName) => Path.Combine(GetProjectFolder(), fileName);
        private static string GetProjectFolder()
        {
            string scriptPath = FindScriptPath("SortifyFileManager");
            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError("Cannot find the 'SortifyFileManager' script in the project.");
                return string.Empty;
            }

            string scriptDirectory = Path.GetDirectoryName(scriptPath);
            string saveFolder = Path.Combine(scriptDirectory, "SaveFiles");

            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            return saveFolder;
        }

        private static string FindScriptPath(string scriptNameWithoutExtension)
        {
            string[] guids = AssetDatabase.FindAssets(scriptNameWithoutExtension + " t:MonoScript");
            if (guids.Length > 0)
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            
            return string.Empty;
        }
    }
}
#endif
