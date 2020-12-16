import { Schema, type } from "@colyseus/schema";

export class LobbyState extends Schema {
    @type("string") custom: string;
}