using BattleshipGame.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Core
{
    public class GameSceneManager : Singleton<GameSceneManager>
    {
        [SerializeField] private SceneReference masterScene;
        [SerializeField] private SceneReference lobbyScene;
        [SerializeField] private SceneReference planScene;
        [SerializeField] private SceneReference battleScene;

        protected override void Awake()
        {
            base.Awake();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SceneManager.sceneLoaded += SetSelfActive;
            if (SceneManager.sceneCount > 1) UnloadAllScenesExcept(masterScene);
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= SetSelfActive;
            base.OnDestroy();
        }

        public void GoToLobby()
        {
            if (!IsAlreadyLoaded(lobbyScene))
                LoadSingleSceneAdditive(lobbyScene);
        }

        public void GoToPlanScene()
        {
            if (!IsAlreadyLoaded(planScene))
                LoadSingleSceneAdditive(planScene);
        }

        public void GoToBattleScene()
        {
            if (!IsAlreadyLoaded(battleScene))
                LoadSingleSceneAdditive(battleScene);
        }

        private void LoadSingleSceneAdditive(SceneReference sceneReference)
        {
            UnloadAllScenesExcept(masterScene);
            SceneManager.LoadScene(sceneReference, LoadSceneMode.Additive);
        }

        private static bool IsAlreadyLoaded(SceneReference sceneReference)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.path.Equals(sceneReference.ScenePath)) return true;
            }

            return false;
        }

        private static void UnloadAllScenesExcept(SceneReference sceneReference)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.path.Equals(sceneReference.ScenePath)) continue;
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        private static void SetSelfActive(Scene scene, LoadSceneMode mode)
        {
            SceneManager.SetActiveScene(scene);
        }
    }
}