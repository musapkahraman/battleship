
import { Client, LobbyRoom } from "colyseus";

import { LobbyState } from "./lobby-state";

export class CustomLobbyRoom extends LobbyRoom {
    async onCreate(options) {
        await super.onCreate(options);
        console.log(options);
        this.setState(new LobbyState());
    }

    onJoin(client: Client, options) {
        super.onJoin(client, options);
        this.state.custom = options.name;
    }

    onLeave(client) {
        super.onLeave(client);
    }
}