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
    ///     a "master scene" enable it to be auto-loaded when the user presses play
    ///     in the editor. When enabled, the selected scene will be loaded on play,
    ///     then the original scene will be reloaded on stop.
    /// </description>
    [InitializeOnLoad]
    internal static class SceneAutoLoader
    {
        // Properties are remembered as editor preferences.
        private const string EditorPrefLoadMasterOnPlay = "SceneAutoLoader.LoadMasterOnPlay";
        private const string EditorPrefMasterScene = "SceneAutoLoader.MasterScene";
        private const string EditorPrefPreviousScene = "SceneAutoLoader.PreviousScene";

        // Static constructor binds a playmode-changed callback.
        // [InitializeOnLoad] above makes sure this gets executed.
        static SceneAutoLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static bool LoadMasterOnPlay
        {
            get => EditorPrefs.GetBool(EditorPrefLoadMasterOnPlay, false);
            set => EditorPrefs.SetBool(EditorPrefLoadMasterOnPlay, value);
        }

        private static string MasterScene
        {
            get => EditorPrefs.GetString(EditorPrefMasterScene, "Master.unity");
            set => EditorPrefs.SetString(EditorPrefMasterScene, value);
        }

        private static string PreviousScene
        {
            get => EditorPrefs.GetString(EditorPrefPreviousScene, SceneManager.GetActiveScene().path);
            set => EditorPrefs.SetString(EditorPrefPreviousScene, value);
        }

        // Menu items to select the "master" scene and control whether or not to load it.
        [MenuItem("Scenes/Scene Autoload/Select Master Scene...")]
        private static void SelectMasterScene()
        {
            string masterScene = EditorUtility.OpenFilePanel("Select Master Scene", Application.dataPath, "unity");
            masterScene =
                masterScene.Replace(Application.dataPath, "Assets"); //project relative instead of absolute path
            if (string.IsNullOrEmpty(masterScene)) return;
            MasterScene = masterScene;
            LoadMasterOnPlay = true;
        }

        [MenuItem("Scenes/Scene Autoload/Load Master On Play", true)]
        private static bool ShowLoadMasterOnPlay()
        {
            return !LoadMasterOnPlay;
        }

        [MenuItem("Scenes/Scene Autoload/Load Master On Play")]
        private static void EnableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = true;
        }

        [MenuItem("Scenes/Scene Autoload/Do not Load Master On Play", true)]
        private static bool ShowDoNotLoadMasterOnPlay()
        {
            return LoadMasterOnPlay;
        }

        [MenuItem("Scenes/Scene Autoload/Do not Load Master On Play")]
        private static void DisableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = false;
        }

        // Play mode change callback handles the scene load/reload.
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!LoadMasterOnPlay) return;

            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed play -- autoload master scene.
                PreviousScene = SceneManager.GetActiveScene().path;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    try
                    {
                        EditorSceneManager.OpenScene(MasterScene);
                    }
                    catch
                    {
                        Debug.LogError($"error: scene not found: {MasterScene}");
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