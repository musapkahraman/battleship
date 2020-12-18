using System;
using BattleshipGame.Schemas;

namespace BattleshipGame.Core
{
    public interface IClient
    {
        event Action<State> FirstRoomStateReceived;
        event Action<string> GamePhaseChanged;
        State GetRoomState();
        string GetSessionId();
        void SendPlacement(int[] placement);
        void SendTurn(int[] targetIndexes);
        void SendRematch(bool isRematching);
        void LeaveRoom();
    }
}