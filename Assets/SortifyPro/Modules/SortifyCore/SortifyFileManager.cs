#if UNITY_EDITOR && SORTIFY
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Sortify
{
    public static class SortifyFileManager
    {
        private const string SAVE_IN_PROJECT_KEY = "Sortify_SaveInProject";
        private const int MAX_BACKUPS = 5;

        public static void SaveToFile<T>(string fileName, T data)
        {
            bool saveInProject = EditorPrefs.GetBool(SAVE_IN_PROJECT_KEY, false);
            string filePath = saveInProject ? GetProjectFilePath(fileName) : GetLocalFilePath(fileName);
            EnsureDirectoryExists(Path.GetDirectoryName(filePath));

            string bakPath = CreateBackup(filePath);
            string tempPath = filePath + ".tmp";
            string jsonData = JsonUtility.ToJson(data, prettyPrint: true);

            try
            {
                TestWriteAccess(filePath);
                File.WriteAllText(tempPath, jsonData);
                File.Copy(tempPath, filePath, overwrite: true);
                File.Delete(tempPath);

                string savedContent = File.ReadAllText(filePath);
                if (savedContent.Equals(jsonData))
                {
                    //Debug.Log($"[Sortify] Successfully saved and verified '{fileName}'.");
                }
                else
                {
                    Debug.LogError($"[Sortify] Verification failed for '{fileName}'. Restoring from backup.");
                    RestoreFromBackup(bakPath, filePath);
                    throw new Exception("Save verification failed – data restored from backup.");
                }

                if (saveInProject)
                    AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Sortify] Error saving JSON to '{fileName}': {ex.Message}. Restoring from backup.");
                if (!string.IsNullOrEmpty(bakPath) && File.Exists(bakPath))
                {
                    RestoreFromBackup(bakPath, filePath);
                }
                else
                {
                    Debug.LogError("[Sortify] No backup available for restore.");
                }
                TryFallbackCopy(tempPath, filePath);
                if (File.Exists(filePath))
                {
                    string fallbackContent = File.ReadAllText(filePath);
                    if (!fallbackContent.Equals(jsonData))
                    {
                        if (!string.IsNullOrEmpty(bakPath))
                            RestoreFromBackup(bakPath, filePath);
                    }
                }
            }
            finally
            {
                if (File.Exists(tempPath)) 
                    File.Delete(tempPath);
            }
        }

        public static T LoadFromFile<T>(string fileName) where T : new()
        {
            bool saveInProject = EditorPrefs.GetBool(SAVE_IN_PROJECT_KEY, false);
            string filePath = saveInProject ? GetProjectFilePath(fileName) : GetLocalFilePath(fileName);

            if (!File.Exists(filePath))
                return new T();

            try
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(jsonData);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Sortify] Corrupted JSON in '{fileName}': {ex.Message}. Trying latest backup.");
                var backup = GetLatestBackup(filePath);
                if (backup != null)
                {
                    try
                    {
                        string bakData = File.ReadAllText(backup.FullName);
                        var recovered = JsonUtility.FromJson<T>(bakData);
                        Debug.Log($"[Sortify] Loaded data from backup: {backup.Name}");
                        File.Copy(backup.FullName, filePath, overwrite: true);
                        return recovered;
                    }
                    catch (Exception bex)
                    {
                        Debug.LogError($"[Sortify] Backup also corrupted: {bex.Message}");
                    }
                }

                Debug.LogError($"[Sortify] All attempts to load data failed. Returning a new instance of {typeof(T).Name}.");
                return new T();
            }
        }

        public static void MigrateData(bool saveInProject)
        {
            string sourceFolder = saveInProject ? GetLocalFolder() : GetProjectFolder();
            string destinationFolder = saveInProject ? GetProjectFolder() : GetLocalFolder();

            if (!Directory.Exists(sourceFolder))
            {
                Debug.LogWarning($"[Sortify] Source folder '{sourceFolder}' does not exist. Migration skipped.");
                return;
            }

            EnsureDirectoryExists(destinationFolder);

            foreach (string sourceFile in Directory.GetFiles(sourceFolder)
                .Where(f => !f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)))
            {
                string fileName = Path.GetFileName(sourceFile);
                string destFile = Path.Combine(destinationFolder, fileName);
                File.Copy(sourceFile, destFile, overwrite: true);
                //Debug.Log($"[Sortify] Migrated '{fileName}' from '{sourceFolder}' to '{destinationFolder}'.");
            }

            string sourceBackups = Path.Combine(sourceFolder, "Backups");
            string destBackups = Path.Combine(destinationFolder, "Backups");
            if (Directory.Exists(sourceBackups))
            {
                EnsureDirectoryExists(destBackups);
                foreach (string bakFile in Directory.GetFiles(sourceBackups)
                    .Where(f => !f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)))
                {
                    string bakName = Path.GetFileName(bakFile);
                    string bakDest = Path.Combine(destBackups, bakName);
                    File.Copy(bakFile, bakDest, overwrite: true);
                    //Debug.Log($"[Sortify] Migrated backup '{bakName}' to '{destBackups}'.");
                }
            }

            if (saveInProject)
                AssetDatabase.Refresh();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static string CreateBackup(string filePath)
        {
            string backupDir = Path.Combine(Path.GetDirectoryName(filePath), "Backups");
            EnsureDirectoryExists(backupDir);

            string baseName = Path.GetFileNameWithoutExtension(filePath);
            string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string bakName = $"{baseName}_{timestamp}.json.bak";
            string bakPath = Path.Combine(backupDir, bakName);

            if(File.Exists(filePath))
            {
                File.Copy(filePath, bakPath, overwrite: true);
            }
            else
            {
                File.WriteAllText(bakPath, "{}");
            }

            var oldBackups = new DirectoryInfo(backupDir).GetFiles($"{baseName}_*.bak").OrderByDescending(f => f.CreationTime).Skip(MAX_BACKUPS);
            foreach (var old in oldBackups)
            {
                try
                {
                    old.Delete();
                    var metaPath = old.FullName + ".meta";
                    if (File.Exists(metaPath)) 
                        File.Delete(metaPath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Sortify] Could not delete old backup or .meta: {e.Message}");
                }
            }

            return bakPath;
        }

        private static void TestWriteAccess(string filePath)
        {
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Append, FileAccess.Write)) { }
            }
            catch (Exception ex)
            {
                throw new IOException($"Write access denied for '{filePath}': {ex.Message}");
            }
        }

        private static void RestoreFromBackup(string backupPath, string destPath)
        {
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, destPath, true);
                Debug.Log($"[Sortify] Restored '{destPath}' from backup '{backupPath}'.");
            }
        }

        private static FileInfo GetLatestBackup(string filePath)
        {
            string backupDir = Path.Combine(Path.GetDirectoryName(filePath), "Backups");
            if (!Directory.Exists(backupDir)) 
                return null;

            string baseName = Path.GetFileNameWithoutExtension(filePath);
            return new DirectoryInfo(backupDir).GetFiles($"{baseName}_*.bak").OrderByDescending(f => f.CreationTime).FirstOrDefault();
        }

        private static void TryFallbackCopy(string src, string dst)
        {
            try 
            { 
                File.Copy(src, dst, overwrite: true); Debug.LogWarning($"[Sortify] Fallback write to '{dst}' succeeded."); 
            }
            catch 
            { 
                Debug.LogError($"[Sortify] Fallback write also failed for '{dst}'."); 
            }
        }
        
        private static string GetLocalFilePath(string fileName) => Path.Combine(GetLocalFolder(), fileName);
        private static string GetLocalFolder()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "SortifyData");
            EnsureDirectoryExists(folderPath);
            return folderPath;
        }

        private static string GetProjectFilePath(string fileName) => Path.Combine(GetProjectFolder(), fileName);
        private static string GetProjectFolder()
        {
            string scriptPath = FindScriptPath("SortifyFileManager");
            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError("[Sortify] Could not find 'SortifyFileManager' script.");
                return string.Empty;
            }

            string dir = Path.GetDirectoryName(scriptPath);
            string saveFolder = Path.Combine(dir, "SaveFiles");
            EnsureDirectoryExists(saveFolder);
            return saveFolder;
        }

        private static string FindScriptPath(string scriptNameWithoutExtension)
        {
            var guids = AssetDatabase.FindAssets(scriptNameWithoutExtension + " t:MonoScript");
            return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : string.Empty;
        }
    }
}
#endif
