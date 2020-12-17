﻿using BattleshipGame.Network;
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
        private NetworkManager _networkManager;

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
                    LeaveMultiplayerGameRoom();
                }

                void OnHardMode()
                {
                    Debug.Log("Hard Mode selected.");
                    LeaveMultiplayerGameRoom();
                }
            }

            void PlayWithFriends()
            {
                if (!NetworkManager.TryGetInstance(out _networkManager)) return;
                _networkManager.ConnectToServer();
                multiplayerButton.SetInteractable(false);
            }
        }

        private void LeaveMultiplayerGameRoom()
        {
            if (!NetworkManager.TryGetInstance(out _networkManager)) return;
            _networkManager.Client.LeaveRoom();
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
            multiplayerButton.SetInteractable(true);
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}