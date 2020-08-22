using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Network
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageField;
        [SerializeField] private Animator progressBar;
        private GameClient _client;
        private Canvas _pBarCanvas;

        private void Awake()
        {
            _pBarCanvas = progressBar.transform.parent.GetComponent<Canvas>();
        }

        private void Start()
        {
            messageField.text = "Connecting to server...";
            _client = GameClient.Instance;
            _client.ConnectionOpened += OnConnected;
            _client.JoinedIn += OnJoined;
            _client.Connect();
        }

        private void OnDestroy()
        {
            if (_client == null) return;
            _client.ConnectionOpened -= OnConnected;
            _client.JoinedIn -= OnJoined;
            _client.GamePhaseChanged -= OnGamePhaseChanged;
        }

        private void OnConnected()
        {
            progressBar.enabled = false;
            _pBarCanvas.enabled = false;
            messageField.text = "Connection successful. Finding a game to join...";
            if (_client.Joined)
                OnJoined();
        }

        private void OnJoined()
        {
            messageField.text = "Successfully joined in. Waiting for the other player...";
            _client.GamePhaseChanged += OnGamePhaseChanged;
        }

        private static void OnGamePhaseChanged(string phase)
        {
            if (phase == "place") SceneManager.LoadScene("GameScene");
        }
    }
}