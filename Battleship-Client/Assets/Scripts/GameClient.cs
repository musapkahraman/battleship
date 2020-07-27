using System;
using System.Collections;
using System.Linq;
using Colyseus;
using Colyseus.Schema;

public class GameClient : GenericSingleton<GameClient>
{
    public EventHandler OnConnect;
    public EventHandler OnClose;
    public EventHandler OnJoin;
    public EventHandler<State> OnInitialState;
    public EventHandler<object> OnMessage;
    public EventHandler<string> OnGamePhaseChange;

    private Client _client;
    private Room<State> _room;
    private bool _initialStateReceived;

    private string ClientId => _client?.Id;

    public string SessionId => _room?.SessionId;

    public bool Connected => ClientId != null;

    public State State => _room?.State;

    public bool Joined => _room != null && _room.Connection.IsOpen;

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
        _client.OnOpen += OnOpenHandler;
        _client.OnClose += OnCloseHandler;

        StartCoroutine(ConnectAndListen());
    }

    public void Join()
    {
        _room = _client.Join<State>("game");
        _room.OnReadyToConnect += (sender, e) => StartCoroutine(_room.Connect());
        _room.OnMessage += OnMessageHandler;
        _room.OnJoin += OnJoinHandler;
        _room.OnStateChange += OnRoomStateChangeHandler;
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

    // handlers

    private void OnOpenHandler(object sender, EventArgs e)
    {
        OnConnect?.Invoke(this, e);
    }

    private void OnCloseHandler(object sender, EventArgs e)
    {
        OnClose?.Invoke(this, e);
    }

    private void OnJoinHandler(object sender, EventArgs e)
    {
        _room.State.OnChange += OnStateChangeHandler;

        OnJoin?.Invoke(this, e);
    }

    private void OnRoomStateChangeHandler(object sender, StateChangeEventArgs<State> e)
    {
        if (!e.IsFirstState || _initialStateReceived) return;
        _initialStateReceived = true;
        OnInitialState?.Invoke(this, e.State);
    }

    private void OnStateChangeHandler(object sender, OnChangeEventArgs e)
    {
        if (!_initialStateReceived) return;

        foreach (var change in e.Changes.Where(change => change.Field == "phase"))
        {
            OnGamePhaseChange?.Invoke(this, (string) change.Value);
        }
    }

    private void OnMessageHandler(object sender, MessageEventArgs e)
    {
        var message = e.Message;

        OnMessage?.Invoke(this, message);
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