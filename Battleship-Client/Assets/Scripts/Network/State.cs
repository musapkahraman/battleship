// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.41
// 

using Colyseus.Schema;

namespace BattleshipGame.Network
{
    public class State : Schema
    {
        [Type(0, "map", typeof(MapSchema<Player>))]
        public MapSchema<Player> players = new MapSchema<Player>();

        [Type(1, "string")] public string phase = "";

        [Type(2, "string")] public string playerTurn = "";

        [Type(3, "string")] public string winningPlayer = "";

        [Type(4, "int8")] public int currentTurn = 0;
    }
}