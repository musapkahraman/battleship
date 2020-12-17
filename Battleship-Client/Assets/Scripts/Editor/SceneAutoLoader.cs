using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Editor
{
    /// <summary>
    ///     Scene auto loader.
    /// </summary>
    /// <description>
    ///     This class adds a File > Scene Autoload menu containing options to select
    ///     a "main scene" enable it to be auto-loaded when the user presses play
    ///     in the editor. When enabled, the selected scene will be loaded on play,
    ///     then the original scene will be reloaded on stop.
    /// </description>
    [InitializeOnLoad]
    internal static class SceneAutoLoader
    {
        // Properties are remembered as editor preferences.
        private const string EditorPrefLoadMainOnPlay = "SceneAutoLoader.LoadMainOnPlay";
        private const string EditorPrefMainScene = "SceneAutoLoader.MainScene";
        private const string EditorPrefPreviousScene = "SceneAutoLoader.PreviousScene";

        // Static constructor binds a playmode-changed callback.
        // [InitializeOnLoad] above makes sure this gets executed.
        static SceneAutoLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static bool LoadMainOnPlay
        {
            get => EditorPrefs.GetBool(EditorPrefLoadMainOnPlay, false);
            set => EditorPrefs.SetBool(EditorPrefLoadMainOnPlay, value);
        }

        private static string MainScene
        {
            get => EditorPrefs.GetString(EditorPrefMainScene, "Main.unity");
            set => EditorPrefs.SetString(EditorPrefMainScene, value);
        }

        private static string PreviousScene
        {
            get => EditorPrefs.GetString(EditorPrefPreviousScene, SceneManager.GetActiveScene().path);
            set => EditorPrefs.SetString(EditorPrefPreviousScene, value);
        }

        // Menu items to select the main scene and control whether or not to load it.
        [MenuItem("Scenes/Scene Autoload/Select Main Scene...")]
        private static void SelectMainScene()
        {
            string mainScene = EditorUtility.OpenFilePanel("Select Main Scene", Application.dataPath, "unity");
            mainScene =
                mainScene.Replace(Application.dataPath, "Assets"); //project relative instead of absolute path
            if (string.IsNullOrEmpty(mainScene)) return;
            MainScene = mainScene;
            LoadMainOnPlay = true;
        }

        [MenuItem("Scenes/Scene Autoload/Load Main On Play", true)]
        private static bool ShowLoadMainOnPlay()
        {
            return !LoadMainOnPlay;
        }

        [MenuItem("Scenes/Scene Autoload/Load Main On Play")]
        private static void EnableLoadMainOnPlay()
        {
            LoadMainOnPlay = true;
        }

        [MenuItem("Scenes/Scene Autoload/Do not Load Main On Play", true)]
        private static bool ShowDoNotLoadMainOnPlay()
        {
            return LoadMainOnPlay;
        }

        [MenuItem("Scenes/Scene Autoload/Do not Load Main On Play")]
        private static void DisableLoadMainOnPlay()
        {
            LoadMainOnPlay = false;
        }

        // Play mode change callback handles the scene load/reload.
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!LoadMainOnPlay) return;

            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed play -- autoload main scene.
                PreviousScene = SceneManager.GetActiveScene().path;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    try
                    {
                        EditorSceneManager.OpenScene(MainScene);
                    }
                    catch
                    {
                        Debug.LogError($"error: scene not found: {MainScene}");
                        EditorApplication.isPlaying = false;
                    }
                else
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
            }

            // isPlaying check required because cannot OpenScene while playing
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
            // User pressed stop -- reload previous scene.
            try
            {
                EditorSceneManager.OpenScene(PreviousScene);
            }
            catch
            {
                Debug.LogError($"error: scene not found: {PreviousScene}");
            }
        }
    }
}