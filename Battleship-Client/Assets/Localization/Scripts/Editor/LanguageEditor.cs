using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BattleshipGame.Localization
{
    [CustomEditor(typeof(Language))]
    public class LanguageEditor : UnityEditor.Editor
    {
        private const string LocalizationDirectory = "Assets/Localization";
        private const string SourceFolder = "Source";
        private const string KeysFolder = "Keys";
        private const string ImportButtonLabel = "Import";
        private const string ExportButtonLabel = "Export";
        private const int ButtonsWidth = 100;
        private const int ButtonsHeight = 25;
        private const int LineHeight = 20;
        private const int KeyPropertyWidth = 300;
        private const int ColumnSpace = 5;
        private SerializedProperty _keyNameProp;
        private ReorderableList _list;
        private LocalizationData _localizationData;
        private SerializedProperty _systemLanguageProp;

        private void OnEnable()
        {
            if (_localizationData == null)
                _localizationData = new LocalizationData(SystemLanguage.English, new List<LocalizationItem>());

            _systemLanguageProp = serializedObject.FindProperty(nameof(Language.title));
            _keyNameProp = serializedObject.FindProperty(nameof(Language.keyName));
            _list = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(Language.items)),
                true, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Localized Texts"); },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = _list.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, KeyPropertyWidth, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative(nameof(LocalizationItem.key)), GUIContent.none);
                    EditorGUI.PropertyField(
                        new Rect(rect.x + KeyPropertyWidth + ColumnSpace, rect.y,
                            rect.width - KeyPropertyWidth - ColumnSpace, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative(nameof(LocalizationItem.value)), GUIContent.none);
                },
                onRemoveCallback = list =>
                {
                    if (EditorUtility.DisplayDialog("Warning!",
                        "Are you sure you want to delete the localized text?", "Yes", "No"))
                        ReorderableList.defaultBehaviours.DoRemoveButton(list);
                },
                onAddCallback = list =>
                {
                    int index = list.serializedProperty.arraySize;
                    list.serializedProperty.arraySize++;
                    list.index = index;
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    element.FindPropertyRelative(nameof(LocalizationItem.key)).objectReferenceValue =
                        CreateLocalizedTextKey(_keyNameProp.stringValue.Trim());
                    element.FindPropertyRelative(nameof(LocalizationItem.value)).stringValue =
                        _keyNameProp.stringValue.Trim();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var languageToLoad = (Language) serializedObject.targetObject;
            _localizationData.language = languageToLoad.title;
            _localizationData.items = languageToLoad.items;

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(0.2f * LineHeight);
                GUILayout.Label("You can import and export language data from and to a json file.");
                GUILayout.Space(0.2f * LineHeight);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(ImportButtonLabel, GUILayout.Width(ButtonsWidth),
                        GUILayout.Height(ButtonsHeight)))
                        ReadFromJsonSource();

                    if (GUILayout.Button(ExportButtonLabel, GUILayout.Width(ButtonsWidth),
                        GUILayout.Height(ButtonsHeight)))
                        WriteToJsonFile();

                    GUILayout.FlexibleSpace();
                }


                GUILayout.Space(LineHeight);
                GUILayout.Label("Language of this translation.");
                GUILayout.Space(0.2f * LineHeight);
                EditorGUILayout.PropertyField(_systemLanguageProp);
                GUILayout.Space(LineHeight);
                _list.DoLayoutList();
                EditorGUILayout.PropertyField(_keyNameProp);
                GUILayout.Space(0.2f * LineHeight);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void WriteToJsonFile()
        {
            CheckFolder(SourceFolder);
            string filePath = EditorUtility.SaveFilePanel("Export language source file",
                Path.Combine(LocalizationDirectory, SourceFolder),
                _localizationData.language.ToString(), "json");
            if (string.IsNullOrEmpty(filePath)) return;
            var json = new LocalizationJsonData(_localizationData.language, new List<LocalizationJsonItem>());
            if (_localizationData.items == null) _localizationData.items = new List<LocalizationItem>();
            foreach (var item in _localizationData.items)
                json.items.Add(new LocalizationJsonItem
                {
                    key = item.key.name, value = item.value
                });

            string dataAsJson = JsonUtility.ToJson(json, true);
            File.WriteAllText(filePath, dataAsJson);
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

        private void ReadFromJsonSource()
        {
            string filePath = EditorUtility.OpenFilePanel(
                "Import language source file",
                Path.Combine(LocalizationDirectory, SourceFolder),
                "json");

            if (string.IsNullOrEmpty(filePath)) return;

            string dataAsJson = File.ReadAllText(filePath);
            _localizationData = ConvertToLocalizationData(dataAsJson);
            if (target is Language asset)
            {
                asset.title = _localizationData.language;
                asset.items = _localizationData.items;
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
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