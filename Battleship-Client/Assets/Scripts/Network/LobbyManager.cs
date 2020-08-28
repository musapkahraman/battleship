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
        private NetworkClient _client;
        private string _cachedRoomId = string.Empty;
        private bool _isJoinLocked;
        private bool _cachedRoomIdIsNotValid;

        private void Start()
        {
            if (NetworkManager.TryGetInstance(out var connectionManager))
            {
                connectionManager.ConnectToServer();
                _client = connectionManager.Client;
                _client.RoomsChanged += PopulateRoomList;
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
                BuildPopUp().Show("Join Game", "This game needs a password to join.",
                    "Join", "Cancel", null, null, false, OnJoin);

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