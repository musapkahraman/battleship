using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Common;
using Colyseus;
using UnityEngine;
using DataChange = Colyseus.Schema.DataChange;

namespace BattleshipGame.Network
{
    public class GameClient : Singleton<GameClient>
    {
        private const string LocalEndpoint = "ws://localhost:2567";
        private const string HerokuEndpoint = "ws://bronzehero.herokuapp.com";
        private bool _initialStateReceived;
        private Room<State> _room;
        public string SessionId => _room?.SessionId;
        public State State => _room?.State;
        public bool Joined => _room != null && _room.Connection.IsOpen;

        private void OnApplicationQuit()
        {
            Leave();
        }

        public event Action ConnectionOpened;
        public event Action JoinedIn;
        public event Action<string> GamePhaseChanged;
        public event Action<State> InitialStateReceived;

        public void Leave()
        {
            _room?.Leave();
            _room = null;
        }

        public void SendPlacement(int[] placement)
        {
            _room.Send("place", placement);
        }

        public void SendTurn(int[] targetIndexes)
        {
            _room.Send("turn", targetIndexes);
        }

        public async void Connect()
        {
            var client = new Client(HerokuEndpoint);
            try
            {
                _room = await client.JoinOrCreate<State>("game");
                ConnectionOpened?.Invoke();
                Debug.Log("Joined successfully!");
                _room.OnStateChange += OnRoomStateChange;
                _room.OnJoin += OnRoomJoin;
                _room.State.players.OnAdd += OnPlayerAdd;
                _room.State.players.OnRemove += OnPlayerRemove;
                _room.State.players.OnChange += OnPlayerChange;

                void OnPlayerAdd(Player player, string key)
                {
                    Debug.Log("player added!");
                    Debug.Log(player); // Here's your `Player` instance
                    Debug.Log(key); // Here's your `Player` key
                }

                void OnPlayerRemove(Player player, string key)
                {
                    Debug.Log("player removed!");
                    Debug.Log(player); // Here's your `Player` instance
                    Debug.Log(key); // Here's your `Player` key
                }

                void OnPlayerChange(Player player, string key)
                {
                    Debug.Log("player moved!");
                    Debug.Log(player); // Here's your `Player` instance
                    Debug.Log(key); // Here's your `Player` key
                }
            }
            catch (Exception exception)
            {
                Debug.Log("Error joining: " + exception.Message);
            }
        }

        private void OnRoomStateChange(State state, bool isFirstState)
        {
            if (isFirstState)
            {
                // First setup of your client state
                Debug.Log(state);
                _initialStateReceived = true;
                InitialStateReceived?.Invoke(state);
            }
            else
            {
                // Further updates on your client state
                Debug.Log(state);
            }
        }

        private void OnRoomJoin()
        {
            Debug.Log("Joined successfully!");
            _room.State.OnChange += OnRoomStateChanged;
            JoinedIn?.Invoke();
        }

        private void OnRoomStateChanged(List<DataChange> changes)
        {
            if (!_initialStateReceived) return;
            foreach (var change in changes.Where(change => change.Field == "phase"))
                GamePhaseChanged?.Invoke((string) change.Value);
        }
    }
}