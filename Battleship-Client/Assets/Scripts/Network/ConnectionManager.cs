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

        [SerializeField] private GameObject progressBarCanvasPrefab;
        [SerializeField] private TMP_Text messageField;
        [SerializeField] private ServerType serverType = ServerType.Online;
        private Animator _progressBarAnimator;
        private Canvas _progressBarCanvas;
        public GameClient Client { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (SceneManager.GetActiveScene().Equals(SceneManager.GetSceneAt(0))) DontDestroyOnLoad(gameObject);
            
            if (progressBarCanvasPrefab)
            {
                _progressBarCanvas = Instantiate(progressBarCanvasPrefab).GetComponent<Canvas>();
                _progressBarAnimator = _progressBarCanvas.transform.GetComponentInChildren<Animator>();
            }

            Client = new GameClient();
            Client.ConnectionOpened += OnConnected;
            Client.JoinedInTheRoom += OnJoined;
            messageField.text = $"Connecting to {serverType.ToString()} server...";
            Client.Connect(serverType);
        }

        protected override void OnDestroy()
        {
            if (Client != null)
            {
                Client.ConnectionOpened -= OnConnected;
                Client.JoinedInTheRoom -= OnJoined;
                Client.GamePhaseChanged -= OnGamePhaseChanged;
            }

            base.OnDestroy();
        }

        private void OnApplicationQuit()
        {
            Client?.Leave();
        }

        private void OnConnected()
        {
            if (_progressBarAnimator) _progressBarAnimator.enabled = false;
            if (_progressBarCanvas) _progressBarCanvas.enabled = false;
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
            if (phase == "place") SceneManager.LoadScene(1);
        }
    }
}