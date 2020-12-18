using System;
using BattleshipGame.Core;
using BattleshipGame.Schemas;

namespace BattleshipGame.AI
{
    public class LocalClient : IClient
    {
        public event Action<State> FirstRoomStateReceived;
        public event Action<string> GamePhaseChanged;

        public State GetRoomState()
        {
            throw new NotImplementedException();
        }

        public string GetSessionId()
        {
            throw new NotImplementedException();
        }

        public void SendPlacement(int[] placement)
        {
            throw new NotImplementedException();
        }

        public void SendTurn(int[] targetIndexes)
        {
            throw new NotImplementedException();
        }

        public void SendRematch(bool isRematching)
        {
            throw new NotImplementedException();
        }

        public void LeaveRoom()
        {
            throw new NotImplementedException();
        }
    }
}