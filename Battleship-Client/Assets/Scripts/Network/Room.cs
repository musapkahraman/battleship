using System;

namespace BattleshipGame.Network
{
    [Serializable]
    public class Room
    {
        public int clients;
        public string createdAt;
        public int maxClients;
        public RoomMetaData metadata;
        public string name;
        public string processId;
        public string roomId;
    }

    [Serializable]
    public class RoomMetaData
    {
        public string name;
        public bool requiresPassword;
    }
}