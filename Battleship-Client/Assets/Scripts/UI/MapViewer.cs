using System;
using BattleshipGame.Common;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public class MapViewer : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private GameManager manager;
        [SerializeField] private ScreenType screenType;
        // @formatter:off
        [Header("Layers")]
        [SerializeField] private Tilemap cursorLayer;
        [SerializeField] private Tilemap markerLayer;
        [SerializeField] private Tilemap fleetLayer;
        [Space] 
        [Header("Cursors")] 
        [SerializeField] private Tile activeCursor;
        [SerializeField] private Tile inactiveCursor;
        [Space] 
        [Header("Markers")] 
        [SerializeField] private Tile hitMarker;
        [SerializeField] private Tile missedMarker;
        [SerializeField] private Tile markedTargetMarker;
        [SerializeField] private Tile shotTargetMarker;
        [Space] 
        // @formatter:on
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
            _cursorTile = inactiveCursor;
        }

        private void EnableTargetCursor()
        {
            _cursorTile = activeCursor;
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
            _maxCellCoordinate = new Vector3Int(manager.MapAreaSize - 1, manager.MapAreaSize - 1, 0);
        }

        public void SetAttackMode()
        {
            _mode = MapMode.Attack;
            _cursorTile = activeCursor;
            _minCellCoordinate = new Vector3Int(0, 0, 0);
            _maxCellCoordinate = new Vector3Int(manager.MapAreaSize - 1, manager.MapAreaSize - 1, 0);
        }

        public void SetCursorTile(Tile tile)
        {
            _cursorTile = tile;
        }

        public void SetShip(Ship ship, Vector3Int coordinate)
        {
            fleetLayer.SetTile(coordinate, ship.tile);
        }

        public bool SetMarker(int index, Marker marker)
        {
            Tile markerTile;
            switch (marker)
            {
                case Marker.Missed:
                    markerTile = missedMarker;
                    break;
                case Marker.Hit:
                    markerTile = hitMarker;
                    break;
                case Marker.MarkedTarget:
                    markerTile = markedTargetMarker;
                    break;
                case Marker.ShotTarget:
                    markerTile = shotTargetMarker;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(marker), marker, null);
            }

            var coordinate = GridConverter.ToCoordinate(index, manager.MapAreaSize);
            var tile = markerLayer.GetTile(coordinate);
            if (tile && !(markerTile is null) && markerTile.name.Equals(markedTargetMarker.name))
                return false;
            markerLayer.SetTile(coordinate, markerTile);
            return true;
        }

        public void ClearTile(int index)
        {
            var coordinate = GridConverter.ToCoordinate(index, manager.MapAreaSize);
            if (markerLayer.HasTile(coordinate)) markerLayer.SetTile(coordinate, null);
        }

        private enum ScreenType
        {
            User,
            Opponent
        }

        private enum MapMode
        {
            Disabled,
            Place,
            Attack
        }
    }
}