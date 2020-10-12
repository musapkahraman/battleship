using System.Collections.Generic;
using BattleshipGame.Network;
using BattleshipGame.Schemas;
using BattleshipGame.UI;
using UnityEditor;
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
        [SerializeField] private ButtonController quitButton;
        [SerializeField] private ButtonController newGameButton;
        [SerializeField] private ButtonController joinButton;
        [SerializeField] private ButtonController leaveButton;
        [SerializeField] private GameObject popUpPrefab;
        private string _cachedRoomId = string.Empty;
        private bool _cachedRoomIdIsNotValid;
        private NetworkClient _client;
        private NetworkManager _networkManager;

        private void Start()
        {
            if (NetworkManager.TryGetInstance(out _networkManager))
            {
                _networkManager.ConnectToServer();
                _client = _networkManager.Client;
                _client.RoomsChanged += PopulateRoomList;

                quitButton.SetText("Quit");
                newGameButton.SetText("New Game");
                joinButton.SetText("Join");
                leaveButton.SetText("Leave");
                _networkManager.ClearStatusText();

                quitButton.AddListener(Quit);
                newGameButton.AddListener(NewGame);
                joinButton.AddListener(JoinGame);
                leaveButton.AddListener(LeaveGame);

                joinButton.SetInteractable(false);
                leaveButton.SetInteractable(false);

                if (_client.SessionId != null)
                {
                    _client.RefreshRooms();
                    WaitForOpponent();
                }
            }
            else
            {
                SceneManager.LoadScene(0);
            }

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

            void NewGame()
            {
                BuildPopUp().Show("New Game", "Create a new game with a name and a password if you like.",
                    "Create", "Cancel", null, null, true, OnCreate);

                void OnCreate(string gameName, string password)
                {
                    _client.CreateRoom(gameName, password);
                    WaitForOpponent();
                    joinButton.SetInteractable(false);
                }
            }

            void JoinGame()
            {
                if (_cachedRoomIdIsNotValid) return;

                if (_client.IsRoomPasswordProtected(_cachedRoomId))
                    BuildPopUp().Show("Join Game", "This game needs a password to join.",
                        "Join", "Cancel", null, null, false, OnJoin);
                else
                    OnJoin("", "");

                void OnJoin(string gameName, string password)
                {
                    _client.JoinRoom(_cachedRoomId, password);
                }
            }

            void LeaveGame()
            {
                leaveButton.SetInteractable(false);
                newGameButton.SetInteractable(true);
                _client.LeaveRoom();
                _networkManager.ClearStatusText();
            }
        }

        private void OnDestroy()
        {
            if (_client != null)
                _client.RoomsChanged -= PopulateRoomList;
        }

        public void SetRoomId(string roomId)
        {
            if (_client.SessionId != null) return;
            _cachedRoomId = roomId;
            _cachedRoomIdIsNotValid = false;
            joinButton.SetInteractable(true);
        }

        private void WaitForOpponent()
        {
            newGameButton.SetInteractable(false);
            leaveButton.SetInteractable(true);
            _networkManager.SetStatusText("Waiting for another player to join.");
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