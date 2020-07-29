using System;
using System.Collections;
using System.Linq;
using Colyseus;
using Colyseus.Schema;

namespace BattleshipGame.Network
{
    public class GameClient : Singleton<GameClient>
    {
        private Client _client;
        private bool _initialStateReceived;
        private Room<State> _room;
        private string ClientId => _client?.Id;
        public string SessionId => _room?.SessionId;
        public bool Connected => ClientId != null;
        public State State => _room?.State;
        public bool Joined => _room != null && _room.Connection.IsOpen;
        public event EventHandler ConnectionClosed;
        public event EventHandler ConnectionOpened;
        public event EventHandler<string> GamePhaseChanged;
        public event EventHandler<State> InitialStateReceived;
        public event EventHandler JoinedIn;
        public event EventHandler<object> MessageReceived;

        private void OnDestroy()
        {
            _client?.Close();
        }

        private void OnApplicationQuit()
        {
            _room?.Leave();
            _client?.Close();
        }

        public void Connect()
        {
            // const string uri = "ws://localhost:2567";
            const string uri = "ws://bronzehero.herokuapp.com";
            _client = new Client(uri);
            _client.OnOpen += OnConnectionOpened;
            _client.OnClose += OnConnectionClosed;

            StartCoroutine(ConnectAndListen());
        }

        public void Join()
        {
            _room = _client.Join<State>("game");
            _room.OnReadyToConnect += (sender, e) => StartCoroutine(_room.Connect());
            _room.OnMessage += OnServerMessageReceived;
            _room.OnJoin += OnJoined;
            _room.OnStateChange += OnRoomStateApplied;
        }

        public void Leave()
        {
            _room?.Leave();
            _room = null;
        }

        public void SendPlacement(int[] placement)
        {
            _room.Send(new {command = "place", placement});
        }

        public void SendTurn(int[] targetIndexes)
        {
            _room.Send(new {command = "turn", targetIndexes});
        }

        private void OnConnectionOpened(object sender, EventArgs e)
        {
            ConnectionOpened?.Invoke(this, e);
        }

        private void OnConnectionClosed(object sender, EventArgs e)
        {
            ConnectionClosed?.Invoke(this, e);
        }

        private void OnJoined(object sender, EventArgs e)
        {
            _room.State.OnChange += OnRoomStateChanged;

            JoinedIn?.Invoke(this, e);
        }

        private void OnRoomStateApplied(object sender, StateChangeEventArgs<State> e)
        {
            if (!e.IsFirstState || _initialStateReceived) return;
            _initialStateReceived = true;
            InitialStateReceived?.Invoke(this, e.State);
        }

        private void OnRoomStateChanged(object sender, OnChangeEventArgs e)
        {
            if (!_initialStateReceived) return;

            foreach (var change in e.Changes.Where(change => change.Field == "phase"))
                GamePhaseChanged?.Invoke(this, (string) change.Value);
        }

        private void OnServerMessageReceived(object sender, MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e.Message);
        }

        private IEnumerator ConnectAndListen()
        {
            yield return StartCoroutine(_client.Connect());

            while (true)
            {
                _client.Recv();
                yield return 0;
            }
        }
    }
}