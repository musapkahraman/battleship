using BattleshipGame.Network;
using BattleshipGame.UI;
using UnityEditor;
using UnityEngine;

namespace BattleshipGame.Core
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private ButtonController quitButton;
        [SerializeField] private ButtonController playAgainstAIButton;
        [SerializeField] private ButtonController playWithFriendsButton;
        [SerializeField] private GameObject popUpPrefab;
        private NetworkManager _networkManager;

        private void Start()
        {
            quitButton.SetText("Quit");
            playAgainstAIButton.SetText("Play Against AI");
            playWithFriendsButton.SetText("Play With Friends");

            quitButton.AddListener(Quit);
            playAgainstAIButton.AddListener(PlayAgainstAI);
            playWithFriendsButton.AddListener(PlayWithFriends);

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
                BuildPopUp().Show("Play against AI", "Select difficulty",
                    "Cadet", "Admiral", OnEasyMode, OnHardMode);

                void OnEasyMode()
                {
                    Debug.Log("Easy Mode selected.");
                }

                void OnHardMode()
                {
                    Debug.Log("Hard Mode selected.");
                }
            }

            void PlayWithFriends()
            {
                if (!NetworkManager.TryGetInstance(out _networkManager)) return;
                _networkManager.ConnectToServer();
                playWithFriendsButton.SetInteractable(false);
            }
        }

        private void OnEnable()
        {
            if (!NetworkManager.TryGetInstance(out _networkManager)) return;
            _networkManager.Client.ConnectionError += OnConnectionError;
        }

        private void OnDisable()
        {
            if (!NetworkManager.TryGetInstance(out _networkManager)) return;
            _networkManager.Client.ConnectionError -= OnConnectionError;
        }

        private void OnConnectionError(string error)
        {
            playWithFriendsButton.SetInteractable(true);
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}