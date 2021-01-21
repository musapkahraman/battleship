# Battleship

Battleship is a turn-based multiplayer game that uses [Colyseus (0.13)](https://0-13-x.docs.colyseus.io/) to employ a client-server architecture.

Client code is written in C# to make use of the [Unity](https://unity.com/) game engine.

Server code is written in TypeScript.

You can [play this game](https://muk.itch.io/amiral) on my itch.io page.

## Game

The rules are a bit different from the classic battleship game. Each player has three shots per turn. This makes it harder to guess the coordinates of your opponent's ships.

You can drag and drop ships into a map from the pool next to it. You can also highlight a set of three shots that you fired in previous turns by clicking on a shot. These options help you find where your opponent's ships are placed.

For now, this game has only one mode of the classic battleship game we played on paper when we were children. The world-wide known modes will be implemented in the future.

There are two options to play:

- Playing against AI (Single player)
- Playing with a friend (Multiplayer via remote server)

### Single player

This option has two difficulty levels. "Cadet" is playing with randomly generated shots while "Admiral" is playing with a pattern logic. This pattern logic simply starts with random shots, but once your ships are hit, it starts trying to find patterns. It also implements probability calculations to prioritize the next shot.

### Multiplayer

You need to create a new game in the lobby and wait for another player to join. You can give a name to your game so that your friend can find your game in the list. You can also set a password for your game to block others.

## Usage

### Server

- Install node.js and npm
- Open server folder (Battleship/Battleship-Server) in your favorite terminal (bash, powershell, etc.)
- Install dependencies:

```bash
npm install
```

- Start the server:

```bash
npm start
```

### Client

- Add the client folder (Battleship/Battleship-Client) to your projects in Unity Hub.
- Open the project in Unity 2020.2
- Choose the server type in Assets/Data/Options/NetworkOptions

### WebGL Builds

You will need to edit the HTML file in the build. Add the following line inside the `<head>` element in your index.html

```html
<meta http-equiv="Content-Security-Policy" content="upgrade-insecure-requests">
```

Depending on your domain setup, you might need to create an HTTPS server as well.

## License

[MIT](https://choosealicense.com/licenses/mit/)
