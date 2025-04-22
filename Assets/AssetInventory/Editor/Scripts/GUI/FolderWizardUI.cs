using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public sealed class FolderWizardUI : EditorWindow
    {
        private string _folder;
        private bool _calculating;
        private bool _activateUnityPackages = true;
        private bool _activateMediaFolders = true;
        private bool _activateArchives = true;
        private bool _activateDevPackages;
        private bool _unityPackagesAlreadyActive;
        private bool _mediaFoldersAlreadyActive;
        private bool _archivesAlreadyActive;
        private bool _devPackagesAlreadyActive;
        private int _packageCount;
        private int _devPackageCount;
        private int _mediaCount;
        private int _archiveCount;
        private bool _isUnityFolder;

        public static FolderWizardUI ShowWindow()
        {
            FolderWizardUI window = GetWindow<FolderWizardUI>("Folder Wizard");
            window.minSize = new Vector2(738, 500);
            window.maxSize = window.minSize;

            return window;
        }

        public void Init(string folder)
        {
            _folder = folder;
            ParseFolder();
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Folder", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField(_folder + (_isUnityFolder ? " (Unity Project)" : ""));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Folders can be scanned for different file types. Each type uses a different importer that can be activated below and configured subsequently with additional settings.", EditorStyles.wordWrappedLabel);

            EditorGUI.BeginDisabledGroup(_calculating);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            int spacing = 6;
            int cellHeight = 180;
            int cellWidth = 360;

            GUILayout.BeginHorizontal();
            GUILayout.Space(spacing);
            GUILayout.BeginVertical();

            GUILayout.BeginVertical("Unity Packages", "window", GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("- Scans for *.unitypackage files", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Creates a new package with the name of the file", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Automatically links package to Asset Store entries", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Extracts previews from package", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Detected packages: {_packageCount:N0}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_unityPackagesAlreadyActive)
            {
                EditorGUILayout.LabelField("Already Active", UIStyles.centerLabel);
            }
            else
            {
                _activateUnityPackages = EditorGUILayout.ToggleLeft("Activate", _activateUnityPackages, GUILayout.Width(80));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.EndVertical();

            GUILayout.Space(spacing);

            GUILayout.BeginVertical("Media Files", "window", GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("- Scans for image, audio, model or any files", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Creates a new package with the name of the folder", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Creates previews while indexing", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Detected files: {_mediaCount:N0}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_mediaFoldersAlreadyActive)
            {
                EditorGUILayout.LabelField("Already Active", UIStyles.centerLabel);
            }
            else
            {
                _activateMediaFolders = EditorGUILayout.ToggleLeft("Activate", _activateMediaFolders, GUILayout.Width(80));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.EndVertical();
            GUILayout.EndVertical();

            GUILayout.Space(spacing);

            GUILayout.BeginVertical();
            GUILayout.BeginVertical("Archives", "window", GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("- Scans for zip/7z/rar archives", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Creates a new package with the name of the file", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Creates previews while indexing", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Detected archives: {_archiveCount:N0}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_archivesAlreadyActive)
            {
                EditorGUILayout.LabelField("Already Active", UIStyles.centerLabel);
            }
            else
            {
                _activateArchives = EditorGUILayout.ToggleLeft("Activate", _activateArchives, GUILayout.Width(80));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.EndVertical();

            GUILayout.Space(spacing);

            GUILayout.BeginVertical("Dev Packages", "window", GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("- Scans for package.json files", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Creates a new registry package based on it", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Allows importing via direct file reference", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Creates previews while indexing", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Detected packages: {_devPackageCount:N0}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_devPackagesAlreadyActive)
            {
                EditorGUILayout.LabelField("Already Active", UIStyles.centerLabel);
            }
            else
            {
                _activateDevPackages = EditorGUILayout.ToggleLeft("Activate", _activateDevPackages, GUILayout.Width(80));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.EndVertical();
            GUILayout.EndVertical();

            GUILayout.Space(spacing);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.Height(40))) SaveSettings();
            // if (GUILayout.Button("Refresh", GUILayout.Height(40), GUILayout.Width(80))) ParseFolder();
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private void SaveSettings()
        {
            if (_activateUnityPackages && !_unityPackagesAlreadyActive) AI.Config.folders.Add(GetSpec(_folder, 0));
            if (_activateMediaFolders && !_mediaFoldersAlreadyActive) AI.Config.folders.Add(GetSpec(_folder, 1));
            if (_activateArchives && !_archivesAlreadyActive) AI.Config.folders.Add(GetSpec(_folder, 2));
            if (_activateDevPackages && !_devPackagesAlreadyActive) AI.Config.folders.Add(GetSpec(_folder, 3));

            AI.SaveConfig();
            Close();
        }

        private FolderSpec GetSpec(string folder, int type)
        {
            FolderSpec spec = new FolderSpec();
            spec.folderType = type;
            spec.location = folder;
            if (AI.IsRel(folder))
            {
                spec.storeRelative = true;
                spec.relativeKey = AI.GetRelKey(folder);
            }

            // scan for all files if that is a Unity project
            if (type == 1 && _isUnityFolder) spec.scanFor = 1;

            return spec;
        }

        private void ParseFolder()
        {
            _calculating = true;

            // determine media extensions
            List<string> mediaExt = new List<string>();
            mediaExt.AddRange(new[] {"Audio", "Images", "Models"});

            List<string> mediaTypes = new List<string>();
            mediaExt.ForEach(t => mediaTypes.AddRange(AI.TypeGroups[t]));

            string deRel = AI.DeRel(_folder);
            _isUnityFolder = AssetUtils.IsUnityProject(deRel);

            // scan
            string rootPath = _isUnityFolder ? Path.Combine(deRel, "Assets") : deRel;
            string[] files = IOUtils.GetFilesSafe(rootPath, "*.*").ToArray();
            _packageCount = files.Count(f => GetExtension(f) == "unitypackage");
            _archiveCount = files.Count(f => GetExtension(f) == "zip" || GetExtension(f) == "rar" || GetExtension(f) == "7z");
            _devPackageCount = files.Count(f => Path.GetFileName(f).ToLowerInvariant() == "package.json");
            _mediaCount = files.Count(f => mediaTypes.Contains(GetExtension(f)));

            _activateUnityPackages = _packageCount > 0 && !_isUnityFolder;
            _activateMediaFolders = _mediaCount > 0 || _isUnityFolder;
            _activateArchives = _archiveCount > 0 && !_isUnityFolder;

            _unityPackagesAlreadyActive = AI.Config.folders.Count(spec => spec.location == _folder && spec.folderType == 0) > 0;
            _mediaFoldersAlreadyActive = AI.Config.folders.Count(spec => spec.location == _folder && spec.folderType == 1) > 0;
            _archivesAlreadyActive = AI.Config.folders.Count(spec => spec.location == _folder && spec.folderType == 2) > 0;
            _devPackagesAlreadyActive = AI.Config.folders.Count(spec => spec.location == _folder && spec.folderType == 3) > 0;

            _calculating = false;
        }

        private string GetExtension(string fileName)
        {
            return IOUtils.GetExtensionWithoutDot(fileName).ToLowerInvariant();
        }
    }
}