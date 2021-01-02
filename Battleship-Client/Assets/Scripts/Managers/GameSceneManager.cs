using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Managers
{
    public class GameSceneManager : Singleton<GameSceneManager>
    {
        [SerializeField] private SceneReference main;
        [SerializeField] private SceneReference menu;
        [SerializeField] private SceneReference lobby;
        [SerializeField] private SceneReference plan;
        [SerializeField] private SceneReference battle;

        protected override void Awake()
        {
            base.Awake();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SceneManager.sceneLoaded += SetSelfActive;
            if (SceneManager.sceneCount > 1) UnloadAllScenesExcept(main);
            GoToMenu();
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= SetSelfActive;
            base.OnDestroy();
        }

        public void GoToMenu()
        {
            if (!IsAlreadyLoaded(menu))
                LoadSingleSceneAdditive(menu);
        }

        public void GoToLobby()
        {
            if (!IsAlreadyLoaded(lobby))
                LoadSingleSceneAdditive(lobby);
        }

        public void GoToPlanScene()
        {
            if (!IsAlreadyLoaded(plan))
                LoadSingleSceneAdditive(plan);
        }

        public void GoToBattleScene()
        {
            if (!IsAlreadyLoaded(battle))
                LoadSingleSceneAdditive(battle);
        }

        private void LoadSingleSceneAdditive(SceneReference sceneReference)
        {
            UnloadAllScenesExcept(main);
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