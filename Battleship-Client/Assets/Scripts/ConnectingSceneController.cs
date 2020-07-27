using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectingSceneController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageField = null;
    private GameClient _client;

    private void Start()
    {
        messageField.text = "Connecting...";

        _client = GameClient.Instance;

        _client.OnConnect += OnConnect;
        _client.OnJoin += OnJoin;

        if (!_client.Connected)
        {
            _client.Connect();
        }
        else
        {
            OnConnect(this, null);
        }
    }

    private void OnConnect(object sender, EventArgs e)
    {
        messageField.text = "Finding a game...";

        if (!_client.Joined)
        {
            _client.Join();
        }
        else
        {
            OnJoin(this, null);
        }
    }

    private void OnJoin(object sender, EventArgs e)
    {
        messageField.text = "Joined! Finding another player...";

        _client.OnGamePhaseChange += GamePhaseChangeHandler;
    }

    private static void GamePhaseChangeHandler(object sender, string phase)
    {
        if (phase == "place")
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    private void OnDestroy()
    {
        if (_client == null) return;
        _client.OnConnect -= OnConnect;
        _client.OnJoin -= OnJoin;
        _client.OnGamePhaseChange -= GamePhaseChangeHandler;
    }
}