using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleshipGame.Network
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private RoomsListManager roomsList;
        private void Start()
        {
            if (ConnectionManager.TryGetInstance(out var connectionManager))
            {
                connectionManager.ConnectToServer();
                PopulateRoomsList();
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        private void PopulateRoomsList()
        {
            
            roomsList.SetRooms(15);
        }
    }
}