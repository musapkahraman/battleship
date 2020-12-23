using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BattleshipGame.Localization
{
    [CustomEditor(typeof(Language))]
    public class LanguageEditor : UnityEditor.Editor
    {
        private const int KeyPropertyWidth = 180;
        private const int ColumnSpace = 5;
        private SerializedProperty _keyNameProp;
        private ReorderableList _list;

        private void OnEnable()
        {
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
                        Localization.CreateLocalizedTextKey(_keyNameProp.stringValue.Trim());
                    element.FindPropertyRelative(nameof(LocalizationItem.value)).stringValue =
                        _keyNameProp.stringValue.Trim();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _list.DoLayoutList();
            EditorGUILayout.PropertyField(_keyNameProp);
            serializedObject.ApplyModifiedProperties();
        }
    }
}