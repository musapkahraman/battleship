using System.Collections.Generic;
using BattleshipGame.Network;
using BattleshipGame.Schemas;
using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Core
{
    public class LobbyManager : MonoBehaviour
    {
#pragma warning disable CS0414
        [SerializeField] private string webQuitPage = "about:blank";
#pragma warning restore CS0414
        [SerializeField] private RoomListManager roomList;
        [SerializeField] private ButtonController mainMenuButton;
        [SerializeField] private ButtonController newGameButton;
        [SerializeField] private ButtonController joinButton;
        [SerializeField] private ButtonController leaveButton;
        [SerializeField] private GameObject popUpPrefab;
        private string _cachedRoomId = string.Empty;
        private bool _cachedRoomIdIsNotValid;
        private GameManager _gameManager;
        private NetworkClient _networkClient;

        private void Start()
        {
            if (GameManager.TryGetInstance(out _gameManager))
            {
                _networkClient = (NetworkClient) _gameManager.Client;
                _networkClient.RoomsChanged += PopulateRoomList;

                mainMenuButton.SetText("Main Menu");
                newGameButton.SetText("New Game");
                joinButton.SetText("Join");
                leaveButton.SetText("Leave");
                _gameManager.ClearStatusText();

                mainMenuButton.AddListener(GoToMainMenu);
                newGameButton.AddListener(NewGame);
                joinButton.AddListener(JoinGame);
                leaveButton.AddListener(LeaveGame);

                joinButton.SetInteractable(false);
                leaveButton.SetInteractable(false);

                if (_networkClient.GetSessionId() != null)
                {
                    _networkClient.RefreshRooms();
                    WaitForOpponent();
                }
            }
            else
            {
                SceneManager.LoadScene(0);
            }

            void GoToMainMenu()
            {
                _gameManager.ClearStatusText();
                GameSceneManager.Instance.GoToMenu();
            }

            void NewGame()
            {
                BuildPopUp().Show("New Game", "Create a new game with a name and a password if you like.",
                    "Create", "Cancel", null, null, true, OnCreate);

                void OnCreate(string gameName, string password)
                {
                    _networkClient.CreateRoom(gameName, password);
                    WaitForOpponent();
                    joinButton.SetInteractable(false);
                }
            }

            void JoinGame()
            {
                if (_cachedRoomIdIsNotValid) return;

                if (_networkClient.IsRoomPasswordProtected(_cachedRoomId))
                    BuildPopUp().Show("Join Game", "This game needs a password to join.",
                        "Join", "Cancel", null, null, false, OnJoin);
                else
                    OnJoin("", "");

                void OnJoin(string gameName, string password)
                {
                    _networkClient.JoinRoom(_cachedRoomId, password);
                }
            }

            void LeaveGame()
            {
                leaveButton.SetInteractable(false);
                newGameButton.SetInteractable(true);
                _networkClient.LeaveRoom();
                _gameManager.ClearStatusText();
            }
        }

        private void OnDestroy()
        {
            if (_networkClient != null)
                _networkClient.RoomsChanged -= PopulateRoomList;
        }

        public void SetRoomId(string roomId)
        {
            if (_networkClient.GetSessionId() != null) return;
            _cachedRoomId = roomId;
            _cachedRoomIdIsNotValid = false;
            joinButton.SetInteractable(true);
        }

        private void WaitForOpponent()
        {
            newGameButton.SetInteractable(false);
            leaveButton.SetInteractable(true);
            _gameManager.SetStatusText("Waiting for another player to join.");
        }

        private void PopulateRoomList(Dictionary<string, Room> rooms)
        {
            if (!rooms.ContainsKey(_cachedRoomId))
            {
                _cachedRoomIdIsNotValid = true;
                joinButton.SetInteractable(false);
            }

            roomList.PopulateRoomList(rooms);
        }

        private PopUpWindow BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpWindow>();
        }
    }
}