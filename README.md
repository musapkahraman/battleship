# Battleship

Battleship is a 2D multiplayer game that uses [Colyseus](https://github.com/colyseus) to communicate with the server.

Client code is written in C# to make use of the Unity game engine.

Server code is written in TypeScript.

The game uses a variant of rules for the classic battleship game.

## Usage

### Server

- Install node.js and npm
- Open server folder in your favorite terminal. bash/powershell.. etc.
- Install dependencies:
```
npm install
```
- Start the server:

```
npm start
```

### Client

- Clone this repository.
- Add the folder "Battleship-Client" to your projects in Unity Hub.
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
