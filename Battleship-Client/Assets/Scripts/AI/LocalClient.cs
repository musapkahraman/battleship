using System;
using BattleshipGame.Network;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class LocalClient : MonoBehaviour, IClient
    {
        private const string PlayerId = "player";
        private const string EnemyId = "enemy";
        private Enemy _enemy;
        private bool _isMatchFinished;
        private LocalRoom _room;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            _enemy.enabled = true;
            _room = new LocalRoom(PlayerId, EnemyId);
            _room.State.OnChange += changes =>
            {
                foreach (var change in changes)
                    switch (change.Field)
                    {
                        case RoomState.Phase:
                            var phase = (string) change.Value;
                            GamePhaseChanged?.Invoke(phase);
                            if (phase.Equals(RoomPhase.Result)) _isMatchFinished = true;
                            break;
                        case RoomState.PlayerTurn:
                            if (!_isMatchFinished && EnemyId.Equals((string) change.Value))
                                StartCoroutine(_enemy.GetShots(cells => _room.Turn(EnemyId, cells)));
                            break;
                    }
            };
            _room.State.players[PlayerId].ships.OnChange += (turn, part) => _enemy.UpdatePlayerShips(part, turn);
        }

        private void OnDisable()
        {
            _enemy.enabled = false;
            _room = null;
        }

        public event Action<string> GamePhaseChanged;

        public State GetRoomState()
        {
            return _room.State;
        }

        public string GetSessionId()
        {
            return PlayerId;
        }

        public void Connect(string endPoint, Action success, Action error)
        {
            _room.Start();
        }

        public void SendPlacement(int[] placement)
        {
            _room.Place(PlayerId, placement);
            _room.Place(EnemyId, _enemy.PlaceShipsRandomly());
        }

        public void SendTurn(int[] targetIndexes)
        {
            _room.Turn(PlayerId, targetIndexes);
        }

        public void SendRematch(bool isRematching)
        {
            _enemy.ResetForRematch();
            _room.Rematch(isRematching);
        }

        public void LeaveRoom()
        {
            enabled = false;
        }
    }
}