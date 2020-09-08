using System;
using BattleshipGame.Common;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;
using static BattleshipGame.Common.MapInteractionMode;

namespace BattleshipGame.TilePaint
{
    [RequireComponent(typeof(Grid), typeof(BoxCollider2D), typeof(GridSpriteMapper))]
    public class TileDragger : MonoBehaviour
    {
        [SerializeField] private GameObject dragShipPrefab;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Rules rules;
        [SerializeField] private Map targetMap;
        [SerializeField] private Tilemap sourceTileMap;
        [SerializeField] private bool removeFromSource;
        [SerializeField] private bool removeIfDraggedOut;
        private BattleMap _battleMap;
        private Vector3Int _grabbedFrom;
        private GameObject _grabbedShip;
        private Grid _grid;
        private bool _isReleasedInside;
        private GridSpriteMapper _selfGridSpriteMapper;
        private Sprite _sprite;
        private GridSpriteMapper _targetGridSpriteMapper;

        private void Start()
        {
            _grid = GetComponent<Grid>();
            _selfGridSpriteMapper = GetComponent<GridSpriteMapper>();
            _targetGridSpriteMapper = targetMap.GetComponent<GridSpriteMapper>();
            _battleMap = GetComponent<BattleMap>();
            if (_battleMap) return;
            if (targetMap is BattleMap battleMap) _battleMap = battleMap;
        }

        private void OnMouseDown()
        {
            _isReleasedInside = false;
            if (_battleMap && _battleMap.InteractionMode != ShipDragging) return;
            Grab();
        }

        private void OnMouseDrag() => Drag();

        private void OnMouseUp() => Drop();

        private void OnMouseUpAsButton() => _isReleasedInside = true;

        private void Grab()
        {
            _grabbedFrom = GridUtils.ScreenToCoordinate(Input.mousePosition, _grid, sceneCamera, rules.AreaSize);
            _sprite = _selfGridSpriteMapper.GetSpriteAt(ref _grabbedFrom);
            if (!_sprite) return;
            _grabbedShip = Instantiate(dragShipPrefab, GetMousePositionOnZeroZ(), Quaternion.identity);
            _grabbedShip.GetComponent<SpriteRenderer>().sprite = _sprite;
            if (removeFromSource) sourceTileMap.SetTile(_grabbedFrom, null);
        }

        private void Drag()
        {
            if (_grabbedShip) _grabbedShip.transform.position = GetMousePositionOnZeroZ();
        }

        private Vector3 GetMousePositionOnZeroZ()
        {
            var position = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(position.x, position.y, 0);
        }

        private void Drop()
        {
            if (!_grabbedShip) return;

            if (removeIfDraggedOut && !_isReleasedInside)
            {
                if (_targetGridSpriteMapper)
                    _targetGridSpriteMapper.RemoveSpritePosition(_sprite.GetInstanceID(), _grabbedFrom);
                Destroy(_grabbedShip);
                return;
            }

            foreach (var ship in rules.ships)
                if (ship.tile.sprite.Equals(_sprite))
                {
                    var droppedTo = GridUtils.ScreenToCoordinate(Input.mousePosition,
                        targetMap.GetComponent<Grid>(), sceneCamera, rules.AreaSize);
                    (int shipWidth, int shipHeight) = ship.GetShipSize();
                    if (GridUtils.DoesShipFitIn(shipWidth, shipHeight, droppedTo, rules.AreaSize.x) &&
                        targetMap.SetShip(ship, droppedTo, _grabbedFrom, true))
                    {
                        if (_targetGridSpriteMapper)
                            _targetGridSpriteMapper.ChangeSpritePosition(_sprite, _grabbedFrom, droppedTo);
                    }
                    else if (removeFromSource)
                    {
                        // Tile is already removed on mouse down, place it back.
                        sourceTileMap.SetTile(_grabbedFrom, ship.tile);
                    }
                }

            Destroy(_grabbedShip);
        }
    }
}