using System;
using BattleshipGame.AI;
using BattleshipGame.Common;
using BattleshipGame.Network;
using TMPro;
using UnityEngine;

namespace BattleshipGame.Core
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private NetworkOptions networkOptions;
        [SerializeField] private TMP_Text messageField;
        [SerializeField] private GameObject progressBarCanvasPrefab;
        private GameObject _progressBar;
        public IClient Client { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SetStatusText("Select a mode to play.");
        }

        private void OnApplicationQuit()
        {
            FinishNetworkClient();
        }

        public void ConnectToServer(Action onError = null)
        {
            if (Client is NetworkClient)
            {
                GameSceneManager.Instance.GoToLobby();
                return;
            }

            Client = new NetworkClient();
            Client.GamePhaseChanged += phase =>
            {
                if (phase != "place") return;
                Destroy(_progressBar);
                GameSceneManager.Instance.GoToPlanScene();
            };
            var networkClient = (NetworkClient) Client;
            SetStatusText("Connecting to server...");
            if (progressBarCanvasPrefab) _progressBar = Instantiate(progressBarCanvasPrefab);
            networkClient.Connect(networkOptions.GetEndpoint(), () =>
            {
                Destroy(_progressBar);
                GameSceneManager.Instance.GoToLobby();
            }, errorMessage =>
            {
                Destroy(_progressBar);
                SetStatusText(errorMessage);
                onError?.Invoke();
            });
        }

        public void StartLocalClient()
        {
            FinishNetworkClient();
            Client = new LocalClient();
        }

        private void FinishNetworkClient()
        {
            if (Client is NetworkClient networkClient)
            {
                networkClient.LeaveRoom();
                networkClient.LeaveLobby();
            }
        }

        public void SetStatusText(string text)
        {
            messageField.text = text;
        }

        public void ClearStatusText()
        {
            messageField.text = string.Empty;
        }
    }
}