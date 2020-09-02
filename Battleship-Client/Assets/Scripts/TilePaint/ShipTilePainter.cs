using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public abstract class ShipTilePainter : MonoBehaviour
    {
        public abstract bool SetShip(Ship ship, Vector3Int coordinate);
        public abstract void ClearAllShips();
    }
}