using System;
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
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private TMP_Text message;
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
                joinButton.onClick.AddListener(() =>
                {
                    if (_cachedRoomIdIsNotValid) return;
                    _client.JoinRoom(_cachedRoomId, passwordField.text);
                });
                createButton.onClick.AddListener(() =>
                {
                    _client.CreateRoom(nameField.text, passwordField.text);
                    _isJoinLocked = true;
                    createButton.interactable = false;
                    joinButton.interactable = false;
                    nameField.enabled = false;
                    passwordField.enabled = false;
                    message.text = "Waiting for another player to join.";
                });
            }
            else
            {
                SceneManager.LoadScene(0);
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
    }
}