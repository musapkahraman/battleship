using System;
using BattleshipGame.Common;
using BattleshipGame.Core;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;
using static BattleshipGame.Common.GridUtils;
using static BattleshipGame.Common.MapInteractionMode;

namespace BattleshipGame.TilePaint
{
    public class BattleMap : Map
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private BattleManager manager;
        [SerializeField] private Rules rules;
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
        private Grid _grid;

        private MapInteractionMode _interactionMode;

        public MapInteractionMode InteractionMode
        {
            get => _interactionMode;
            set
            {
                switch (value)
                {
                    case Disabled:
                        break;
                    case MarkTargets:
                        IsMarkingTargets = true;
                        break;
                    case HighlightTurn:
                        break;
                    case GrabShips:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                _interactionMode = value;
            }
        }

        public bool IsMarkingTargets { get; set; }

        private void Start()
        {
            if (sceneCamera == null) sceneCamera = Camera.main;
            _grid = GetComponent<Grid>();
        }

        private void OnMouseDown()
        {
            if (InteractionMode != MarkTargets || screenType != ScreenType.Opponent) return;
            manager.MarkTarget(ScreenToCoordinate(Input.mousePosition, _grid, sceneCamera, rules.AreaSize));
        }

        private void OnMouseExit()
        {
            if (InteractionMode != MarkTargets || screenType != ScreenType.Opponent) return;
            cursorLayer.ClearAllTiles();
        }

        private void OnMouseOver()
        {
            if (InteractionMode != MarkTargets || screenType != ScreenType.Opponent) return;
            cursorLayer.ClearAllTiles();
            var coordinate = ScreenToCoordinate(Input.mousePosition, _grid, sceneCamera, rules.AreaSize);
            if (markerLayer.HasTile(coordinate))
            {
                var tile = markerLayer.GetTile(coordinate);
                if (tile && tile.name.Equals(markedTargetMarker.name))
                    cursorLayer.SetTile(coordinate, inactiveCursor);
            }
            else if (IsMarkingTargets)
            {
                cursorLayer.SetTile(coordinate, activeCursor);
            }
        }

        public override bool SetShip(Ship ship, Vector3Int coordinate, Vector3Int grabbedFrom, bool isDragged = false)
        {
            fleetLayer.SetTile(coordinate, ship.tile);
            return true;
        }

        public override void ClearAllShips()
        {
            fleetLayer.ClearAllTiles();
        }

        public void ClearMarker(Vector3Int coordinate)
        {
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

            var coordinate = CellIndexToCoordinate(index, rules.AreaSize.x);
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