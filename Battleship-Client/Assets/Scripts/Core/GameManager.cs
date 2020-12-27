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
        [SerializeField] private Key statusSelectMode;
        [SerializeField] private Key statusConnecting;
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

        public void ConnectToServer(Action onSuccess, Action onError)
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
                GameSceneManager.Instance.GoToPlanScene();
            };
            var networkClient = (NetworkClient) Client;
            SetStatusText(statusConnecting);
            networkClient.Connect(networkOptions.EndPoint,
                () => { onSuccess?.Invoke(); },
                () => { onError?.Invoke(); });
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
            Client.Connect();
        }

        public void FinishNetworkClient()
        {
            if (Client is NetworkClient networkClient)
            {
                networkClient.LeaveRoom();
                networkClient.LeaveLobby();
                Client = null;
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