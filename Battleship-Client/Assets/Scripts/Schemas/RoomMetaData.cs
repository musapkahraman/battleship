using System;

namespace BattleshipGame.Schemas
{
    [Serializable]
    public class RoomMetaData
    {
        public string name;
        public bool requiresPassword;
    }
}