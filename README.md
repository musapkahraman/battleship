# Battleship

Battleship is a 2D multiplayer game that uses [Colyseus](https://github.com/colyseus) to communicate with the server.

Client code is written in C# to make use of the Unity game engine.

Server code is written in TypeScript.

### About the game

Battleship game with another variant of rules. 

You need to create a new game in the lobby and wait for another player to join. You can give a name to your game so that your friend can find your game in the list. You can also set a password for your game to block others.

The rules are a bit different from the classic battleship game. Each player has three shots per turn. This makes it harder to guess the coordinates where your opponent's ships are.

You can drag and drop ships into the maps from the pool beside them.  You can also highlight a set of three shots that you fired in previous turns by clicking on a shot. These options help you while guessing where your opponent's ships are.

You can [play](https://muk.itch.io/battleship) this game.

## Usage

### Server

- Install node.js and npm
- Open server folder (Battleship/Battleship-Server) in your favorite terminal (bash, powershell, etc.)
- Install dependencies:
```
npm install
```
- Start the server:

```
npm start
```

### Client

- Add the client folder (Battleship/Battleship-Client) to your projects in Unity Hub.
- Open the project in Unity 2019.4.9f1
- Enable loading the "master" scene on Play button press in the editor using the menu bar: 
```
Scenes -> Scene Autoload -> Load Master On Play
```
The application starts from the "master" scene (Assets/Scenes/Master.unity)
- Choose the server type in the Network Manager script component attached on the NetworkManager gameObject in the master scene.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)
