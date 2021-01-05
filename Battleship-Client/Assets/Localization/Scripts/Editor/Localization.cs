using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Localization
{
    public class Localization : EditorWindow
    {
        private const string OptionsAssetName = "LocalizationOptions";
        private const string OptionsDirectory = "Assets/Data/Options";
        private const string ManagerAssetName = "Manager";
        private const string LocalizationDirectory = "Assets/Localization";
        private const string SourceFolder = "Source";
        private const string KeysFolder = "Keys";
        private const string LanguageFolder = "Languages";
        private static LocalizationManager _manager;
        private static LocalizationData _localizationData;

        [MenuItem("Tools/Localization/Import Translation")]
        private static void Init()
        {
            ReadFromJsonSource();
            CreateLanguageAsset();
        }

        private static void ReadFromJsonSource()
        {
            string filePath = EditorUtility.OpenFilePanel(
                "Import language source file",
                Path.Combine(LocalizationDirectory, SourceFolder),
                "json");
            if (string.IsNullOrEmpty(filePath)) return;
            string dataAsJson = File.ReadAllText(filePath);
            _localizationData = ConvertToLocalizationData(dataAsJson);
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

        private static Key CreateLocalizedTextKey(string key)
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

        private static void CreateLanguageAsset()
        {
            var assetName = _localizationData.language.ToString();
            string languageDirectory = Path.Combine(LocalizationDirectory, LanguageFolder);
            string assetPath = Path.Combine(languageDirectory, assetName + ".asset");
            var language = CreateInstance<Language>();
            CheckFolder(LanguageFolder);
            if (CheckDirectoryForAssetName(assetName, languageDirectory))
                language = AssetDatabase.LoadAssetAtPath<Language>(assetPath);
            else
                AssetDatabase.CreateAsset(language, assetPath);

            language.title = _localizationData.language;
            language.items = _localizationData.items;
            EditorUtility.SetDirty(language);
            AssetDatabase.SaveAssets();
            AddLanguageToManager(language);
        }

        private static void CheckFolder(string folder)
        {
            CheckLocalizationFolder();
            if (!AssetDatabase.IsValidFolder(Path.Combine(LocalizationDirectory, folder)))
                CreateFolder(LocalizationDirectory, folder);
        }

        private static void CheckLocalizationFolder()
        {
            if (AssetDatabase.IsValidFolder(LocalizationDirectory)) return;
            string[] dir = LocalizationDirectory.Split('/');
            CreateFolder(dir[0], dir[1]);
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

        private static void AddLanguageToManager(Language language)
        {
            string managerAssetPath = Path.Combine(LocalizationDirectory, ManagerAssetName + ".asset");
            if (CheckDirectoryForAssetName(ManagerAssetName, LocalizationDirectory))
            {
                _manager = AssetDatabase.LoadAssetAtPath<LocalizationManager>(managerAssetPath);
            }
            else
            {
                _manager = CreateInstance<LocalizationManager>();
                AssetDatabase.CreateAsset(_manager, managerAssetPath);
            }

            if (!_manager.options)
            {
                var prefAsset = CreateInstance<LocalizationOptions>();
                string prefPath = Path.Combine(OptionsDirectory, OptionsAssetName + ".asset");
                CheckPreferencesFolder();
                if (CheckDirectoryForAssetName(OptionsAssetName, OptionsDirectory))
                    prefAsset = AssetDatabase.LoadAssetAtPath<LocalizationOptions>(prefPath);
                else
                    AssetDatabase.CreateAsset(prefAsset, prefPath);

                _manager.options = prefAsset;
                EditorUtility.SetDirty(_manager);
                AssetDatabase.SaveAssets();
            }

            if (_manager.AddLanguage(language))
            {
                EditorUtility.SetDirty(_manager);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = _manager;
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "The language is already in the manager.", "OK");
            }
        }

        private static void CheckPreferencesFolder()
        {
            if (AssetDatabase.IsValidFolder(OptionsDirectory)) return;
            string[] dir = OptionsDirectory.Split('/');
            CreateFolder(dir[0], dir[1]);
        }

        private static void CreateFolder(string parent, string folder)
        {
            string guid = AssetDatabase.CreateFolder(parent, folder);
            AssetDatabase.GUIDToAssetPath(guid);
        }
    }
}