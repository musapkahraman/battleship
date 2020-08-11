import {Room, Client} from "colyseus";

import {State, Player} from './state';

export class GameRoom extends Room<State> {

    gridSize: number = 9;

    startingFleetHealth: number = 19;

    placements: Array<Array<number>>;

    playerHealth: Array<number>;

    playersPlaced: number = 0;

    playerCount: number = 0;

    onInit(option) {
        console.log('room created!');
        this.reset();
    }

    onJoin(client: Client) {
        console.log('client coined!', client.sessionId);

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

    onLeave(client: Client) {
        console.log('client left!', client.sessionId);
        
        delete this.state.players[client.sessionId];
        this.playerCount--;
        this.state.phase = 'waiting';
    }

    onMessage (client, message) {
        console.log("message received", message);

        if (!message) return;

        let player: Player = this.state.players[client.sessionId];

        if (!player) return;

        let command: string = message['command'];

        switch (command) {
            case 'place':
                this.placements[player.seat - 1] = message['placement'];
                this.playersPlaced++;

                if (this.playersPlaced == 2) {
                    this.state.phase = 'battle';
                }
                break;
            case 'turn':
                if (this.state.playerTurn != player.seat) return;

                let targetIndexes: number[] = message['targetIndexes'];

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
                break;
            default:
                console.log('unknown command');
        }
    }

    onDispose(){
        console.log('room disposed!');
    }

    reset() {
        this.playerHealth = new Array<number>();
        this.playerHealth[0] = this.startingFleetHealth;
        this.playerHealth[1] = this.startingFleetHealth;

        this.placements = new Array<Array<number>>();
        this.placements[0] = new Array<number>();
        this.placements[1] = new Array<number>();

        let cellCount = this.gridSize * this.gridSize;
        let state = new State();

        state.phase = 'waiting';
        state.playerTurn = 1;
        state.winningPlayer = -1;

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