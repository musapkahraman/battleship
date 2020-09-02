using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public abstract class ShipTilePainter : MonoBehaviour
    {
        public abstract bool SetShip(Ship ship, Vector3Int coordinate, bool option = true);
        public abstract void ClearAllShips();
    }
}