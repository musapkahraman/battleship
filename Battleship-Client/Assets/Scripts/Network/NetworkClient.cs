using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Schemas;
using Colyseus;
using UnityEngine;
using DataChange = Colyseus.Schema.DataChange;

namespace BattleshipGame.Network
{
    public class NetworkClient
    {
        private const string LocalEndpoint = "ws://localhost:2567";
        private const string OnlineEndpoint = "ws://bronzehero.herokuapp.com";
        private const string RoomName = "game";
        private const string LobbyName = "lobby";
        private readonly Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
        private Client _client;
        private bool _initialStateReceived;
        private Room<LobbyState> _lobby;
        private Room<State> _room;
        public string SessionId => _room?.SessionId;
        public State State => _room?.State;

        public event Action Connected;

        public event Action<Dictionary<string, Room>> RoomsChanged;
        public event Action<string> GamePhaseChanged;
        public event Action<State> InitialStateReceived;

        public async void Connect(NetworkManager.ServerType serverType)
        {
            if (_lobby != null && _lobby.Connection.IsOpen) return;
            string endPoint = serverType == NetworkManager.ServerType.Online ? OnlineEndpoint : LocalEndpoint;
            _client = new Client(endPoint);
            Debug.Log($"<color=#DAA520>Joining in the room: \'{LobbyName}\'</color>");
            _lobby = await _client.JoinOrCreate<LobbyState>(LobbyName);
            Debug.Log($"<color=green>Joined successfully in the room: \'{LobbyName}\'</color>");
            Connected?.Invoke();
            RegisterLobbyHandlers();
        }

        private void RegisterLobbyHandlers()
        {
            _lobby.OnMessage<Room[]>("rooms", message =>
            {
                foreach (var room in message) _rooms.Add(room.roomId, room);
                RoomsChanged?.Invoke(_rooms);
            });

            _lobby.OnMessage<object[]>("+", message => { _lobby.Send("roomInfo", message[0]); });

            _lobby.OnMessage<Room>("roomInfo", room =>
            {
                if (_rooms.ContainsKey(room.roomId))
                    _rooms[room.roomId] = room;
                else
                    _rooms.Add(room.roomId, room);
                RoomsChanged?.Invoke(_rooms);
            });

            _lobby.OnMessage<string>("-", roomId =>
            {
                if (!_rooms.ContainsKey(roomId)) return;
                _rooms.Remove(roomId);
                RoomsChanged?.Invoke(_rooms);
            });
        }

        public async void CreateRoom(string name, string password)
        {
            Debug.Log($"<color=#DAA520>Creating the room: \'{RoomName}\'</color>");
            _room = await _client.Create<State>(RoomName,
                new Dictionary<string, object> {{"name", name}, {"password", password}});
            Debug.Log($"<color=green>Created successfully the room: \'{RoomName}\'</color>");
            RegisterRoomHandlers();
        }

        public async void JoinRoom(string roomId, string password)
        {
            Debug.Log($"<color=#DAA520>Joining in the room: \'{RoomName}\'</color>");
            _room = await _client.JoinById<State>(roomId, new Dictionary<string, object> {{"password", password}});
            Debug.Log($"<color=green>Joined successfully in the room: \'{RoomName}\'</color>");
            RegisterRoomHandlers();
        }

        private void RegisterRoomHandlers()
        {
            _room.OnStateChange += OnStateChange;
            _room.State.OnChange += OnRoomStateChange;
            _room.State.players.OnAdd += OnPlayerAdd;
            _room.State.players.OnRemove += OnPlayerRemove;
            _room.State.players.OnChange += OnPlayerChange;

            void OnStateChange(State state, bool isFirstState)
            {
                if (isFirstState)
                {
                    Debug.Log("Room state is changed for the first time.");
                    _initialStateReceived = true;
                    InitialStateReceived?.Invoke(state);
                }

                LogState(state);
            }

            void OnRoomStateChange(List<DataChange> changes)
            {
                foreach (var dataChange in changes)
                    Debug.Log($"<color=#63B5B5>{dataChange.Field}:</color> " +
                              $"{dataChange.PreviousValue} <color=green>-></color> {dataChange.Value}");

                if (!_initialStateReceived) return;
                foreach (var change in changes.Where(change => change.Field == "phase"))
                    GamePhaseChanged?.Invoke((string) change.Value);
            }

            void OnPlayerAdd(Player player, string key)
            {
                Debug.Log($"player added: <color=#63B5B5>{key}</color>");
            }

            void OnPlayerRemove(Player player, string key)
            {
                Debug.Log($"player removed: <color=#63B5B5>{key}</color>");
            }

            void OnPlayerChange(Player player, string key)
            {
                Debug.Log($"player moved: <color=#63B5B5>{key}</color>");
            }
        }

        private static void LogState(State state)
        {
            Debug.Log($"<color=#63B5B5>Room state is changed.</color> Phase: {state.phase}" +
                      $" | Player count: {state.players.Items.Count} | Current turn: {state.currentTurn}" +
                      $" | Player turn: {state.playerTurn} | Winning player: {state.winningPlayer}");

            // foreach (var item in state.player1Shots.Items) Debug.Log($"Player1 shot: [{item.Key}, {item.Value}]");
            //
            // foreach (var item in state.player2Shots.Items) Debug.Log($"Player2 shot: [{item.Key}, {item.Value}]");
            //
            // foreach (var item in state.player1Ships.Items) Debug.Log($"Player1 ship: [{item.Key}, {item.Value}]");
            //
            // foreach (var item in state.player2Ships.Items) Debug.Log($"Player2 ship: [{item.Key}, {item.Value}]");
        }

        public void Leave()
        {
            _room?.Leave();
            _room = null;
            _lobby?.Leave();
            _lobby = null;
        }

        public void SendPlacement(int[] placement)
        {
            _room.Send("place", placement);
        }

        public void SendTurn(int[] targetIndexes)
        {
            _room.Send("turn", targetIndexes);
        }
    }
}