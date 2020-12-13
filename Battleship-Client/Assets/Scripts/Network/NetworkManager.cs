using BattleshipGame.Common;
using BattleshipGame.Core;
using TMPro;
using UnityEngine;

namespace BattleshipGame.Network
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        private const string LocalEndpoint = "ws://localhost:2567";
        [SerializeField] private string onlineEndpoint = "ws://";
        [SerializeField] private ServerType serverType = ServerType.Online;
        [SerializeField] private TMP_Text messageField;
        [SerializeField] private GameObject progressBarCanvasPrefab;
        private GameObject _progressBar;
        public NetworkClient Client { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (progressBarCanvasPrefab) _progressBar = Instantiate(progressBarCanvasPrefab);
            Client = new NetworkClient();
            Client.Connected += OnConnected;
            Client.ConnectionError += OnConnectionError;
            Client.GamePhaseChanged += OnGamePhaseChanged;
            SetStatusText($"Connecting to {serverType.ToString()} server...");
            ConnectToServer();
        }

        protected override void OnDestroy()
        {
            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.ConnectionError -= OnConnectionError;
                Client.GamePhaseChanged -= OnGamePhaseChanged;
            }

            base.OnDestroy();
        }

        private void OnApplicationQuit()
        {
            Client?.LeaveRoom();
            Client?.LeaveLobby();
        }

        public void SetStatusText(string text)
        {
            messageField.text = text;
        }

        public void ConnectToServer()
        {
            Client.Connect(serverType == ServerType.Online ? onlineEndpoint : LocalEndpoint);
        }

        public void ClearStatusText()
        {
            messageField.text = string.Empty;
        }

        private void OnConnected()
        {
            Destroy(_progressBar);
            GameSceneManager.Instance.GoToLobby();
        }

        private void OnConnectionError(string errorMessage)
        {
            Destroy(_progressBar);
            SetStatusText(errorMessage);
        }

        private void OnGamePhaseChanged(string phase)
        {
            if (phase != "place") return;
            Destroy(_progressBar);
            GameSceneManager.Instance.GoToPlanScene();
        }

        private enum ServerType
        {
            Local,
            Online
        }
    }
}