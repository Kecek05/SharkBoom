using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if UNITY_2021_2_OR_NEWER
using SharpCompress.Archives;
using SharpCompress.Common;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace AssetInventory
{
    public static class IOUtils
    {
        private const string LONG_PATH_PREFIX = @"\\?\";

        public static string ToLongPath(string path)
        {
            if (path == null) return null;

#if UNITY_EDITOR_WIN && UNITY_2020_2_OR_NEWER // support was only added in that Mono version
            // see https://learn.microsoft.com/en-us/answers/questions/240603/c-app-long-path-support-on-windows-10-post-1607-ne
            path = path.Replace("/", "\\"); // in case later concatenations added /
            if (path.StartsWith(LONG_PATH_PREFIX)) return path;
            return $"{LONG_PATH_PREFIX}{path}";
#else
            return path;
#endif
        }

        public static string ToShortPath(string path)
        {
#if UNITY_EDITOR_WIN && UNITY_2020_2_OR_NEWER
            return path?.Replace(LONG_PATH_PREFIX, string.Empty).Replace("\\", "/");
#else
            return path;
#endif
        }

        public static bool PathContainsInvalidChars(string path)
        {
            return !string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        public static string RemoveInvalidChars(string path)
        {
            return string.Concat(path.Split(Path.GetInvalidFileNameChars()));
        }

        public static string MakeProjectRelative(string path)
        {
            if (path.Replace("\\", "/").StartsWith(Application.dataPath.Replace("\\", "/")))
            {
                return "Assets" + path.Substring(Application.dataPath.Length);
            }
            return path;
        }

        public static string CreateTempFolder()
        {
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectoryPath);

            return tempDirectoryPath;
        }

        public static async Task<List<string>> FindMatchesInBinaryFile(string filePath, List<string> searchStrings, int bufferSize = 1048576)
        {
            HashSet<string> foundMatches = new HashSet<string>();
            byte[] buffer = new byte[bufferSize];

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                int bytesRead;
                List<Task> searchTasks = new List<Task>();
                StringBuilder chunk = new StringBuilder();

                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    chunk.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    string chunkContent = chunk.ToString();

                    searchTasks.Add(Task.Run(() =>
                    {
                        Parallel.ForEach(searchStrings, searchString =>
                        {
                            if (chunkContent.IndexOf(searchString, StringComparison.Ordinal) >= 0)
                            {
                                lock (foundMatches)
                                {
                                    foundMatches.Add(searchString);
                                }
                            }
                        });
                    }));

                    if (chunk.Length > bufferSize * 2)
                    {
                        chunk.Remove(0, chunk.Length - bufferSize);
                    }
                }

                await Task.WhenAll(searchTasks);
            }

            return foundMatches.ToList();
        }

        // faster helper method using also fewer allocations 
        public static string GetExtensionWithoutDot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            int dotIndex = path.LastIndexOf('.');
            if (dotIndex >= 0 && dotIndex < path.Length - 1) // Ensure there is an extension and it's not the last character
            {
                return path.Substring(dotIndex + 1);
            }

            return string.Empty;
        }

        public static string GetFileName(string path, bool returnOriginalOnError = true, bool quiet = true)
        {
            try
            {
                return Path.GetFileName(path);
            }
            catch (Exception e)
            {
                if (!quiet) Debug.LogError($"Illegal characters in path '{path}': {e}");
                return returnOriginalOnError ? path : null;
            }
        }

        public static string ReadFirstLine(string path)
        {
            string result = null;
            try
            {
                using (StreamReader reader = new StreamReader(ToLongPath(path)))
                {
                    result = reader.ReadLine();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading file '{path}': {e.Message}");
            }

            return result;
        }

        public static bool TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task DeleteFileOrDirectory(string path, int retries = 3)
        {
            while (retries >= 0)
            {
                try
                {
                    FileUtil.DeleteFileOrDirectory(path); // use Unity method to circumvent unauthorized access that can happen every now and then
                    break;
                }
                catch
                {
                    retries--;
                    if (retries >= 0) await Task.Delay(200);
                }
            }
        }

        // Regex version
        public static IEnumerable<string> GetFiles(string path, string searchPatternExpression = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(path, "*", searchOption)
                .Where(file => reSearchPattern.IsMatch(Path.GetExtension(file)));
        }

        // Takes multiple patterns and executes in parallel
        public static IEnumerable<string> GetFiles(string path, IEnumerable<string> searchPatterns, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return searchPatterns.AsParallel()
                .SelectMany(searchPattern => Directory.EnumerateFiles(path, searchPattern, searchOption));
        }

        public static IEnumerable<string> GetFilesSafe(string rootPath, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            Queue<string> dirs = new Queue<string>();
            dirs.Enqueue(rootPath);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Dequeue();
                string[] subDirs;
                string[] files;

                // Try to get files in the current directory
                try
                {
                    files = Directory.GetFiles(currentDir, searchPattern);
                }
                catch (Exception)
                {
                    // Skip this directory if access is denied
                    // Skip if the directory is not found
                    // Skip if timeout happens
                    continue;
                }

                foreach (string file in files)
                {
                    yield return file;
                }

                if (searchOption == SearchOption.TopDirectoryOnly) continue;

                // Try to get subdirectories
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (string subDir in subDirs)
                {
                    dirs.Enqueue(subDir);
                }
            }
        }

        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public static bool IsSameDirectory(string path1, string path2)
        {
            DirectoryInfo di1 = new DirectoryInfo(path1);
            DirectoryInfo di2 = new DirectoryInfo(path2);

            return string.Equals(di1.FullName, di2.FullName, StringComparison.OrdinalIgnoreCase);
        }

        public static void CopyDirectory(string sourceDir, string destDir, bool includeSubDirs = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (includeSubDirs)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string tempPath = Path.Combine(destDir, subDir.Name);
                    CopyDirectory(subDir.FullName, tempPath, includeSubDirs);
                }
            }
        }

        public static async Task<long> GetFolderSize(string folder, bool async = true)
        {
            if (!Directory.Exists(folder)) return 0;
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            try
            {
                if (async)
                {
                    // FIXME: this can crash Unity
                    return await Task.Run(() => dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length));
                }
                return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns a combined path with unified slashes
        /// </summary>
        /// <returns></returns>
        public static string PathCombine(params string[] path)
        {
            return Path.GetFullPath(Path.Combine(path));
        }

        public static string ExecuteCommand(string command, string arguments)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command, arguments)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process {StartInfo = processStartInfo})
                {
                    process.Start();
                    string result = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return result;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing command '{command}': {e.Message}");
                return null;
            }
        }

#if UNITY_2021_2_OR_NEWER
        public static async Task<bool> DownloadFile(Uri uri, string targetFile)
        {
            UnityWebRequest request = UnityWebRequest.Get(uri);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                return false;
            }

            byte[] data = request.downloadHandler.data;
            await File.WriteAllBytesAsync(targetFile, data);

            return true;
        }
#endif

#if UNITY_2021_2_OR_NEWER
        public static bool ExtractArchive(string archiveFile, string targetFolder)
        {
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            try
            {
                using (IArchive archive = ArchiveFactory.Open(archiveFile))
                {
                    foreach (IArchiveEntry entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Key)) continue;

                        if (!entry.IsDirectory)
                        {
                            string fullOutputPath = Path.Combine(targetFolder, entry.Key);
                            string directoryName = Path.GetDirectoryName(fullOutputPath);
                            if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

                            entry.WriteToDirectory(targetFolder, new ExtractionOptions
                            {
                                Overwrite = true,
                                ExtractFullPath = true
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not extract file from archive '{archiveFile}'. The process was potentially interrupted, the file is corrupted or the path too long: {e.Message}");
                return false;
            }

            return true;
        }
#endif
    }
}
