using System;
using BattleshipGame.Core;
using BattleshipGame.Schemas;

namespace BattleshipGame.AI
{
    public class LocalClient : IClient
    {
        private const string PlayerId = "player";
        private const string EnemyId = "enemy";
        private readonly LocalRoom _room;

        public LocalClient()
        {
            _room = new LocalRoom(PlayerId, EnemyId);
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

        public void SendPlacement(int[] placement)
        {
            _room.Place(PlayerId, placement);
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
        }
    }
}