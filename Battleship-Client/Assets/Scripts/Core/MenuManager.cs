using BattleshipGame.UI;
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Core
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private ButtonController quitButton;
        [SerializeField] private ButtonController singlePlayerButton;
        [SerializeField] private ButtonController multiplayerButton;
        [SerializeField] private GameObject popUpPrefab;
        private GameManager _gameManager;

        private void Start()
        {
            quitButton.SetText("Quit");
            singlePlayerButton.SetText("Single Player");
            multiplayerButton.SetText("Multiplayer");

            quitButton.AddListener(Quit);
            singlePlayerButton.AddListener(PlayAgainstAI);
            multiplayerButton.AddListener(PlayWithFriends);

            void Quit()
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
                Application.OpenURL(webQuitPage);
#else
                Application.Quit();
#endif
            }

            void PlayAgainstAI()
            {
                BuildPopUp().Show("Single player mode", "Select difficulty",
                    "Cadet", "Admiral", OnEasyMode, OnHardMode);

                void OnEasyMode()
                {
                    Debug.Log("Easy Mode selected.");
                    StartLocalRoom();
                }

                void OnHardMode()
                {
                    Debug.Log("Hard Mode selected.");
                    StartLocalRoom();
                }
            }

            void PlayWithFriends()
            {
                if (!GameManager.TryGetInstance(out _gameManager)) return;
                _gameManager.ConnectToServer(() => multiplayerButton.SetInteractable(true));
                multiplayerButton.SetInteractable(false);
            }
        }

        private void StartLocalRoom()
        {
            if (!GameManager.TryGetInstance(out _gameManager)) return;
            _gameManager.StartLocalClient();
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}