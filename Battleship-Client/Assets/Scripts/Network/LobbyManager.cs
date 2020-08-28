using System.Collections.Generic;
using BattleshipGame.Schemas;
using BattleshipGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BattleshipGame.Network
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private RoomListManager roomList;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private TMP_Text message;
        [SerializeField] private GameObject popUpPrefab;
        private string _cachedRoomId = string.Empty;
        private bool _cachedRoomIdIsNotValid;
        private NetworkClient _client;
        private bool _isJoiningLocked;

        private void Start()
        {
            if (NetworkManager.TryGetInstance(out var connectionManager))
            {
                connectionManager.ConnectToServer();
                _client = connectionManager.Client;
                _client.RoomsChanged += PopulateRoomList;
                joinButton.interactable = false;
                leaveButton.interactable = false;
                if (_client.SessionId != null)
                {
                    _client.RefreshRoomsList();
                    WaitForOpponent();
                }

                joinButton.onClick.AddListener(JoinGame);
                createButton.onClick.AddListener(CreateGame);
                leaveButton.onClick.AddListener(LeaveGame);
            }
            else
            {
                SceneManager.LoadScene(0);
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

            void CreateGame()
            {
                BuildPopUp().Show("Create Game", "Write an name and a password for your game.",
                    "Create", "Cancel", null, null, true, OnCreate);

                void OnCreate(string gameName, string password)
                {
                    _client.CreateRoom(gameName, password);
                    WaitForOpponent();
                    joinButton.interactable = false;
                }
            }

            void LeaveGame()
            {
                leaveButton.interactable = false;
                createButton.interactable = true;
                _client.LeaveRoom();
            }
        }

        private void OnDestroy()
        {
            if (_client != null)
                _client.RoomsChanged -= PopulateRoomList;
        }

        public void SetRoomId(string roomId)
        {
            if (_isJoiningLocked) return;
            _cachedRoomId = roomId;
            _cachedRoomIdIsNotValid = false;
            joinButton.interactable = true;
        }

        private void WaitForOpponent()
        {
            _isJoiningLocked = true;
            createButton.interactable = false;
            leaveButton.interactable = true;
            message.text = "Waiting for another player to join.";
        }

        private void PopulateRoomList(Dictionary<string, Room> rooms)
        {
            if (!rooms.ContainsKey(_cachedRoomId))
            {
                _cachedRoomIdIsNotValid = true;
                joinButton.interactable = false;
            }

            roomList.PopulateRoomList(rooms);
        }

        private PopUpCanvas BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpCanvas>();
        }
    }
}