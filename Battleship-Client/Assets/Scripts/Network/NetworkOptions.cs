using UnityEngine;

namespace BattleshipGame.Network
{
    [CreateAssetMenu(fileName = "NetworkOptions", menuName = "Battleship/Network Options")]
    public class NetworkOptions : ScriptableObject
    {
        [SerializeField] private string localEndpoint = "ws://localhost:2567";
        [SerializeField] private  string onlineEndpoint = "ws://";
        [SerializeField] private  ServerType serverType = ServerType.Online;

        public string GetEndpoint()
        {
            return serverType == ServerType.Online ? onlineEndpoint : localEndpoint;
        }

        private enum ServerType
        {
            Local,
            Online
        }
    }
}