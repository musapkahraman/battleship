using System;
using BattleshipGame.Common;
using BattleshipGame.Core;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public class MapViewer : MonoBehaviour
    {
        public enum MapMode
        {
            Disabled,
            Place,
            Attack
        }

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

        public MapMode Mode { get; private set; }

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
            var cellCoordinate =
                GridConverter.ScreenToCell(Input.mousePosition, _grid, sceneCamera, manager.MapAreaSize);
            switch (Mode)
            {
                case MapMode.Place:
                    if (screenType == ScreenType.User) manager.PlaceShip(cellCoordinate);
                    break;
                case MapMode.Attack:
                    if (screenType == ScreenType.Opponent) manager.MarkTarget(cellCoordinate);
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
            if (Mode == MapMode.Disabled) return;
            var cell = GridConverter.ScreenToCell(Input.mousePosition, _grid, sceneCamera, manager.MapAreaSize);
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

        public void SetDisabled()
        {
            Mode = MapMode.Disabled;
        }

        public void SetPlacementMode()
        {
            Mode = MapMode.Place;
        }

        public void SetAttackMode()
        {
            Mode = MapMode.Attack;
            _cursorTile = activeCursor;
        }

        public void SetCursorTile(Tile tile)
        {
            _cursorTile = tile;
        }

        public void SetShip(Ship ship, Vector3Int coordinate)
        {
            fleetLayer.SetTile(coordinate, ship.tile);
        }

        public void ClearAllShips()
        {
            fleetLayer.ClearAllTiles();
        }

        public void ClearMarkerTile(int index)
        {
            var coordinate = GridConverter.ToCoordinate(index, manager.MapAreaSize);
            if (markerLayer.HasTile(coordinate)) markerLayer.SetTile(coordinate, null);
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

        private enum ScreenType
        {
            User,
            Opponent
        }
    }
}