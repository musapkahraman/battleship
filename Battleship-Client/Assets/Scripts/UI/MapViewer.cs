using System;
using BattleshipGame.Common;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public class MapViewer : MonoBehaviour
    {
        // @formatter:off
        [Header("Layers")]
        [SerializeField] private Tilemap cursorLayer;
        [SerializeField] private Tilemap markerLayer;
        [SerializeField] private Tilemap fleetLayer;
        [Space] 
        [Header("Cursors")] 
        [SerializeField] private Tile inactiveCursor;
        [SerializeField] private Tile activeCursor;
        [Space] 
        // @formatter:on
        
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private ScreenType screenType;
        [SerializeField] private GameManager manager;
        [SerializeField] private Tile[] cursorTiles;
        private Tile _cursorTile;
        private Grid _grid;
        private Vector3Int _maxCellCoordinate;
        private Vector3Int _minCellCoordinate;
        private MapMode _mode;

        private void Start()
        {
            if (sceneCamera == null) sceneCamera = Camera.main;
            _grid = GetComponent<Grid>();
            manager.FireReady += DisableTargetCursor;
            manager.FireNotReady += EnableTargetCursor;
        }

        private void OnDestroy()
        {
            manager.FireReady -= DisableTargetCursor;
            manager.FireNotReady -= EnableTargetCursor;
        }

        private void OnMouseDown()
        {
            var cell = ScreenToCell(Input.mousePosition);
            switch (_mode)
            {
                case MapMode.Place:
                    if (screenType == ScreenType.User) manager.PlaceShip(cell);
                    break;
                case MapMode.Attack:
                    if (screenType == ScreenType.Opponent) manager.MarkTarget(cell);
                    break;
                case MapMode.Disabled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnMouseOver()
        {
            cursorLayer.ClearAllTiles();
            if (_mode == MapMode.Disabled) return;
            var cell = ScreenToCell(Input.mousePosition);
            cursorLayer.SetTile(cell, _cursorTile);
        }

        private void DisableTargetCursor()
        {
            _cursorTile = cursorTiles[(int) Marker.TargetInactive];
        }

        private void EnableTargetCursor()
        {
            _cursorTile = cursorTiles[(int) Marker.TargetActive];
        }

        private Vector3Int ScreenToCell(Vector3 screenPoint)
        {
            var worldPoint = sceneCamera.ScreenToWorldPoint(screenPoint);
            var cell = _grid.WorldToCell(worldPoint);
            cell.Clamp(_minCellCoordinate, _maxCellCoordinate);
            return cell;
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
            _cursorTile = cursorTiles[(int) Marker.TargetActive];
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
            var coordinate = GridConverter.ToCoordinate(index, manager.MapSize);
            var tile = markerLayer.GetTile(coordinate);
            if (tile && cursorTiles[(int) marker].name.Equals(cursorTiles[(int) Marker.TargetMarked].name))
                return false;
            markerLayer.SetTile(coordinate, cursorTiles[(int) marker]);
            return true;
        }

        public void ClearTile(int index)
        {
            var coordinate = GridConverter.ToCoordinate(index, manager.MapSize);
            if (markerLayer.HasTile(coordinate)) markerLayer.SetTile(coordinate, null);
        }

        private enum ScreenType
        {
            User,
            Opponent
        }
    }
}