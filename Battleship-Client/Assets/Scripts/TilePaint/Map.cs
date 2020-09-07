using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public abstract class Map : MonoBehaviour
    {
        public abstract bool SetShip(Ship ship, Vector3Int coordinate, Vector3Int grabbedFrom, bool isDragged = false);
        public abstract void ClearAllShips();
    }
}