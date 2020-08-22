using BattleshipGame.Common;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Network
{
    public class ConnectionManager : Singleton<ConnectionManager>
    {
        public enum ServerType
        {
            Local,
            Online
        }

        [SerializeField] private TMP_Text messageField;
        [SerializeField] private Animator progressBar;

        [SerializeField] private ServerType serverType = ServerType.Online;
        private Canvas _pBarCanvas;

        public GameClient Client { get; private set; }

        public override void Awake()
        {
            base.Awake();
            _pBarCanvas = progressBar.transform.parent.GetComponent<Canvas>();
            Client = new GameClient();
            Client.ConnectionOpened += OnConnected;
            Client.JoinedInTheRoom += OnJoined;
            messageField.text = $"Connecting to {serverType.ToString()} server...";
            Client.Connect(serverType);
        }

        private void OnDestroy()
        {
            if (Client == null) return;
            Client.ConnectionOpened -= OnConnected;
            Client.JoinedInTheRoom -= OnJoined;
            Client.GamePhaseChanged -= OnGamePhaseChanged;
        }

        private void OnApplicationQuit()
        {
            Client.Leave();
        }

        private void OnConnected()
        {
            progressBar.enabled = false;
            _pBarCanvas.enabled = false;
            messageField.text = "Connection successful. Finding a game to join...";
            if (Client.Joined)
                OnJoined();
        }

        private void OnJoined()
        {
            messageField.text = "Successfully joined in. Waiting for the other player...";
            Client.GamePhaseChanged += OnGamePhaseChanged;
        }

        private static void OnGamePhaseChanged(string phase)
        {
            if (phase == "place") SceneManager.LoadScene("GameScene");
        }
    }
}