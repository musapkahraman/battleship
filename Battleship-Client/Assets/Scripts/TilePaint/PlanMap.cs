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

        public override void SetShip(Ship ship, Vector3Int coordinate)
        {
            fleetLayer.SetTile(coordinate, ship.tile);
        }

        public override bool MoveShip(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn)
        {
            return manager.PlaceShip(ship, from, to, isMovedIn);
        }

        public override void ClearAllShips()
        {
            fleetLayer.ClearAllTiles();
        }
    }
}