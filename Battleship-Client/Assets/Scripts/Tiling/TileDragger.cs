using System.Linq;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace BattleshipGame.Tiling
{
    [RequireComponent(typeof(Grid), typeof(BoxCollider2D))]
    public class TileDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private GameObject dragShipPrefab;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Rules rules;
        [SerializeField] private Map targetMap;
        [SerializeField] private Tilemap sourceTileMap;
        [SerializeField] private bool removeFromSource;
        [SerializeField] private bool removeIfDraggedOut;
        private GameObject _grabbedShip;
        private Vector3Int _grabCell;
        private Vector3 _grabOffset;
        private Grid _grid;
        private bool _isGrabbedFromTarget;
        private GridSpriteMapper _selfGridSpriteMapper;
        private Sprite _sprite;
        private GridSpriteMapper _targetGridSpriteMapper;

        private void Start()
        {
            _grid = GetComponent<Grid>();
            _selfGridSpriteMapper = GetComponent<GridSpriteMapper>();
            _targetGridSpriteMapper = targetMap.GetComponent<GridSpriteMapper>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isGrabbedFromTarget = eventData.hovered.Contains(targetMap.gameObject);
            _grabCell = GridUtils.ScreenToCell(eventData.position, sceneCamera, _grid, rules.areaSize);
            _sprite = _selfGridSpriteMapper.GetSpriteAt(ref _grabCell);
            var grabPoint = transform.position + _grabCell + new Vector3(0.5f, 0.5f, 0);
            _grabOffset = grabPoint - GetWorldPoint(eventData.position);
            if (!_sprite) return;
            _grabbedShip = Instantiate(dragShipPrefab, _grabCell, Quaternion.identity);
            _grabbedShip.GetComponent<SpriteRenderer>().sprite = _sprite;
            if (removeFromSource) sourceTileMap.SetTile(_grabCell, null);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_grabbedShip) _grabbedShip.transform.position = GetWorldPoint(eventData.position) + _grabOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_grabbedShip) return;

            var isOverTheTarget = false;
#if UNITY_ANDROID || UNITY_IOS
            var ray = sceneCamera.ScreenPointToRay(eventData.position);
            var raycast2d = Physics2D.Raycast(ray.origin, ray.direction, 100);
            if (raycast2d) isOverTheTarget = raycast2d.transform.gameObject.Equals(targetMap.gameObject);
#else
            isOverTheTarget = eventData.hovered.Contains(targetMap.gameObject);
#endif

            if (removeIfDraggedOut && !isOverTheTarget)
            {
                if (_targetGridSpriteMapper)
                    _targetGridSpriteMapper.RemoveSpritePosition(_sprite, _grabCell);
                Destroy(_grabbedShip);
                return;
            }

            if (SpriteRepresentsShip(out var ship))
            {
                var grid = targetMap.GetComponent<Grid>();
                var dropWorldPoint = GetWorldPoint(eventData.position) + _grabOffset;
                var dropCell = GridUtils.WorldToCell(dropWorldPoint, sceneCamera, grid, rules.areaSize);
                (int shipWidth, int shipHeight) = ship.GetShipSize();
                if (isOverTheTarget && _targetGridSpriteMapper &&
                    GridUtils.IsInsideBoundaries(shipWidth, shipHeight, dropCell, rules.areaSize) &&
                    targetMap.MoveShip(ship, _grabCell, dropCell, !_isGrabbedFromTarget))
                {
                    if (removeFromSource) _selfGridSpriteMapper.RemoveSpritePosition(_sprite, _grabCell);
                    _targetGridSpriteMapper.ChangeSpritePosition(_sprite, _grabCell, dropCell);
                }
                else if (removeFromSource)
                    // The tile is already removed inside the OnBeginDrag callback. Place the tile back.
                {
                    sourceTileMap.SetTile(_grabCell, ship.tile);
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

        private Vector3 GetWorldPoint(Vector2 position)
        {
            var worldPoint = sceneCamera.ScreenToWorldPoint(position);
            return new Vector3(worldPoint.x, worldPoint.y, 0);
        }
    }
}