import { Schema, type, MapSchema, ArraySchema, } from "@colyseus/schema";

export class Player extends Schema {
    @type('string')
    sessionId: string;

    @type(['int8'])
    shots: ArraySchema<number>;

    @type(['int8'])
    ships: ArraySchema<number>;

    constructor(sessionId: string, shotsSize, shipsSize) {
        super();
        this.sessionId = sessionId;
        this.reset(shotsSize, shipsSize);
    }

    reset(shotsSize, shipsSize) {
        this.shots = new ArraySchema<number>(...new Array(shotsSize).fill(-1));
        this.ships = new ArraySchema<number>(...new Array(shipsSize).fill(-1));
    }
}

export class State extends Schema {
    @type({ map: Player })
    players: MapSchema<Player> = new MapSchema<Player>();

    @type('string')
    phase: string = 'waiting';

    @type('string')
    playerTurn: string;

    @type('string')
    winningPlayer: string;

    @type('int8')
    currentTurn: number = 1;
}