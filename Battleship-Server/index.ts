import express from 'express';
import { createServer } from 'http';
import { Server } from 'colyseus';
import { monitor } from '@colyseus/monitor';
import { CustomLobbyRoom } from "./src/lobby-room";
import { GameRoom } from "./src/game-room";

const port = Number(process.env.PORT || 2567);
const app = express();

// Create WebSocket Server
const gameServer = new Server({
  server: createServer(app)
});

// Lobby
gameServer.define("lobby", CustomLobbyRoom);

// Define a room type
gameServer.define("game", GameRoom).filterBy(['password']).enableRealtimeListing();

// (optional) attach web monitoring panel
app.use('/cmon', monitor());

gameServer.onShutdown(function(){
  console.log(`game server is going down.`);
});

gameServer.listen(port);

console.log(`Listening on http://localhost:${ port }`);
