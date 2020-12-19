using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using BattleshipGame.Schemas;
using Colyseus.Schema;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class LocalClient : IClient
    {
        private const string PlayerId = "player";
        private const string EnemyId = "enemy";
        private readonly LocalRoom _room;
        private readonly Enemy _enemy;
        private bool _isFirstRoomStateReceived;

        public LocalClient()
        {
            _room = new LocalRoom(PlayerId, EnemyId);
            RegisterRoomHandlers();
            _enemy = new Enemy(81);
        }

        public event Action<State> FirstRoomStateReceived;
        public event Action<string> GamePhaseChanged;

        public State GetRoomState()
        {
            return _room.State;
        }

        public string GetSessionId()
        {
            return PlayerId;
        }

        public void Connect(string endPoint, Action success = null, Action<string> error = null)
        {
            _room.Start();
        }

        public void SendPlacement(int[] placement)
        {
            _room.Place(PlayerId, placement);
            _room.Place(EnemyId, placement);
        }

        public void SendTurn(int[] targetIndexes)
        {
            _room.Turn(PlayerId, targetIndexes);
        }

        public void SendRematch(bool isRematching)
        {
            _room.Rematch(isRematching);
        }

        public void LeaveRoom()
        {
            GameSceneManager.Instance.GoToMenu();
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
                foreach (var change in changes)
                {
                    Debug.Log($"change: {change.Field} | {change.PreviousValue} -> {change.Value}");
                }

                foreach (var change in changes)
                {
                    switch (change.Field)
                    {
                        case "phase":
                            GamePhaseChanged?.Invoke((string) change.Value);
                            break;
                        case "playerTurn":
                            var player = (string) change.Value;
                            if (player.Equals(EnemyId))
                            {
                                Debug.Log("AI's turn.");
                                _room.Turn(EnemyId, _enemy.GetRandomCells(3));
                            }
                            break;
                    }
                    
                }
            }
        }
    }
}