// adapted from https://docs.unity3d.com/Packages/com.unity.cloud.assets@1.2/manual/get-started-management.html
#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;
using UnityEngine;

namespace AssetInventory
{
    public class CloudAssetManagement
    {
        private const int k_DefaultCancellationTimeout = 5000;

        IOrganization[] m_AvailableOrganizations;

        CancellationTokenSource m_ProjectCancellationTokenSrc = new();
        CancellationTokenSource m_AssetCancellationTokenSrc = new();
        CancellationTokenSource TagGenerationCancellationSource = new();

        public static bool IsBusy => _busyCount > 0;
        private static int _busyCount;

        public IOrganization[] AvailableOrganizations => m_AvailableOrganizations;
        public IOrganization CurrentOrganization { get; private set; }
        public bool IsOrganizationSelected => CurrentOrganization != null;
        public List<IAssetProject> AvailableProjects { get; } = new();
        public List<IProject> AvailableProjectsAlt { get; } = new();
        public IAssetProject CurrentProject { get; private set; }
        public bool IsProjectSelected => CurrentProject != null;
        public List<IAsset> AvailableAssets { get; } = new();
        public IAsset CurrentAsset { get; set; }
        public List<IDataset> Datasets { get; private set; }
        public IDataset CurrentDataset { get; private set; }
        public Dictionary<DatasetId, IEnumerable<IFile>> DatasetFiles { get; } = new();
        public List<IAsset> AssetVersions { get; private set; }
        public List<IAssetCollection> AssetCollections { get; private set; }
        public IAssetCollection CurrentCollection { get; private set; }
        public List<IAsset> CurrentCollectionAssets { get; } = new();
        public string[] ReachableStatuses { get; private set; }

        private VersionQueryBuilder m_CurrentQuery;

        public void Clear()
        {
            m_ProjectCancellationTokenSrc.Cancel();
            m_ProjectCancellationTokenSrc.Dispose();
            m_AssetCancellationTokenSrc.Cancel();
            m_AssetCancellationTokenSrc.Dispose();

            CurrentAsset = null;
            CurrentProject = null;
            CurrentOrganization = null;
        }

        public static void IncBusyCount() => _busyCount++;
        public static void DecBusyCount() => _busyCount--;

        public void SetSelectedOrganization(IOrganization organization)
        {
            CurrentAsset = null;
            CurrentProject = null;
            CurrentOrganization = organization;
        }

        public void SetSelectedProject(IAssetProject project)
        {
            AssetCollections = null;
            CurrentAsset = null;
            CurrentProject = project;
        }

        public void SetSelectedAsset(IAsset asset)
        {
            CurrentAsset = asset;
        }

        public async Task GetOrganizationsAsync()
        {
            m_AvailableOrganizations = null;

            _busyCount++;
            try
            {
                List<IOrganization> organizations = new List<IOrganization>();
                IAsyncEnumerable<IOrganization> organizationsAsyncEnumerable = PlatformServices.OrganizationRepository.ListOrganizationsAsync(Range.All);
                await foreach (IOrganization organization in organizationsAsyncEnumerable)
                {
                    organizations.Add(organization);
                }

                m_AvailableOrganizations = organizations.ToArray();
            }
            catch (OperationCanceledException oe)
            {
                Debug.Log(oe);
            }
            catch (AggregateException e)
            {
                Debug.LogError(e.InnerException);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _busyCount--;
        }

        public async Task<IOrganization> GetOrganizationAsync(OrganizationId orgId)
        {
            _busyCount++;
            try
            {
                IOrganization org = await PlatformServices.OrganizationRepository.GetOrganizationAsync(orgId);

                _busyCount--;
                return org;
            }
            catch (OperationCanceledException oe)
            {
                Debug.Log(oe);
            }
            catch (AggregateException e)
            {
                Debug.LogError(e.InnerException);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _busyCount--;
            return null;
        }

        public async Task<IAsset> CreateAssetAsync(AssetType assetType, string name, string description = null, List<string> tags = null, Dictionary<string, MetadataValue> metadata = null)
        {
            _busyCount++;

            AssetCreation assetCreation = new AssetCreation(name)
            {
                Description = description,
                Type = assetType,
                Tags = tags,
                Metadata = metadata
            };

            CancellationTokenSource cancellationTokenSrc = new CancellationTokenSource(k_DefaultCancellationTimeout);
            try
            {
                IAsset asset = await CurrentProject.CreateAssetAsync(assetCreation, cancellationTokenSrc.Token);

                _busyCount--;
                return asset;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create asset: {e.Message}");
            }

            _busyCount--;
            return null;
        }

        public async Task UpdateAssetAsync(IAsset asset, IAssetUpdate assetUpdate)
        {
            _busyCount++;
            try
            {
                CancellationTokenSource cancellationTokenSrc = new CancellationTokenSource(k_DefaultCancellationTimeout);
                await asset.UpdateAsync(assetUpdate, cancellationTokenSrc.Token);
                await asset.RefreshAsync(cancellationTokenSrc.Token);
            }
            catch (OperationCanceledException oe)
            {
                Debug.Log(oe);
            }
            catch (AggregateException e)
            {
                Debug.LogError(e.InnerException);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _busyCount--;
        }

        public async Task GetProjectsAsync()
        {
            _busyCount++;

            m_ProjectCancellationTokenSrc.Cancel();
            m_ProjectCancellationTokenSrc.Dispose();
            m_ProjectCancellationTokenSrc = new CancellationTokenSource();

            try
            {
                CancellationToken token = m_ProjectCancellationTokenSrc.Token;
                IAsyncEnumerable<IAssetProject> projects = PlatformServices.AssetRepository.ListAssetProjectsAsync(CurrentOrganization.Id, Range.All, token);
                IAsyncEnumerable<IProject> projects2 = CurrentOrganization.ListProjectsAsync(Range.All, token);

                AvailableProjects.Clear();
                AvailableProjectsAlt.Clear();
                CurrentProject = null;

                await foreach (IAssetProject project in projects)
                {
                    AvailableProjects.Add(project);
                }
                await foreach (IProject project in projects2)
                {
                    AvailableProjectsAlt.Add(project);
                }
            }
            catch (OperationCanceledException oe)
            {
                Debug.Log(oe);
            }
            catch (AggregateException e)
            {
                Debug.LogError(e.InnerException);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _busyCount--;
        }

        public async Task<IAssetProject> GetProjectAsync(OrganizationId orgId, ProjectId projectId)
        {
            _busyCount++;

            try
            {
                ProjectDescriptor pd = new ProjectDescriptor(orgId, projectId);
                IAssetProject project = await PlatformServices.AssetRepository.GetAssetProjectAsync(pd, CancellationToken.None);

                _busyCount--;
                return project;
            }
            catch (OperationCanceledException oe)
            {
                Debug.Log(oe);
            }
            catch (AggregateException e)
            {
                Debug.LogError(e.InnerException);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _busyCount--;

            return null;
        }

        public async Task GetProjectAssetsAsync()
        {
            _busyCount++;

            m_AssetCancellationTokenSrc.Cancel();
            m_AssetCancellationTokenSrc.Dispose();
            m_AssetCancellationTokenSrc = new CancellationTokenSource();

            try
            {
                CancellationToken token = m_AssetCancellationTokenSrc.Token;
                IAsyncEnumerable<IAsset> assets = CurrentProject.QueryAssets().ExecuteAsync(token);

                AvailableAssets.Clear();
                CurrentAsset = null;

                await foreach (IAsset asset in assets)
                {
                    AvailableAssets.Add(asset);
                }
            }
            catch (OperationCanceledException oe)
            {
                Debug.Log(oe);
            }
            catch (AggregateException e)
            {
                Debug.LogError(e.InnerException);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            _busyCount--;
        }

        public async Task GetDatasetsAsync()
        {
            Datasets = null;
            if (CurrentAsset == null) return;

            _busyCount++;

            List<IDataset> datasets = new List<IDataset>();
            IAsyncEnumerable<IDataset> asyncList = CurrentAsset.ListDatasetsAsync(Range.All, CancellationToken.None);
            await foreach (IDataset dataset in asyncList)
            {
                datasets.Add(dataset);
            }

            Datasets = datasets;
            DatasetFiles.Clear();

            _busyCount--;
        }

        public async Task<IDataset> CreateDataset(string name, string description = null)
        {
            _busyCount++;
            IDatasetCreation datasetCreation = new DatasetCreation(name)
            {
                Description = description
            };

            try
            {
                IDataset dataset = await CurrentAsset.CreateDatasetAsync(datasetCreation, CancellationToken.None);
                Datasets.Add(dataset);

                _busyCount--;
                return dataset;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create dataset: {e.Message}");
            }

            _busyCount--;
            return null;
        }

        public async Task UpdateDataset(IDatasetUpdate update)
        {
            _busyCount++;

            try
            {
                await CurrentDataset.UpdateAsync(update, CancellationToken.None);
                await CurrentDataset.RefreshAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update dataset: {e.Message}");
            }

            _busyCount--;
        }

        public void SetCurrentDataset(IDataset dataset)
        {
            CurrentDataset = dataset;
        }

        public async Task<IAsset> GetAssetAsync(Asset asset, AssetFile assetFile)
        {
            string projectId = asset.GetRootAsset().SafeName;

            return await PlatformServices.AssetRepository.GetAssetAsync(
                new AssetDescriptor(
                    new ProjectDescriptor(
                        new OrganizationId(asset.OriginalLocationKey), new ProjectId(projectId)),
                    new AssetId(assetFile.Guid), new Unity.Cloud.Common.AssetVersion(assetFile.FileVersion)),
                CancellationToken.None);
        }

        public async Task AddTags(Asset asset, AssetFile assetFile, List<string> tags)
        {
            _busyCount++;

            IAsset cloudAsset = await GetAssetAsync(asset, assetFile);
            if (cloudAsset == null)
            {
                _busyCount--;
                return;
            }
            try
            {
                await cloudAsset.AddTagsAsync(tags, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to add tags: {e.Message}");
            }

            _busyCount--;
        }

        public async Task RemoveTags(Asset asset, AssetFile assetFile, List<string> tags)
        {
            _busyCount++;

            IAsset cloudAsset = await GetAssetAsync(asset, assetFile);
            if (cloudAsset == null)
            {
                _busyCount--;
                return;
            }
            try
            {
                await cloudAsset.RemoveTagsAsync(tags, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to remove tags: {e.Message}");
            }

            _busyCount--;
        }

        public async Task GetFilesAsync(DatasetId datasetId)
        {
            DatasetFiles.Remove(datasetId);

            IDataset dataset = Datasets?.FirstOrDefault(d => d.Descriptor.DatasetId == datasetId);
            if (dataset == null) return;

            _busyCount++;

            DatasetFiles[datasetId] = null;

            List<IFile> files = new List<IFile>();
            IAsyncEnumerable<IFile> fileList = dataset.ListFilesAsync(Range.All, CancellationToken.None);
            await foreach (IFile file in fileList)
            {
                files.Add(file);
            }

            DatasetFiles[datasetId] = files;

            _busyCount--;
        }

        public async Task<List<string>> FetchAssetFromRemote(Asset asset, AssetFile assetFile, string targetFolder)
        {
            _busyCount++;

            List<string> result = new List<string>();

            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            IAsset cloudAsset = await GetAssetAsync(asset, assetFile);
            if (cloudAsset == null)
            {
                _busyCount--;
                return null;
            }
            SetSelectedAsset(cloudAsset);

            await GetDatasetsAsync();
            List<IFile> files = await GetAllFilesAsync();

            List<Task> tasks = new List<Task>();
            CancellationTokenSource cts = new CancellationTokenSource();
            int progressId = MetaProgress.Start("Downloading from Asset Manager Cloud");
            for (int i = 0; i < files.Count; i++)
            {
                IFile file = files[i];
                MetaProgress.Report(progressId, i + 1, files.Count, file.Descriptor.Path);

                string targetFile = Path.Combine(targetFolder, file.Descriptor.Path);
                string targetParentFolder = Path.GetDirectoryName(targetFile);
                if (!Directory.Exists(targetParentFolder)) Directory.CreateDirectory(targetParentFolder);

                // download file, keep folder structure intact
                tasks.Add(DownloadFileAsync(file, targetFile, cts.Token));
                result.Add(targetFile);
            }

            await Task.WhenAll(tasks);
            MetaProgress.Remove(progressId);
            _busyCount--;

            return result;
        }

        public async Task<IFile> UploadFile(IDataset dataset, string filePath, string folderPath = null, string description = null)
        {
            _busyCount++;

            string path = folderPath == null ? Path.GetFileName(filePath) : Path.GetRelativePath(folderPath, filePath);
            FileCreation fileCreation = new FileCreation(path)
            {
                Description = description
            };

            try
            {
                LogProgress progress = new LogProgress();
                progress.enabled = false;

                FileStream fileStream = File.OpenRead(filePath);
                IFile file = await dataset.UploadFileAsync(fileCreation, fileStream, progress, CancellationToken.None);

                _busyCount--;
                return file;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to upload file '{fileCreation.Path}': {e.Message}");
            }

            _busyCount--;
            return null;
        }

        public async Task<bool> UploadFolderAsync(IDataset dataset, string folderPath)
        {
            bool success = true;

            _busyCount++;

            string parentDirectoryPath = Directory.GetParent(folderPath)?.FullName;
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

            List<Task> tasks = new List<Task>();
            foreach (string file in files)
            {
                tasks.Add(UploadFile(dataset, file, parentDirectoryPath));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to upload folder: '{folderPath}': {e.Message}");
                success = false;
            }
            _busyCount--;
            return success;
        }

        public async Task ReplaceFileAsync(IFile file, MemoryStream memoryStream)
        {
            await file.UploadAsync(memoryStream, new LogProgress(), CancellationToken.None);
        }

        private async Task DownloadFileAsyncAlt(IFile file, string targetFile)
        {
            _busyCount++;

            Uri uri = await file.GetDownloadUrlAsync(CancellationToken.None);
            if (!await IOUtils.DownloadFile(uri, targetFile))
            {
                Debug.LogError($"Could not download file: {uri}");
            }

            _busyCount--;
        }

        public async Task DownloadFileAsync(IFile file, string targetFile, CancellationToken cancellationToken)
        {
            _busyCount++;

            try
            {
                await using FileStream destination = File.OpenWrite(targetFile);

                LogProgress progress = new LogProgress();
                progress.enabled = false;

                await file.DownloadAsync(destination, progress, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download asset file: {file.Descriptor.Path}. {e}");
            }

            _busyCount--;
        }

        public async Task UpdateFileAsync(IFile assetFile, IFileUpdate fileUpdate)
        {
            await assetFile.UpdateAsync(fileUpdate, CancellationToken.None);
            await assetFile.RefreshAsync(CancellationToken.None);
        }

        public async Task<IEnumerable<GeneratedTag>> GenerateTagsAsync(IFile file)
        {
            CancelTagGeneration();

            TagGenerationCancellationSource = new CancellationTokenSource();

            try
            {
                return await file.GenerateSuggestedTagsAsync(TagGenerationCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Cancelled tag generation for {file.Descriptor.Path}.");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        private void CancelTagGeneration()
        {
            if (TagGenerationCancellationSource != null)
            {
                TagGenerationCancellationSource.Cancel();
                TagGenerationCancellationSource.Dispose();
            }

            TagGenerationCancellationSource = null;
        }

        public async Task<List<IFile>> GetAllFilesAsync()
        {
            _busyCount++;

            if (DatasetFiles.Count == 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (IDataset dataset in Datasets.Where(ds => ds.IsVisible && ds.Name != "Preview"))
                {
                    tasks.Add(GetFilesAsync(dataset.Descriptor.DatasetId));
                }
                await Task.WhenAll(tasks);
            }
            _busyCount--;

            return DatasetFiles.SelectMany(kvp => kvp.Value).ToList();
        }

        public async Task LinkFile(IDataset dataset, IFile file)
        {
            try
            {
                await dataset.AddExistingFileAsync(file.Descriptor.Path, file.Descriptor.DatasetId, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to link asset file: {file.Descriptor.Path}. {e}");
            }
        }

        public async Task UnlinkFile(IDataset dataset, IFile file)
        {
            try
            {
                await dataset.RemoveFileAsync(file.Descriptor.Path, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to unlink asset file: {file.Descriptor.Path}. {e}");
            }
        }

        public async Task GetReachableStatuses()
        {
            ReachableStatuses = null;
            ReachableStatuses = await CurrentAsset.GetReachableStatusNamesAsync(CancellationToken.None);
        }

        public async Task UpdateStatusAsync(string reachableStatus)
        {
            await CurrentAsset.UpdateStatusAsync(reachableStatus, CancellationToken.None);
            await GetReachableStatuses();
        }

        public async Task<IAssetCollection> GetProjectAssetCollectionAsync(OrganizationId orgId, ProjectId projectId, CollectionPath path)
        {
            _busyCount++;

            ProjectDescriptor pd = new ProjectDescriptor(orgId, projectId);
            CollectionDescriptor cd = new CollectionDescriptor(pd, path);
            IAssetCollection result = await PlatformServices.AssetRepository.GetAssetCollectionAsync(cd, CancellationToken.None);

            _busyCount--;
            return result;
        }

        public async Task ListProjectAssetCollectionsAsync()
        {
            _busyCount++;

            CurrentCollection = null;

            IAsyncEnumerable<IAssetCollection> results = CurrentProject.ListCollectionsAsync(Range.All, CancellationToken.None);
            List<IAssetCollection> collections = new List<IAssetCollection>();
            await foreach (IAssetCollection collection in results)
            {
                collections.Add(collection);
            }

            AssetCollections = collections.OrderBy(c => c.ParentPath.ToString()).ToList();

            _busyCount--;
        }

        public void SetCurrentCollection(IAssetCollection collection)
        {
            if (collection != CurrentCollection)
            {
                CurrentCollection = collection;
            }
        }

        public async Task ListCollectionAssetsAsync()
        {
            _busyCount++;

            CurrentCollectionAssets.Clear();

            AssetSearchFilter searchFilter = new AssetSearchFilter();
            searchFilter.Collections.WhereContains(CurrentCollection.Descriptor.Path);

            IAsyncEnumerable<IAsset> assetList = CurrentProject.QueryAssets().SelectWhereMatchesFilter(searchFilter).ExecuteAsync(CancellationToken.None);
            await foreach (IAsset asset in assetList)
            {
                CurrentCollectionAssets.Add(asset);
            }
            _busyCount--;
        }

        public async Task<IAssetCollection> CreateAssetCollectionAsync(CollectionPath newPath, string description = "Created through Asset Inventory")
        {
            _busyCount++;
            string name = newPath.GetLastComponentOfPath();
            AssetCollectionCreation newCollection = new AssetCollectionCreation(name, description)
            {
                ParentPath = newPath.GetParentPath()
            };

            try
            {
                IAssetCollection col = await CurrentProject.CreateCollectionAsync(newCollection, CancellationToken.None);
                await ListProjectAssetCollectionsAsync();

                _busyCount--;
                return col;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            _busyCount--;
            return null;
        }

        public async Task UpdateProjectAssetCollectionAsync(IAssetCollectionUpdate update)
        {
            _busyCount++;
            await CurrentCollection.UpdateAsync(update, CancellationToken.None);
            _busyCount--;
        }

        public async Task DeleteAssetCollectionAsync(CollectionPath path, bool refresh = false)
        {
            _busyCount++;
            await CurrentProject.DeleteCollectionAsync(path, CancellationToken.None);
            if (refresh) await ListProjectAssetCollectionsAsync();
            _busyCount--;
        }

        public async Task MoveProjectAssetCollectionAsync(CollectionPath newPath, bool refresh = false)
        {
            _busyCount++;
            await CurrentCollection.MoveToNewPathAsync(newPath, CancellationToken.None);
            if (refresh) await ListProjectAssetCollectionsAsync();
            _busyCount--;
        }

        public async Task LinkAssetToCollectionAsync(IAsset asset, bool refresh = false)
        {
            _busyCount++;
            await CurrentCollection.LinkAssetsAsync(new[] {asset}, CancellationToken.None);
            if (refresh) await RefreshCollectionAssets();
            _busyCount--;
        }

        public async Task UnlinkAssetFromCollectionAsync(IAsset asset, bool refresh = false)
        {
            _busyCount++;
            await CurrentCollection.UnlinkAssetsAsync(new[] {asset}, CancellationToken.None);
            if (refresh) await RefreshCollectionAssets();
            _busyCount--;
        }

        private async Task RefreshCollectionAssets()
        {
            _busyCount++;
            CurrentCollectionAssets.Clear();

            AssetSearchFilter searchFilter = new AssetSearchFilter();
            searchFilter.Collections.WhereContains(CurrentCollection.Descriptor.Path);

            IAsyncEnumerable<IAsset> assetList = CurrentProject.QueryAssets().SelectWhereMatchesFilter(searchFilter).ExecuteAsync(CancellationToken.None);
            await foreach (IAsset asset in assetList)
            {
                CurrentCollectionAssets.Add(asset);
            }
            _busyCount--;
        }

        public async Task SearchVersions(string sortingField, SortingOrder sortingOrder)
        {
            m_CurrentQuery = CurrentAsset.QueryVersions().OrderBy(sortingField, sortingOrder);

            await PopulateVersions(m_CurrentQuery);
        }

        private async Task PopulateVersions(VersionQueryBuilder query)
        {
            if (query == null) return;

            IAsyncEnumerable<IAsset> results = query.ExecuteAsync(CancellationToken.None);

            AssetVersions = new List<IAsset>();
            await foreach (IAsset asset in results)
            {
                AssetVersions ??= new List<IAsset>();
                AssetVersions.Add(asset);
            }
        }

        public async Task FreezeVersion(IAsset asset)
        {
            int sequenceNumber = await asset.FreezeAsync("Use case coding example submission.", CancellationToken.None);

            List<Task> tasks = AssetVersions.Select(version => version.RefreshAsync(CancellationToken.None)).ToList();
            await Task.WhenAll(tasks);

            Debug.Log($"Version frozen with sequence number: {sequenceNumber}");
        }

        public async Task CreateVersion(IAsset asset)
        {
            IAsset version = await asset.CreateUnfrozenVersionAsync(CancellationToken.None);
            await PopulateVersions(m_CurrentQuery);

            Debug.Log($"New version created with version: {version.Descriptor.AssetVersion}");
        }

        public async Task<IOrganization> SelectOrgAsync(Asset asset)
        {
            _busyCount++;
            IOrganization org = AvailableOrganizations?.FirstOrDefault(o => o.Id.ToString() == asset.OriginalLocationKey);
            if (org == null)
            {
                org = await GetOrganizationAsync(new OrganizationId(asset.OriginalLocationKey));
            }
            if (org == null)
            {
                Debug.LogError($"Could not find or access remote organization '{asset.OriginalLocation}'");

                _busyCount--;
                return null;
            }
            SetSelectedOrganization(org);

            _busyCount--;
            return org;
        }

        public async Task<IAssetProject> SelectProjectAsync(Asset asset)
        {
            if (CurrentProject != null && CurrentProject.Descriptor.ProjectId.ToString() == asset.GetRootAsset().SafeName) return CurrentProject;

            _busyCount++;

            IAssetProject project = AvailableProjects?.FirstOrDefault(p => p.Descriptor.ProjectId.ToString() == asset.GetRootAsset().SafeName);
            if (project == null)
            {
                project = await GetProjectAsync(new OrganizationId(asset.OriginalLocationKey), new ProjectId(asset.GetRootAsset().SafeName));
            }
            if (project == null)
            {
                Debug.LogError($"Could not find or access remote project '{asset.GetRootAsset().DisplayName}'");

                _busyCount--;
                return null;
            }
            SetSelectedProject(project);

            _busyCount--;
            return project;
        }

        public async Task<IAssetCollection> SelectProjectAssetCollectionAsync(Asset asset)
        {
            _busyCount++;

            string projectId = asset.GetRootAsset().SafeName;
            IAssetCollection collection = AssetCollections?.FirstOrDefault(c => c.Descriptor.Path.ToString() == asset.Location && c.Descriptor.ProjectId.ToString() == projectId);
            if (collection == null)
            {
                collection = await GetProjectAssetCollectionAsync(new OrganizationId(asset.OriginalLocationKey), new ProjectId(projectId), new CollectionPath(asset.Location));
            }
            if (collection == null)
            {
                Debug.LogError($"Could not find remote collection '{asset.Location}'");

                _busyCount--;
                return null;
            }
            SetCurrentCollection(collection);

            _busyCount--;
            return collection;
        }
    }

    class LogProgress : IProgress<HttpProgress>
    {
        public bool enabled = true;

        public void Report(HttpProgress value)
        {
            if (!enabled || !value.UploadProgress.HasValue) return;

            Debug.Log($"Upload progress: {value.UploadProgress * 100} %");
        }
    }
}
#endif
