using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BattleshipGame.Editor
{
    public static class RestoreScenes
    {
        private const string Directory = "Assets/Data";
        private const string AssetName = "SceneSetup";

        [MenuItem("Scenes/Scene Setup/Save")]
        public static void SaveSceneSetup()
        {
            string assetPath = Path.Combine(Directory, AssetName + ".asset");
            var asset = ScriptableObject.CreateInstance<SceneSetupState>();
            CheckFolder(Directory);

            if (CheckDirectoryForAssetName(AssetName, Directory))
                asset = AssetDatabase.LoadAssetAtPath<SceneSetupState>(assetPath);
            else
                AssetDatabase.CreateAsset(asset, assetPath);

            asset.sceneSetup = EditorSceneManager.GetSceneManagerSetup();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Scenes/Scene Setup/Restore")]
        public static void RestoreSceneSetup()
        {
            if (!CheckDirectoryForAssetName(AssetName, Directory)) return;
            string assetPath = Path.Combine(Directory, AssetName + ".asset");
            var asset = AssetDatabase.LoadAssetAtPath<SceneSetupState>(assetPath);
            EditorSceneManager.RestoreSceneManagerSetup(asset.sceneSetup);
        }

        private static void CheckFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var dir = Directory.Split('/');
            string guid = AssetDatabase.CreateFolder(dir[0], dir[1]);
            AssetDatabase.GUIDToAssetPath(guid);
        }

        private static bool CheckDirectoryForAssetName(string assetName, string directory)
        {
            return AssetDatabase.FindAssets(assetName, new[] {directory})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(foundAssetName => !string.IsNullOrEmpty(foundAssetName))
                .Any(foundAssetName => foundAssetName.Equals(assetName));
        }
    }
}