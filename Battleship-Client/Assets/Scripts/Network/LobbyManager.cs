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
        [SerializeField] private TMP_Text message;
        [SerializeField] private GameObject popUpPrefab;
        private string _cachedRoomId = string.Empty;
        private bool _cachedRoomIdIsNotValid;
        private NetworkClient _client;
        private bool _isJoinLocked;

        private void Start()
        {
            if (NetworkManager.TryGetInstance(out var connectionManager))
            {
                connectionManager.ConnectToServer();
                _client = connectionManager.Client;
                _client.RoomsChanged += PopulateRoomList;
                if (_client.SessionId != null)
                {
                    _client.RefreshRoomsList();
                    _isJoinLocked = true;
                    createButton.interactable = false;
                    message.text = "Waiting for another player to join.";
                }

                joinButton.interactable = false;
                joinButton.onClick.AddListener(JoinGame);
                createButton.onClick.AddListener(CreateGame);
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
                    _isJoinLocked = true;
                    createButton.interactable = false;
                    joinButton.interactable = false;
                    message.text = "Waiting for another player to join.";
                }
            }
        }

        private void OnDestroy()
        {
            if (_client != null)
                _client.RoomsChanged -= PopulateRoomList;
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

        public void SetRoomId(string roomId)
        {
            if (_isJoinLocked) return;
            _cachedRoomId = roomId;
            _cachedRoomIdIsNotValid = false;
            joinButton.interactable = true;
        }

        private PopUpCanvas BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpCanvas>();
        }
    }
}