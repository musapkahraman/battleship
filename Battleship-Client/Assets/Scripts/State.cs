// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.41
// 

using Colyseus.Schema;

public class State : Schema
{
    [Type(4, "int8")] public int currentTurn = 0;

    [Type(1, "string")] public string phase = "";

    [Type(7, "array", "int8")] public ArraySchema<int> player1Ships = new ArraySchema<int>();

    [Type(5, "array", "int8")] public ArraySchema<int> player1Shots = new ArraySchema<int>();

    [Type(8, "array", "int8")] public ArraySchema<int> player2Ships = new ArraySchema<int>();

    [Type(6, "array", "int8")] public ArraySchema<int> player2Shots = new ArraySchema<int>();

    [Type(0, "map", typeof(MapSchema<Player>))]
    public MapSchema<Player> players = new MapSchema<Player>();

    [Type(2, "int8")] public int playerTurn = 0;

    [Type(3, "int8")] public int winningPlayer = 0;
}