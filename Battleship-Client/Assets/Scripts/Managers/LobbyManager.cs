using System.Collections.Generic;
using BattleshipGame.Core;
using BattleshipGame.Network;
using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BattleshipGame.Core.GameStateContainer.GameState;

namespace BattleshipGame.Managers
{
    public class LobbyManager : MonoBehaviour, IRoomClickListener
    {
        [SerializeField] private RoomListManager roomList;
        [SerializeField] private RoomDialog newRoomDialog;
        [SerializeField] private RoomDialog joinRoomDialog;
        [SerializeField] private ButtonController mainMenuButton;
        [SerializeField] private ButtonController newGameButton;
        [SerializeField] private ButtonController joinButton;
        [SerializeField] private ButtonController leaveButton;
        [SerializeField] private GameStateContainer gameStateContainer;
        private string _cachedRoomId = string.Empty;
        private bool _cachedRoomIdIsNotValid;
        private NetworkClient _networkClient;

        private void Awake()
        {
            if (GameManager.TryGetInstance(out var gameManager) && gameManager.Client is NetworkClient client)
            {
                _networkClient = client;
                _networkClient.RoomsChanged += PopulateRoomList;
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        private void Start()
        {
            gameStateContainer.State = BeginLobby;
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
                gameStateContainer.State = BeginLobby;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) GoToMainMenu();
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

        private void GoToMainMenu()
        {
            gameStateContainer.State = MainMenu;
            GameSceneManager.Instance.GoToMenu();
        }

        private void WaitForOpponent()
        {
            newGameButton.SetInteractable(false);
            leaveButton.SetInteractable(true);
            gameStateContainer.State = WaitingOpponentJoin;
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