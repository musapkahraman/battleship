using System;
using System.Collections.Generic;
using BattleshipGame.Schemas;
using BattleshipGame.UI;
using Colyseus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Network
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private RoomListManager roomList;
        private NetworkClient _client;

        private void Start()
        {
            if (NetworkManager.TryGetInstance(out var connectionManager))
            {
                connectionManager.ConnectToServer();
                _client = connectionManager.Client;
                _client.RoomsChanged += PopulateRoomList;
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
            roomList.PopulateRoomList(rooms);
        }
    }
}