using System;
using UnityEngine;

namespace BattleshipGame.Network
{
    public class LobbyManager : MonoBehaviour
    {
        private void Start()
        {
            ConnectionManager.Instance.ConnectToServer();
        }
    }
}