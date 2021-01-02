using System;

namespace BattleshipGame.Network
{
    public interface IClient
    {
        event Action<string> GamePhaseChanged;
        State GetRoomState();
        string GetSessionId();
        void Connect(string endPoint = null, Action success = null, Action error = null);
        void SendPlacement(int[] placement);
        void SendTurn(int[] targetIndexes);
        void SendRematch(bool isRematching);
        void LeaveRoom();
    }
}