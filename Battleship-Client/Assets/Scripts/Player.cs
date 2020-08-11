// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.41
// 

using Colyseus.Schema;

public class Player : Schema {
	[Type(0, "int16")]
	public short seat = 0;

	[Type(1, "string")]
	public string sessionId = "";
}

