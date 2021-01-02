using System.Collections.Generic;
using BattleshipGame.Localization;
using BattleshipGame.Network;
using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Managers
{
    public class LobbyManager : MonoBehaviour, IRoomClickListener
    {
        [SerializeField] private RoomListManager roomList;
        [SerializeField] private Key statusWaitingJoin;
        [SerializeField] private RoomDialog newRoomDialog;
        [SerializeField] private RoomDialog joinRoomDialog;
        [SerializeField] private ButtonController mainMenuButton;
        [SerializeField] private ButtonController newGameButton;
        [SerializeField] private ButtonController joinButton;
        [SerializeField] private ButtonController leaveButton;
        private string _cachedRoomId = string.Empty;
        private bool _cachedRoomIdIsNotValid;
        private GameManager _gameManager;
        private NetworkClient _networkClient;

        private void Start()
        {
            if (GameManager.TryGetInstance(out _gameManager) && _gameManager.Client is NetworkClient client)
            {
                _networkClient = client;
                _networkClient.RoomsChanged += PopulateRoomList;

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
                newRoomDialog.Show(true, (gameName, password) =>
                {
                    _networkClient.CreateRoom(gameName, password);
                    WaitForOpponent();
                    joinButton.SetInteractable(false);
                });
            }

            void JoinGame()
            {
                if (_cachedRoomIdIsNotValid) return;

                if (_networkClient.IsRoomPasswordProtected(_cachedRoomId))
                    joinRoomDialog.Show(false, OnJoin);
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

        public void OnRoomClicked(string roomId)
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
            _gameManager.SetStatusText(statusWaitingJoin);
        }

        private void PopulateRoomList(Dictionary<string, Room> rooms)
        {
            if (!rooms.ContainsKey(_cachedRoomId))
            {
                _cachedRoomIdIsNotValid = true;
                joinButton.SetInteractable(false);
            }

            roomList.PopulateRoomList(rooms, this);
        }
    }
}