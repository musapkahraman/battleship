import express from 'express';
import { createServer } from 'http';
import { Server } from 'colyseus';
import { monitor } from '@colyseus/monitor';
import { GameRoom } from "./src/game-room";

const port = Number(process.env.PORT || 2567);
const app = express();

// Create WebSocket Server
const gameServer = new Server({
  server: createServer(app)
});

// Define a room type
gameServer.register("game", GameRoom);

// (optional) attach web monitoring panel
app.use('/colyseus', monitor(gameServer));

gameServer.onShutdown(function(){
  console.log(`game server is going down.`);
});

gameServer.listen(port);

console.log(`Listening on http://localhost:${ port }`);
