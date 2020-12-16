
import { Client, LobbyRoom } from "colyseus";

import { LobbyState } from "./lobby-state";

export class CustomLobbyRoom extends LobbyRoom {
    async onCreate(options) {
        await super.onCreate(options);
        this.setState(new LobbyState());

        this.onMessage("query", (client, message) => {
            const matched = this.rooms.filter(room => room.metadata.name == message.name);
            client.send("query", matched[0] ? matched[0].roomId : null);
        });

        this.onMessage("roomInfo", (client, message) => {
            const roomInfo = this.rooms.filter(room => room.roomId == message);
            client.send("roomInfo", roomInfo[0] || null);
        });
    }

    onJoin(client: Client, options) {
        super.onJoin(client, options);
        this.state.custom = options.name;
    }

    onLeave(client) {
        super.onLeave(client);
    }
}