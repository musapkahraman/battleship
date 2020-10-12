using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public abstract class Map : MonoBehaviour
    {
        public abstract void SetShip(Ship ship, Vector3Int coordinate);

        public abstract bool MoveShip(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn);
        
        public abstract void ClearAllShips();
    }
}