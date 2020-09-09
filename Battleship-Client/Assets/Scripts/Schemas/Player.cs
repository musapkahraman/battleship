// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.41
// 

using Colyseus.Schema;

namespace BattleshipGame.Schemas
{
    public class Player : Schema
    {
        [Type(0, "string")] public string sessionId = "";

        [Type(1, "array", "int8")] public ArraySchema<int> shots = new ArraySchema<int>();

        [Type(2, "array", "int8")] public ArraySchema<int> ships = new ArraySchema<int>();
    }
}