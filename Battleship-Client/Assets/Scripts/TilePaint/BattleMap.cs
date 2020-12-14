using System;
using BattleshipGame.Common;
using BattleshipGame.Core;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static BattleshipGame.Common.GridUtils;
using static BattleshipGame.Common.MapInteractionMode;

namespace BattleshipGame.TilePaint
{
    public class BattleMap : Map, IPointerClickHandler
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
                    case NoInteraction:
                        break;
                    case TargetMarking:
                        IsMarkingTargets = true;
                        break;
                    case TurnHighlighting:
                        break;
                    case ShipDragging:
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (screenType == ScreenType.Opponent && InteractionMode == TargetMarking)
                manager.MarkTarget(ScreenToCell(eventData.position, sceneCamera, _grid, rules.AreaSize));
        }

#if UNITY_WEBGL || UNITY_STANDALONE || UNITY_EDITOR
        private void OnMouseOver()
        {
            if (screenType != ScreenType.Opponent || InteractionMode != TargetMarking) return;
            cursorLayer.ClearAllTiles();
            var coordinate = ScreenToCell(Input.mousePosition, sceneCamera, _grid, rules.AreaSize);
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

        private void OnMouseExit()
        {
            if (screenType != ScreenType.Opponent || InteractionMode != TargetMarking) return;
            cursorLayer.ClearAllTiles();
        }
#endif

        public override void SetShip(Ship ship, Vector3Int coordinate)
        {
            fleetLayer.SetTile(coordinate, ship.tile);
        }

        public override bool MoveShip(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn)
        {
            SetShip(ship, to);
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