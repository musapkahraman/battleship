using System;
using BattleshipGame.AI;
using BattleshipGame.Core;
using BattleshipGame.Network;
using Colyseus;
using UnityEngine;
using static BattleshipGame.Core.StatusData.Status;

namespace BattleshipGame.Managers
{
    public class GameManager : ColyseusManager<GameManager>
    {
        [SerializeField] private NetworkOptions networkOptions;
        [SerializeField] private StatusData statusData;
        public IClient Client { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            statusData.State = GameStart;
        }

        protected override void OnApplicationQuit()
        {
            FinishNetworkClient();
        }

        public void ConnectToServer(Action onSuccess, Action onError)
        {
            Debug.Log("Connecting to server..");
            switch (Client)
            {
                case NetworkClient _:
                    Debug.Log("Go to lobby");
                    GameSceneManager.Instance.GoToLobby();
                    return;
                case LocalClient _:
                    gameObject.GetComponent<LocalClient>().enabled = false;
                    break;
            }

            Client = new NetworkClient();
            Debug.Log("new NetworkClient is created");
            Client.GamePhaseChanged += phase =>
            {
                if (phase != RoomPhase.Place) return;
                GameSceneManager.Instance.GoToPlanScene();
            };
            var networkClient = (NetworkClient) Client;
            statusData.State = Connecting;
            networkClient.Connect(networkOptions.EndPoint,
                () =>
                {
                    Debug.Log("new NetworkClient is connected");
                    if (Client is NetworkClient)
                    {
                        onSuccess?.Invoke();
                    }
                },
                () =>
                {
                    if (Client is NetworkClient)
                    {
                        onError?.Invoke();
                        Client = null;
                    }
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
                if (phase != RoomPhase.Place) return;
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
    }
}