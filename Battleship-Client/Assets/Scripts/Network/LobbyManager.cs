using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Network
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private RoomListManager roomList;

        private void Start()
        {
            if (ConnectionManager.TryGetInstance(out var connectionManager))
            {
                connectionManager.ConnectToServer();
                PopulateRoomList();
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        private void PopulateRoomList()
        {
            roomList.PopulateRoomList(30);
        }
    }
}