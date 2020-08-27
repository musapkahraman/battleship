import {Room, Client} from "colyseus";

import {State, Player} from './game-state';

export class GameRoom extends Room<State> {
    rematchCount: any = {};
    maxClients: number = 2;
    password: string;
    name: string;
    gridSize: number = 9;
    startingFleetHealth: number = 19;
    placements: Array<Array<number>>;
    playerHealth: Array<number>;
    playersPlaced: number = 0;
    playerCount: number = 0;

    onCreate(options) {
        console.log(options);
        if(options.password){
            this.password = options.password;
            this.name = options.name;
            // this.setPrivate();
        }
        this.reset(false);
        this.setMetadata({name: options.name || this.roomId, requiresPassword: !!this.password});
        this.onMessage("place", (client, message) => this.playerPlace(client, message));
        this.onMessage("turn", (client, message) => this.playerTurn(client, message));
        this.onMessage("rematch", (client, message) => this.rematch(client, message));
        console.log('room created!');
    }

    onJoin(client: Client) {
        console.log('client joined!', client.sessionId);

        let player: Player =  new Player();
        player.sessionId = client.sessionId;
        player.seat = this.playerCount + 1;

        this.state.players[client.sessionId] = player;
        this.playerCount++;

        if(this.playerCount == 2) {
            this.state.phase = 'place';
            this.lock();
        }
    }

    onAuth(client: Client, options: any) {
        if(!this.password) {
            return true
        };

        if(!options.password) {
            throw new Error("This room requires a password!")
        };

        if (this.password === options.password){
            return true;
        }
        throw new Error("Invalid Password!");
    }

    onLeave(client: Client) {
        console.log('client left!', client.sessionId);
        
        delete this.state.players[client.sessionId];
        this.playerCount--;
        this.state.phase = 'waiting';
        this.unlock();
    }

    playerPlace(client: Client, message: any){
        let player: Player = this.state.players[client.sessionId];
        this.placements[player.seat - 1] = message;
        this.playersPlaced++;

        if (this.playersPlaced == 2) {
            this.state.phase = 'battle';
        }
    }

    playerTurn(client: Client, message: any){
        let player: Player = this.state.players[client.sessionId];

        if (this.state.playerTurn != player.seat) return;

        let targetIndexes: number[] = message;

        if(targetIndexes.length != 3) return;

        let shots = player.seat == 1 ? this.state.player1Shots : this.state.player2Shots;
        let targetShips = player.seat == 1 ? this.state.player2Ships : this.state.player1Ships;
        let targetPlayerIndex = player.seat == 1 ? 1 : 0;
        let targetedPlacement = this.placements[targetPlayerIndex];

        for (const targetIndex of targetIndexes) {
            if(shots[targetIndex] == -1){
                shots[targetIndex] = this.state.currentTurn;
                if(targetedPlacement[targetIndex] >= 0){
                    this.playerHealth[targetPlayerIndex]--;
                    switch(targetedPlacement[targetIndex]){
                        case 0: // Admiral
                            this.updateShips(targetShips, 0, 5, this.state.currentTurn);
                            break;
                        case 1: // VTrio
                            this.updateShips(targetShips, 5, 8, this.state.currentTurn);
                            break;
                        case 2: // HTrio
                            this.updateShips(targetShips, 8, 11, this.state.currentTurn);
                            break;
                        case 3: // VDuo
                            this.updateShips(targetShips, 11, 13, this.state.currentTurn);
                            break;
                        case 4: // HDuo
                            this.updateShips(targetShips, 13, 15, this.state.currentTurn);
                            break;
                        case 5: // S
                        case 6: // S
                        case 7: // S
                        case 8: // S
                            this.updateShips(targetShips, 15, 19, this.state.currentTurn);
                            break;
                    }
                }
            }
        }

        if(this.playerHealth[targetPlayerIndex] <= 0){
            this.state.winningPlayer = player.seat;
            this.state.phase = 'result';
        } else {
            if( this.state.playerTurn === 1){
                this.state.playerTurn = 2;
            } else {
                this.state.playerTurn = 1;
                this.state.currentTurn++;
            }
        }
    }

    onDispose(){
        console.log('room disposed!');
    }

    rematch(client: Client, message: Boolean){
        if(!message){
            return this.state.phase ="leave";
        }
        this.rematchCount[client.sessionId] = message;
        if(Object.keys(this.rematchCount).length == 2){
            this.reset(true);
        }
    }

    reset(rematch) {
        this.rematchCount = {};
        this.playerHealth = new Array<number>();
        this.playerHealth[0] = this.startingFleetHealth;
        this.playerHealth[1] = this.startingFleetHealth;

        this.placements = new Array<Array<number>>();
        this.placements[0] = new Array<number>();
        this.placements[1] = new Array<number>();

        let cellCount = this.gridSize * this.gridSize;
        let state = new State();

        state.phase = rematch ? 'place': 'waiting';
        state.playerTurn = 1;
        state.winningPlayer = -1;
        if(rematch){
            state.players = this.state.players;
        }

        for (let i = 0; i < cellCount; i++) {
            this.placements[0][i] = -1;        
            this.placements[1][i] = -1;
            
            state.player1Shots[i] = -1
            state.player2Shots[i] = -1
        }

        for (let i = 0; i < this.startingFleetHealth; i++){
            state.player1Ships[i] = -1;
            state.player2Ships[i] = -1;
        }

        this.setState(state);
        this.playersPlaced = 0;
        this.state.currentTurn = 1;
    }

    updateShips(arr: number[], s:number, e: number, t: number){
        for(let i = s; i < e; i++){
            if (arr[i] == -1){
                arr[i] = t;
                break;
            }
        }
    }
}