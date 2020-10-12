using System.Linq;
using BattleshipGame.Common;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static BattleshipGame.Common.MapInteractionMode;

namespace BattleshipGame.TilePaint
{
    [RequireComponent(typeof(Grid), typeof(BoxCollider2D), typeof(GridSpriteMapper))]
    public class TileDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_battleMap && _battleMap.InteractionMode != ShipDragging) return;
            _grabbedFrom = GridUtils.ScreenToCoordinate(eventData.position, sceneCamera, _grid, rules.AreaSize);
            _sprite = _selfGridSpriteMapper.GetSpriteAt(ref _grabbedFrom);
            if (!_sprite) return;
            _grabbedShip = Instantiate(dragShipPrefab, GetZeroDepth(eventData.position), Quaternion.identity);
            _grabbedShip.GetComponent<SpriteRenderer>().sprite = _sprite;
            if (removeFromSource) sourceTileMap.SetTile(_grabbedFrom, null);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_grabbedShip) _grabbedShip.transform.position = GetZeroDepth(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_grabbedShip) return;

            bool isOverTheTarget = eventData.hovered.Contains(targetMap.gameObject);

            if (removeIfDraggedOut && !isOverTheTarget)
            {
                if (_targetGridSpriteMapper)
                    _targetGridSpriteMapper.RemoveSpritePosition(_sprite.GetInstanceID(), _grabbedFrom);
                Destroy(_grabbedShip);
                return;
            }

            if (SpriteRepresentsShip(out var ship))
            {
                var grid = targetMap.GetComponent<Grid>();
                var droppedTo = GridUtils.ScreenToCoordinate(eventData.position, sceneCamera, grid, rules.AreaSize);
                (int shipWidth, int shipHeight) = ship.GetShipSize();
                if (isOverTheTarget && _targetGridSpriteMapper &&
                    GridUtils.DoesShipFitIn(shipWidth, shipHeight, droppedTo, rules.AreaSize.x) &&
                    targetMap.SetShip(ship, droppedTo, _grabbedFrom, true))
                {
                    _targetGridSpriteMapper.ChangeSpritePosition(_sprite, _grabbedFrom, droppedTo);
                }
                else if (removeFromSource)
                {
                    // The tile is already removed inside the OnBeginDrag callback. Place the tile back.
                    sourceTileMap.SetTile(_grabbedFrom, ship.tile);
                }
            }

            Destroy(_grabbedShip);
        }

        private bool SpriteRepresentsShip(out Ship spriteShip)
        {
            foreach (var ship in rules.ships.Where(ship => ship.tile.sprite.Equals(_sprite)))
            {
                spriteShip = ship;
                return true;
            }

            spriteShip = null;
            return false;
        }

        private Vector3 GetZeroDepth(Vector2 position)
        {
            var worldPoint = sceneCamera.ScreenToWorldPoint(position);
            return new Vector3(worldPoint.x, worldPoint.y, 0);
        }
    }
}