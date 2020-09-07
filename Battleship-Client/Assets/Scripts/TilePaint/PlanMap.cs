using BattleshipGame.Core;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.TilePaint
{
    public class PlanMap : Map
    {
        [SerializeField] private Tilemap fleetLayer;
        [SerializeField] private PlanManager manager;

        public override bool SetShip(Ship ship, Vector3Int coordinate, Vector3Int grabbedFrom, bool isDragged = false)
        {
            if (isDragged && !manager.PlaceShip(ship, coordinate, grabbedFrom)) return false;
            fleetLayer.SetTile(coordinate, ship.tile);
            return true;
        }

        public override void ClearAllShips()
        {
            fleetLayer.ClearAllTiles();
        }
    }
}