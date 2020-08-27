using BattleshipGame.Common;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Network
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        public enum ServerType
        {
            Local,
            Online
        }

        [SerializeField] private GameObject progressBarCanvasPrefab;
        [SerializeField] private TMP_Text messageField;
        [SerializeField] private ServerType serverType = ServerType.Online;
        public NetworkClient Client { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (SceneManager.GetActiveScene().buildIndex == 0) DontDestroyOnLoad(gameObject);
            if (progressBarCanvasPrefab) Instantiate(progressBarCanvasPrefab);
            Client = new NetworkClient();
            Client.Connected += OnConnected;
            Client.GamePhaseChanged += OnGamePhaseChanged;
            messageField.text = $"Connecting to {serverType.ToString()} server...";
            ConnectToServer();
        }

        protected override void OnDestroy()
        {
            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.GamePhaseChanged -= OnGamePhaseChanged;
            }

            base.OnDestroy();
        }

        private void OnApplicationQuit()
        {
            Client?.LeaveRoom();
            Client?.LeaveLobby();
        }

        public void ConnectToServer()
        {
            Client.Connect(serverType);
        }

        private static void OnConnected()
        {
            // Connection established. Go to the lobby.
            SceneManager.LoadScene(1);
        }

        private static void OnGamePhaseChanged(string phase)
        {
            if (phase == "place")
                // Another player is joined in the game. Phase is changed. Go to place mode.
                SceneManager.LoadScene(2);
        }
    }
}