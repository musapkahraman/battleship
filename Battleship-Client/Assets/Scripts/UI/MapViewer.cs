using System;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public enum ShipType
    {
        Admiral = 0,
        VCruiser,
        HCruiser,
        VGunBoat,
        HGunBoat,
        Scout1,
        Scout2,
        Scout3,
        Scout4
    }

    public enum Marker
    {
        Target = 9,
        Hit,
        Miss,
        Marked,
        Fired
    }

    public enum MapMode
    {
        Disabled,
        Place,
        Attack
    }

    public class MapViewer : MonoBehaviour
    {
        private Tile _cursorTile;
        private Grid _grid;
        private Vector3Int _maxCellCoordinate;
        private Vector3Int _minCellCoordinate;
        private MapMode _mode;
        [SerializeField] private Camera cam;
        [SerializeField] private Tilemap cursorLayer;
        [SerializeField] private Tile[] cursorTiles;
        [SerializeField] private Tilemap fleetLayer;
        [SerializeField] private GameManager manager;
        [SerializeField] private Tilemap markerLayer;
        [SerializeField] private ScreenType screenType;

        private void Start()
        {
            if (cam == null) cam = Camera.main;
            _grid = GetComponent<Grid>();
        }

        private void OnMouseDown()
        {
            var cellPosition = ConvertToCellPosition();
            switch (_mode)
            {
                case MapMode.Place:
                    if (screenType == ScreenType.User) manager.PlaceShip(cellPosition);

                    break;
                case MapMode.Attack:
                    if (screenType == ScreenType.Opponent) manager.MarkTarget(cellPosition);

                    break;
                case MapMode.Disabled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            cursorLayer.ClearAllTiles();
            if (_mode == MapMode.Disabled) return;
            var cellPosition = ConvertToCellPosition();
            cursorLayer.SetTile(cellPosition, _cursorTile);
        }

        private Vector3Int ConvertToCellPosition()
        {
            var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            var cellPosition = _grid.WorldToCell(worldPosition);
            cellPosition.Clamp(_minCellCoordinate, _maxCellCoordinate);
            return cellPosition;
        }

        public void SetDisabled()
        {
            _mode = MapMode.Disabled;
        }

        public void SetPlacementMode()
        {
            _mode = MapMode.Place;
            _minCellCoordinate = new Vector3Int(0, 0, 0);
            _maxCellCoordinate = new Vector3Int(manager.MapSize - 1, manager.MapSize - 1, 0);
        }

        public void SetAttackMode()
        {
            _mode = MapMode.Attack;
            _cursorTile = cursorTiles[(int) Marker.Target];
            _minCellCoordinate = new Vector3Int(0, 0, 0);
            _maxCellCoordinate = new Vector3Int(manager.MapSize - 1, manager.MapSize - 1, 0);
        }

        public void SetShipCursor(ShipType shipType)
        {
            _cursorTile = cursorTiles[(int) shipType];
        }

        public void SetShip(ShipType shipType, Vector3Int coordinate)
        {
            var index = (int) shipType;
            var tile = cursorTiles[index];
            fleetLayer.SetTile(coordinate, tile);
        }

        public bool SetMarker(int index, Marker marker)
        {
            var coordinate = new Vector3Int(index % manager.MapSize, index / manager.MapSize, 0);
            var tile = markerLayer.GetTile(coordinate);
            if (tile && cursorTiles[(int) marker].name.Equals(cursorTiles[(int) Marker.Marked].name)) return false;
            markerLayer.SetTile(coordinate, cursorTiles[(int) marker]);
            return true;
        }

        private enum ScreenType
        {
            User,
            Opponent
        }
    }
}