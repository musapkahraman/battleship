using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.Tiling
{
    public class PlanMap : Map
    {
        [SerializeField] private Tilemap fleetLayer;
        private IPlanMapMoveListener _listener;

        public override void SetShip(Ship ship, Vector3Int coordinate)
        {
            fleetLayer.SetTile(coordinate, ship.tile);
        }

        public override bool MoveShip(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn)
        {
            return _listener.OnShipMoved(ship, from, to, isMovedIn);
        }

        public override void ClearAllShips()
        {
            fleetLayer.ClearAllTiles();
        }

        public void SetPlaceListener(IPlanMapMoveListener listener)
        {
            _listener = listener;
        }
    }
}