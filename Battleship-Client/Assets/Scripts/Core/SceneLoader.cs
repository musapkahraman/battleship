using BattleshipGame.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Core
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private SceneReference masterScene;
        [SerializeField] private SceneReference lobbyScene;
        [SerializeField] private SceneReference placementScene;
        [SerializeField] private SceneReference gameScene;

        protected override void Awake()
        {
            base.Awake();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            if (SceneManager.sceneCount > 1) UnloadAllScenesExcept(masterScene);
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            base.OnDestroy();
        }

        public void GoToLobby()
        {
            LoadSingleSceneAdditive(lobbyScene);
        }

        public void GoToPlacementScene()
        {
            LoadSingleSceneAdditive(placementScene);
        }

        public void GoToGameScene()
        {
            LoadSingleSceneAdditive(gameScene);
        }

        private void LoadSingleSceneAdditive(SceneReference sceneReference)
        {
            PrintLoadedSceneNames();
            UnloadAllScenesExcept(masterScene);
            Debug.Log($"<color=yellow>Loading {sceneReference.ScenePath}</color>");
            SceneManager.LoadScene(sceneReference, LoadSceneMode.Additive);
        }

        private static void UnloadAllScenesExcept(SceneReference sceneReference)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.path.Equals(sceneReference.ScenePath)) continue;
                Debug.Log($"<color=yellow>Unloading {scene.name}</color>");
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"<color=green>{scene.name}</color> is loaded.");
            SceneManager.SetActiveScene(scene);
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"<color=red>{scene.name}</color> is unloaded.");
        }

        private static void PrintLoadedSceneNames()
        {
            Debug.Log("<color=cyan>Scenes already loaded:</color>");
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                Debug.Log($"<color=cyan>{scene.name}</color>");
            }
        }
    }
}