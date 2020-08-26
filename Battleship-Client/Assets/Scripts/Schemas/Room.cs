using System;

namespace BattleshipGame.Schemas
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
}