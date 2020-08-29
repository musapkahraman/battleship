using System.Collections.Generic;
using BattleshipGame.Common;
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
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button joinButton;
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

                newGameButton.GetComponentInChildren<TMP_Text>().text = "New Game";
                joinButton.GetComponentInChildren<TMP_Text>().text = "Join";
                leaveButton.GetComponentInChildren<TMP_Text>().text = "Leave";

                newGameButton.onClick.AddListener(NewGame);
                joinButton.onClick.AddListener(JoinGame);
                leaveButton.onClick.AddListener(LeaveGame);

                joinButton.SetInteractable(false);
                leaveButton.SetInteractable(false);

                if (_client.SessionId != null)
                {
                    _client.RefreshRoomsList();
                    WaitForOpponent();
                }
            }
            else
            {
                SceneManager.LoadScene(0);
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
            }
        }

        private void OnDestroy()
        {
            if (_client != null)
                _client.RoomsChanged -= PopulateRoomList;
        }

        public void SetRoomId(string roomId)
        {
            Debug.Log($"Joining is locked: <color=yellow>{_isJoiningLocked}</color>");
            if (_isJoiningLocked) return;
            _cachedRoomId = roomId;
            _cachedRoomIdIsNotValid = false;
            joinButton.SetInteractable(true);
        }

        private void WaitForOpponent()
        {
            _isJoiningLocked = true;
            newGameButton.SetInteractable(false);
            leaveButton.SetInteractable(true);
            message.text = "Waiting for another player to join.";
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

        private PopUpCanvas BuildPopUp()
        {
            return Instantiate(popUpPrefab).GetComponent<PopUpCanvas>();
        }
    }
}