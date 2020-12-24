using System;
using BattleshipGame.AI;
using BattleshipGame.Common;
using BattleshipGame.Localization;
using BattleshipGame.Network;
using UnityEngine;

namespace BattleshipGame.Core
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private NetworkOptions networkOptions;
        [SerializeField] private LocalizedText statusText;
        [SerializeField] private GameObject progressBarCanvasPrefab;
        [SerializeField] private Key statusSelectMode;
        [SerializeField] private Key statusConnecting;
        [SerializeField] private Key statusNetworkError;
        private GameObject _progressBar;
        public IClient Client { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SetStatusText(statusSelectMode);
        }

        private void OnApplicationQuit()
        {
            FinishNetworkClient();
        }

        public void ConnectToServer(Action onError = null)
        {
            switch (Client)
            {
                case NetworkClient _:
                    GameSceneManager.Instance.GoToLobby();
                    return;
                case LocalClient _:
                    gameObject.GetComponent<LocalClient>().enabled = false;
                    break;
            }

            Client = new NetworkClient();
            Client.GamePhaseChanged += phase =>
            {
                if (phase != "place") return;
                Destroy(_progressBar);
                GameSceneManager.Instance.GoToPlanScene();
            };
            var networkClient = (NetworkClient) Client;
            SetStatusText(statusConnecting);
            if (progressBarCanvasPrefab) _progressBar = Instantiate(progressBarCanvasPrefab);
            networkClient.Connect(networkOptions.GetEndpoint(), () =>
            {
                Destroy(_progressBar);
                GameSceneManager.Instance.GoToLobby();
            }, errorMessage =>
            {
                Destroy(_progressBar);
                SetStatusText(statusNetworkError);
                Debug.LogError(errorMessage);
                onError?.Invoke();
            });
        }

        public void StartLocalClient()
        {
            FinishNetworkClient();
            var localClient = GetComponent<LocalClient>();
            localClient.enabled = true;
            Client = localClient;
            Client.GamePhaseChanged += phase =>
            {
                if (phase != "place") return;
                GameSceneManager.Instance.GoToPlanScene();
            };
            Client.Connect(string.Empty);
        }

        private void FinishNetworkClient()
        {
            if (Client is NetworkClient networkClient)
            {
                networkClient.LeaveRoom();
                networkClient.LeaveLobby();
            }
        }

        public void SetStatusText(Key text)
        {
            statusText.SetText(text);
        }

        public void ClearStatusText()
        {
            statusText.ClearText();
        }
    }
}