using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Schemas;
using Colyseus;
using DataChange = Colyseus.Schema.DataChange;

namespace BattleshipGame.Network
{
    public class NetworkClient
    {
        private const string RoomName = "game";
        private const string LobbyName = "lobby";
        private readonly Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
        private Client _client;
        private bool _isFirstRoomStateReceived;
        private Room<LobbyState> _lobby;
        private Room<State> _room;
        public string SessionId => _room?.SessionId;
        public State RoomState => _room?.State;

        public event Action Connected;
        public event Action<string> ConnectionError;
        public event Action<string> GamePhaseChanged;
        public event Action<Dictionary<string, Room>> RoomsChanged;
        public event Action<State> FirstRoomStateReceived;

        public async void Connect(string endPoint)
        {
            if (_lobby != null && _lobby.Connection.IsOpen) return;
            _client = new Client(endPoint);
            try
            {
                _lobby = await _client.JoinOrCreate<LobbyState>(LobbyName);
                Connected?.Invoke();
                RegisterLobbyHandlers();
            }
            catch (Exception exception)
            {
                ConnectionError?.Invoke(exception.Message);
            }
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
                if (room == null)
                    _rooms.Clear();
                else if (_rooms.ContainsKey(room.roomId))
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
            try
            {
                _room = await _client.Create<State>(RoomName,
                    new Dictionary<string, object> {{"name", name}, {"password", password}});
                RegisterRoomHandlers();
            }
            catch (Exception exception)
            {
                ConnectionError?.Invoke(exception.Message);
            }
        }

        public async void JoinRoom(string roomId, string password)
        {
            try
            {
                _room = await _client.JoinById<State>(roomId, new Dictionary<string, object> {{"password", password}});
                RegisterRoomHandlers();
            }
            catch (Exception exception)
            {
                ConnectionError?.Invoke(exception.Message);
            }
        }

        private void RegisterRoomHandlers()
        {
            _room.OnStateChange += OnStateChange;
            _room.State.OnChange += OnRoomStateChange;

            void OnStateChange(State state, bool isFirstState)
            {
                if (!isFirstState) return;
                _isFirstRoomStateReceived = true;
                FirstRoomStateReceived?.Invoke(state);
            }

            void OnRoomStateChange(List<DataChange> changes)
            {
                if (!_isFirstRoomStateReceived) return;
                foreach (var change in changes.Where(change => change.Field == "phase"))
                    GamePhaseChanged?.Invoke((string) change.Value);
            }
        }

        public void LeaveRoom()
        {
            _room?.Leave();
            _room = null;
        }

        public void LeaveLobby()
        {
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

        public void SendRematch(bool isRematching)
        {
            _room.Send("rematch", isRematching);
        }

        public bool IsRoomPasswordProtected(string roomId)
        {
            return _rooms.TryGetValue(roomId, out var room) && room.metadata.requiresPassword;
        }

        public void RefreshRooms()
        {
            RoomsChanged?.Invoke(_rooms);
        }
    }
}