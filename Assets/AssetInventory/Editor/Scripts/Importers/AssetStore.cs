using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace AssetInventory
{
    public static class AssetStore
    {
        private const string URL_PURCHASES = "https://packages-v2.unity.com/-/api/purchases";
        private const string URL_TAGS = "https://packages-v2.unity.com/-/api/taggings";
        private const string URL_ASSET_DETAILS = "https://packages-v2.unity.com/-/api/product";
        private const string URL_ASSET_DOWNLOAD = "https://packages-v2.unity.com/-/api/legacy-package-download-info";
        private const string URL_ASSET_UPDATE = "https://packages-v2.unity.com/-/api/legacy-package-update-info";
        private const string URL_TERMS_CHECK = "https://packages-v2.unity.com/-/api/terms/check";
        private const string URL_TERMS_ACCEPT = "https://packages-v2.unity.com/-/api/terms/accept";
        private const string URL_TOKEN_INFO = "https://api.unity.com/v1/oauth2/tokeninfo?access_token=";
        private const string URL_USER_INFO = "https://api.unity.com/v1/users";

        private const int PAGE_SIZE = 100; // more is not supported by Asset Store

        public static Action OnPackageListUpdated;

        private static ListRequest _listRequest;
        private static SearchRequest _searchRequest;
        private static Dictionary<string, PackageInfo> _allPackages;
        private static Dictionary<string, PackageInfo> _projectPackages;
        private static List<string> _failedPackages;
        private static string _currentSearch;
        private static bool _offlineMode;
        private static MethodInfo _getOriginMethod;

        public static bool CancellationRequested { get; set; }

        public static async Task<AssetPurchases> RetrievePurchases()
        {
            AI.CurrentMain = "Phase 1/3: Fetching purchases";
            AI.MainCount = 1;
            AI.MainProgress = 1;
            int progressId = MetaProgress.Start("Fetching purchases");

            string token = CloudProjectSettings.accessToken;
            AssetPurchases result = await AssetUtils.FetchAPIData<AssetPurchases>($"{URL_PURCHASES}?offset=0&limit={PAGE_SIZE}", token);

            // if more results than page size retrieve rest as well and merge
            // Unity's web client can only run on the main thread
            if (result != null && result.total > PAGE_SIZE)
            {
                int pageCount = AssetUtils.GetPageCount(result.total, PAGE_SIZE) - 1;
                AI.MainCount = pageCount + 1;

                for (int i = 1; i <= pageCount; i += AI.Config.maxConcurrentUnityRequests)
                {
                    List<Task<AssetPurchases>> currentBatch = new List<Task<AssetPurchases>>();

                    for (int j = i; j < i + AI.Config.maxConcurrentUnityRequests && j <= pageCount; j++)
                    {
                        int offset = j * PAGE_SIZE;
                        currentBatch.Add(AssetUtils.FetchAPIData<AssetPurchases>($"{URL_PURCHASES}?offset={offset}&limit={PAGE_SIZE}", token));
                    }
                    AssetPurchases[] pageResults = await Task.WhenAll(currentBatch);

                    for (int k = 0; k < pageResults.Length; k++)
                    {
                        AI.MainProgress = i + k + 1;
                        MetaProgress.Report(progressId, i + k + 1, pageCount + 1, string.Empty);
                        if (CancellationRequested) break;

                        if (pageResults[k]?.results != null)
                        {
                            result.results.AddRange(pageResults[k].results);
                        }
                        else
                        {
                            Debug.LogError("Could only retrieve a partial list of asset purchases. Most likely the Unity web API has a hick-up. Try again later.");
                        }
                    }
                }
            }

            AI.CurrentMain = null;
            MetaProgress.Remove(progressId);

            return result;
        }

        public static async Task<AssetDetails> RetrieveAssetDetails(int id, string eTag = null, bool suppressErrors = false)
        {
            string token = CloudProjectSettings.accessToken;
            string newEtag = eTag;
            AssetDetails result = await AssetUtils.FetchAPIData<AssetDetails>($"{URL_ASSET_DETAILS}/{id}", token, eTag, newCacheTag => newEtag = newCacheTag, 1, null, suppressErrors);
            if (result != null) result.ETag = newEtag;

            return result;
        }

        public static async Task<DownloadInfo> RetrieveAssetDownloadInfo(int id, Action<long> responseIssueCodeCallback = null)
        {
            string token = CloudProjectSettings.accessToken;
            DownloadInfoResult result = await AssetUtils.FetchAPIData<DownloadInfoResult>($"{URL_ASSET_DOWNLOAD}/{id}", token, null, null, 1, responseIssueCodeCallback);

            // special handling of "." also in AssetStoreDownloadInfo
            // null values in content itself should never happen, only one known case:
            // https://assetstore.unity.com/packages/templates/tutorials/untitled-125547
            if (result?.result?.download != null)
            {
                if (result.result.download.filename_safe_category_name != null)
                {
                    result.result.download.filename_safe_category_name = result.result.download.filename_safe_category_name.Replace(".", string.Empty);
                }
                else
                {
                    Debug.LogWarning($"Missing category information for asset {id} in Asset Store download info");
                }
                if (result.result.download.filename_safe_package_name != null)
                {
                    result.result.download.filename_safe_package_name = result.result.download.filename_safe_package_name.Replace(".", string.Empty);
                }
                else
                {
                    Debug.LogWarning($"Missing package information for asset {id} in Asset Store download info");
                }
                if (result.result.download.filename_safe_publisher_name != null)
                {
                    result.result.download.filename_safe_publisher_name = result.result.download.filename_safe_publisher_name.Replace(".", string.Empty);
                }
                else
                {
                    Debug.LogWarning($"Missing publisher information for asset {id} in Asset Store download info");
                }
            }

            return result?.result?.download;
        }

        public static void GatherProjectMetadata()
        {
            _listRequest = Client.List(false, true);
            EditorApplication.update += ListProgress;
        }

        private static void ListProgress()
        {
            if (!_listRequest.IsCompleted) return;
            EditorApplication.update -= ListProgress;

            if (_listRequest.Status == StatusCode.Success)
            {
                _projectPackages = _listRequest.Result.ToDictionary(p => p.name);
            }
        }

        public static void GatherAllMetadata()
        {
            _searchRequest = Client.SearchAll(_offlineMode);
            EditorApplication.update += SearchProgress;
        }

        private static void SearchProgress()
        {
            if (!_searchRequest.IsCompleted) return;
            EditorApplication.update -= SearchProgress;

            if (_searchRequest.Status == StatusCode.Success)
            {
                _allPackages = _searchRequest.Result?.ToDictionary(p => p.name);
                OnPackageListUpdated?.Invoke();
            }
            else if (_searchRequest.Status == StatusCode.Failure)
            {
                // fallback to offline mode
                if (!_offlineMode)
                {
                    _offlineMode = true;
                    GatherAllMetadata();
                }
            }
        }

        private static void IncrementalSearchProgress()
        {
            if (!_searchRequest.IsCompleted) return;
            EditorApplication.update -= IncrementalSearchProgress;

            if (_searchRequest.Status == StatusCode.Success && _searchRequest.Result != null)
            {
                if (_allPackages == null) _allPackages = new Dictionary<string, PackageInfo>();
                foreach (PackageInfo packageInfo in _searchRequest.Result)
                {
                    _allPackages[packageInfo.name] = packageInfo;
                }
                OnPackageListUpdated?.Invoke();
            }
            else
            {
                if (_failedPackages == null) _failedPackages = new List<string>();
                _failedPackages.Add(_currentSearch);
            }
            _currentSearch = null;
        }

        public static bool IsMetadataAvailable(bool considerSearch = true)
        {
            FillBufferOnDemand();

            if (considerSearch && _searchRequest != null && !_searchRequest.IsCompleted) return false;
            return _projectPackages != null && (_allPackages != null || (_searchRequest != null && _searchRequest.IsCompleted));
        }

        public static Dictionary<string, PackageInfo> GetPackages()
        {
            return _allPackages;
        }

        public static Dictionary<string, PackageInfo> GetAllPackages()
        {
            Dictionary<string, PackageInfo> result = new Dictionary<string, PackageInfo>();
            if (_allPackages != null) result = result.Concat(_allPackages).ToDictionary(p => p.Key, p => p.Value);

            // merge with _projectPackages if not duplicate
            if (_projectPackages != null)
            {
                foreach (KeyValuePair<string, PackageInfo> pair in _projectPackages)
                {
                    if (!result.ContainsKey(pair.Key)) result[pair.Key] = pair.Value;
                }
            }

            return result;
        }

        public static PackageInfo GetPackageInfo(AssetInfo info)
        {
            return GetPackageInfo(info.SafeName);
        }

        public static PackageInfo GetPackageInfo(string name, bool inProjectOnly = false)
        {
            FillBufferOnDemand();

            PackageInfo result = null;
            if (_projectPackages != null && _projectPackages.TryGetValue(name, out PackageInfo package)) result = package;
            if (inProjectOnly) return result;

            if (result == null && _allPackages != null)
            {
                if (_allPackages.TryGetValue(name, out PackageInfo package2)) result = package2;
            }
            if (result == null && (_searchRequest == null || _searchRequest.IsCompleted) && (_failedPackages == null || !_failedPackages.Contains(name)))
            {
                _currentSearch = name;
                _searchRequest = Client.Search(name);
                EditorApplication.update += IncrementalSearchProgress;
            }

            return result;
        }

        public static Dictionary<string, PackageInfo> GetProjectPackages()
        {
            FillBufferOnDemand();

            return _projectPackages;
        }

        public static void FillBufferOnDemand(bool force = false)
        {
            if ((force || _projectPackages == null) && _listRequest == null) GatherProjectMetadata();
            if ((force || _allPackages == null) && _searchRequest == null) GatherAllMetadata();
        }

        public static bool IsInstalled(AssetInfo info)
        {
            if (info.AssetSource != Asset.Source.RegistryPackage) return false;

            FillBufferOnDemand();

            return IsInstalled(info.SafeName, info.InstalledPackageVersion());
        }

        public static bool IsInstalled(string name, string version)
        {
            PackageInfo result = GetInstalledPackage(name);
            return result != null && result.version == version;
        }

        public static PackageInfo GetInstalledPackage(string name)
        {
            if (_projectPackages != null && _projectPackages.TryGetValue(name, out PackageInfo pInfo)) return pInfo;
            return null;
        }

        public static PackageInfo GetInstalledPackage(AssetInfo info)
        {
            return GetInstalledPackage(info.SafeName);
        }

        public static void OpenInPackageManager(AssetInfo info)
        {
            if (info.ForeignId > 0)
            {
                OpenAssetInPackageManager(info.GetItemLink());
            }
            else if (info.AssetSource == Asset.Source.RegistryPackage)
            {
                OpenPackageInPackageManager(info.SafeName);
            }
        }

        public static void OpenAssetInPackageManager(string url)
        {
#if UNITY_2022_2_OR_NEWER
            Assembly assembly = Assembly.Load("UnityEditor.CoreModule");
            Type asc = assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
            MethodInfo openURL = asc.GetMethod("OpenURL", BindingFlags.NonPublic | BindingFlags.Static);
            openURL?.Invoke(null, new object[] {url});
#elif UNITY_2020_1_OR_NEWER
            Assembly assembly = Assembly.Load("UnityEditor.PackageManagerUIModule");
            Type asc = assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
            MethodInfo openURL = asc.GetMethod("OpenURL", BindingFlags.NonPublic | BindingFlags.Static);
            openURL?.Invoke(null, new object[] {url});
#else
            // loading assembly will fail below 2020
            OpenPackageInPackageManager(url);
#endif
        }

        public static string GetAssetCacheFolder()
        {
#if UNITY_2022_2_OR_NEWER
            Assembly assembly = Assembly.Load("UnityEditor.CoreModule");
            Type asc = assembly.GetType("UnityEditorInternal.AssetStoreCachePathManager");
            MethodInfo openURL = asc.GetMethod("GetConfig", BindingFlags.Public | BindingFlags.Static);
            object result = openURL?.Invoke(null, null);

            Type type = result?.GetType();
            PropertyInfo propertyInfo = type?.GetProperty("path", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo != null)
            {
                string path = (string)propertyInfo.GetValue(result, null);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    return IOUtils.PathCombine(path, AI.ASSET_STORE_FOLDER_NAME);
                }
            }
#endif
            return null;
        }

        public static AssetOrigin GetAssetOrigin(string guid)
        {
#if UNITY_2023_1_OR_NEWER
            if (_getOriginMethod == null)
            {
                Assembly assembly = Assembly.Load("UnityEditor.CoreModule");
                Type asc = assembly.GetType("UnityEditor.AssetDatabase");
                _getOriginMethod = asc.GetMethod("GetAssetOrigin", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Any, new[] {typeof (string)}, null);
            }
            object origin = _getOriginMethod?.Invoke(null, new object[] {guid});
            if (origin != null)
            {
                Type type = origin.GetType();
                int id = (int)type.GetField("productId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(origin)!;
                if (id > 0)
                {
                    AssetOrigin result = new AssetOrigin();
                    result.productId = id;
                    result.assetPath = (string)type.GetField("assetPath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(origin);
                    result.packageName = (string)type.GetField("packageName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(origin);
                    result.packageVersion = (string)type.GetField("packageVersion", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(origin);
                    result.uploadId = (int)type.GetField("uploadId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(origin)!;

                    return result;
                }
            }
#endif
            return null;
        }

        public static void OpenPackageInPackageManager(string id)
        {
            UnityEditor.PackageManager.UI.Window.Open(id);
        }

        public static void ImportPackage(string archivePath, bool interactive, object assetOrigin)
        {
            if (assetOrigin != null)
            {
                Assembly assembly = Assembly.Load("UnityEditor.CoreModule");
                Type asc = assembly.GetType("UnityEditor.AssetDatabase");
                MethodInfo importPackageMethod = asc.GetMethod("ImportPackageWithOrigin", BindingFlags.NonPublic | BindingFlags.Static);
                importPackageMethod?.Invoke(null, new[] {archivePath, assetOrigin, interactive ? 0 : 1});
            }
            else
            {
                AssetDatabase.ImportPackage(archivePath, interactive);
            }
        }
    }
}