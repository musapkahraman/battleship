import {Schema, type, MapSchema, ArraySchema} from "@colyseus/schema";

export class Player extends Schema {
    @type('int16')
    seat: number;

    @type('string')
    sessionId: string;
}

export class State extends Schema {
    @type({map: Player})
    players: MapSchema<Player> = new MapSchema<Player>();

    @type('string')
    phase: string = 'waiting';

    @type('int8')
    playerTurn: number = 1;

    @type('int8')
    winningPlayer: number = -1;

    @type('int8')
    currentTurn: number = 1;

    @type(['int8'])
    player1Shots: ArraySchema<number> = new ArraySchema<number>();
    
    @type(['int8'])
    player2Shots: ArraySchema<number> = new ArraySchema<number>();
    
    @type(['int8'])
    player1Ships: ArraySchema<number> = new ArraySchema<number>();
    
    @type(['int8'])
    player2Ships: ArraySchema<number> = new ArraySchema<number>();
}