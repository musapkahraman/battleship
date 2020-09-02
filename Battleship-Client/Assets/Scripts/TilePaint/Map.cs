using BattleshipGame.Core;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.TilePaint
{
    public class Map : ShipTilePainter
    {
        [SerializeField] private Tilemap fleetLayer;
        [SerializeField] private PlacementManager manager;

        public override bool SetShip(Ship ship, Vector3Int coordinate, bool shouldSendToManager = true)
        {
            if (shouldSendToManager && !manager.PlaceShipOnDrag(ship, coordinate)) return false;
            fleetLayer.SetTile(coordinate, ship.tile);
            return true;
        }

        public override void ClearAllShips()
        {
            fleetLayer.ClearAllTiles();
        }
    }
}