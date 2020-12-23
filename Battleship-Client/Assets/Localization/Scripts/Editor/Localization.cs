using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Localization
{
    public class Localization : EditorWindow
    {
        private const string OptionsAssetName = "LocalizationOptions.asset";
        private const string OptionsDirectory = "Assets/Data";
        private const string ManagerAssetName = "LocalizationManager.asset";
        private const string LocalizationDirectory = "Assets/Localization";
        private const string SourceFolder = "Source";
        private const string KeysFolder = "Keys";
        private const string LanguageFolder = "Languages";
        private const string ImportButtonLabel = "Import";
        private const string ExportButtonLabel = "Export";
        private const string LoadButtonLabel = "Load";
        private const string UseButtonLabel = "Use";
        private const string ClearButtonLabel = "Clear";
        private const int ButtonsWidth = 90;
        private const int ButtonsHeight = 25;
        private const int LineHeight = 20;
        private const int MaxWindowHeight = 500;
        private static LocalizationManager _managerAsset;
        private static EditorWindow _window;
        private string _assetFileName;
        private Vector2 _scrollPos = Vector2.zero;
        public Language languageToLoad;
        public LocalizationData localizationData;

        [MenuItem("Tools/" + nameof(Localization))]
        private static void Init()
        {
            _window = GetWindow(typeof(Localization), false, nameof(Localization));
            _window.minSize = new Vector2(2.5f * ButtonsWidth, 1.5f * ButtonsHeight);
            _window.Show();
            CheckLocalizationFolder();
            if (!CheckDirectoryForAssetName(ManagerAssetName, LocalizationDirectory)) return;
            string assetPath = Path.Combine(LocalizationDirectory, ManagerAssetName);
            _managerAsset = AssetDatabase.LoadAssetAtPath<LocalizationManager>(assetPath);
        }

        private void OnGUI()
        {
            if (_window == null) Init();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(0.2f * LineHeight);
                var serializedObject = new SerializedObject(this);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(LineHeight);
                    if (localizationData != null)
                    {
                        float width = _window.position.width - 2 * LineHeight;
                        var height = 0;
                        if (localizationData.items != null)
                            height =
                                Mathf.Clamp(4 * LineHeight + 3 * LineHeight * localizationData.items.Count,
                                    0, MaxWindowHeight) + 2 /*for hiding the redundant vertical scroll bar*/;

                        // ReSharper disable once ConvertToUsingDeclaration
                        using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos,
                            GUILayout.Width(width), GUILayout.Height(height)))
                        {
                            _scrollPos = scrollView.scrollPosition;
                            EditorGUILayout.PropertyField(
                                serializedObject.FindProperty(nameof(localizationData)), true);
                        }

                        GUILayout.Space(LineHeight);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (_assetFileName != null)
                                if (GUILayout.Button(UseButtonLabel, GUILayout.Width(ButtonsWidth),
                                    GUILayout.Height(ButtonsHeight)))
                                    CreateLanguageAsset();

                            GUILayout.FlexibleSpace();
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(ImportButtonLabel, GUILayout.Width(ButtonsWidth),
                            GUILayout.Height(ButtonsHeight)))
                            ReadFromJsonSource();

                        if (GUILayout.Button(ExportButtonLabel, GUILayout.Width(ButtonsWidth),
                            GUILayout.Height(ButtonsHeight)))
                            WriteToJsonFile();

                        if (localizationData != null)
                            if (GUILayout.Button(ClearButtonLabel, GUILayout.Width(ButtonsWidth),
                                GUILayout.Height(ButtonsHeight)))
                            {
                                localizationData = null;
                                languageToLoad = null;
                                _assetFileName = null;
                            }

                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(LineHeight);
                    GUILayout.Label("Drop a language asset here to load.");
                    GUILayout.Space(0.2f * LineHeight);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(languageToLoad)), true);
                    if (languageToLoad != null)
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();

                            GUILayout.Space(LineHeight);
                            if (GUILayout.Button(LoadButtonLabel, GUILayout.Width(ButtonsWidth),
                                GUILayout.Height(ButtonsHeight)))
                            {
                                if (localizationData == null)
                                    localizationData =
                                        new LocalizationData(SystemLanguage.Afrikaans, new List<LocalizationItem>());

                                localizationData.language = languageToLoad.title;
                                localizationData.items = languageToLoad.items;
                            }
                        }
                }

                serializedObject.ApplyModifiedProperties();
                GUILayout.Space(0.2f * LineHeight);
            }
        }

        private void WriteToJsonFile()
        {
            CheckFolder(SourceFolder);
            string filePath = EditorUtility.SaveFilePanel("Export language source file",
                Path.Combine(LocalizationDirectory, SourceFolder),
                localizationData.language.ToString(), "json");
            if (string.IsNullOrEmpty(filePath)) return;
            var localizationJsonData =
                new LocalizationJsonData(localizationData.language, new List<LocalizationJsonItem>());
            foreach (var t in localizationData.items)
                localizationJsonData.items.Add(new LocalizationJsonItem
                {
                    key = t.key.name, value = t.value
                });

            string dataAsJson = JsonUtility.ToJson(localizationJsonData);
            File.WriteAllText(filePath, dataAsJson);
            _assetFileName = Path.GetFileName(filePath);
        }

        private static LocalizationData ConvertToLocalizationData(string dataAsJson)
        {
            var localizationJsonData = JsonUtility.FromJson<LocalizationJsonData>(dataAsJson);
            var data = new LocalizationData(localizationJsonData.language, new List<LocalizationItem>());
            foreach (var item in localizationJsonData.items)
                data.items.Add(new LocalizationItem
                {
                    key = CreateLocalizedTextKey(item.key),
                    value = item.value
                });

            return data;
        }

        internal static Key CreateLocalizedTextKey(string key)
        {
            CheckFolder(KeysFolder);
            string directory = Path.Combine(LocalizationDirectory, KeysFolder);
            string filename = key + ".asset";
            string assetPath = Path.Combine(directory, filename);
            var asset = AssetDatabase.LoadAssetAtPath<Key>(assetPath);
            if (CheckDirectoryForAssetName(key, directory)) return asset;
            asset = CreateInstance<Key>();
            if (!IsValidFilename(assetPath, filename)) return null;
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private void ReadFromJsonSource()
        {
            string filePath = EditorUtility.OpenFilePanel("Import language source file", "Assets", "json");
            if (string.IsNullOrEmpty(filePath)) return;
            string dataAsJson = File.ReadAllText(filePath);
            localizationData = ConvertToLocalizationData(dataAsJson);
            _assetFileName = Path.GetFileName(filePath);
        }

        private void CreateLanguageAsset()
        {
            var assetName = localizationData.language.ToString();
            string languageDirectory = Path.Combine(LocalizationDirectory, LanguageFolder);
            string assetPath = Path.Combine(languageDirectory, assetName + ".asset");
            var asset = CreateInstance<Language>();
            CheckFolder(LanguageFolder);
            if (CheckDirectoryForAssetName(assetName, languageDirectory))
                asset = AssetDatabase.LoadAssetAtPath<Language>(assetPath);
            else
                AssetDatabase.CreateAsset(asset, assetPath);

            asset.title = localizationData.language;
            asset.items = localizationData.items;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            if (_managerAsset == null)
            {
                string managerPath = Path.Combine(LocalizationDirectory, ManagerAssetName);
                _managerAsset = CreateInstance<LocalizationManager>();
                AssetDatabase.CreateAsset(_managerAsset, managerPath);
            }

            if (_managerAsset.options == null)
            {
                var prefAsset = CreateInstance<LocalizationOptions>();
                string prefPath = Path.Combine(OptionsDirectory, OptionsAssetName);
                CheckPreferencesFolder();
                if (CheckDirectoryForAssetName(OptionsAssetName, OptionsDirectory))
                    prefAsset = AssetDatabase.LoadAssetAtPath<LocalizationOptions>(prefPath);
                else
                    AssetDatabase.CreateAsset(prefAsset, prefPath);

                _managerAsset.options = prefAsset;
                EditorUtility.SetDirty(_managerAsset);
                AssetDatabase.SaveAssets();
            }

            if (_managerAsset.AddLanguage(asset))
            {
                EditorUtility.SetDirty(_managerAsset);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = _managerAsset;
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "The language is already in the manager.", "OK");
            }
        }

        private static bool CheckDirectoryForAssetName(string assetName, string directory)
        {
            return AssetDatabase.FindAssets(assetName, new[] {directory})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(foundAssetName => !string.IsNullOrEmpty(foundAssetName))
                .Any(foundAssetName => foundAssetName.Equals(assetName));
        }

        private static bool IsValidFilename(string path, string filename)
        {
            return !string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(filename)) &&
                   filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                   !File.Exists(path);
        }

        private static void CheckPreferencesFolder()
        {
            if (AssetDatabase.IsValidFolder(OptionsDirectory)) return;
            string[] dir = OptionsDirectory.Split('/');
            CreateFolder(dir[0], dir[1]);
        }

        private static void CheckLocalizationFolder()
        {
            if (AssetDatabase.IsValidFolder(LocalizationDirectory)) return;
            string[] dir = LocalizationDirectory.Split('/');
            CreateFolder(dir[0], dir[1]);
        }

        private static void CheckFolder(string folder)
        {
            CheckLocalizationFolder();
            if (!AssetDatabase.IsValidFolder(Path.Combine(LocalizationDirectory, folder)))
                CreateFolder(LocalizationDirectory, folder);
        }

        private static void CreateFolder(string parent, string folder)
        {
            string guid = AssetDatabase.CreateFolder(parent, folder);
            AssetDatabase.GUIDToAssetPath(guid);
        }
    }
}