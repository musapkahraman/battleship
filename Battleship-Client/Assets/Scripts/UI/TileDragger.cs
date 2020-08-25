using BattleshipGame.Common;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(Grid), typeof(BoxCollider2D), typeof(GridSpriteMapper))]
    public class TileDragger : MonoBehaviour
    {
        [SerializeField] private GameObject dragShipPrefab;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private GameManager manager;
        [SerializeField] private MapViewer targetMapViewer;
        [SerializeField] private Tilemap sourceTileMap;
        [SerializeField] private bool removeFromSource;
        [SerializeField] private bool removeIfDraggedOut;
        private Vector3Int _grabbedFrom;
        private GameObject _grabbedShip;
        private Grid _grid;
        private bool _isReleasedInside;
        private GridSpriteMapper _selfGridSpriteMapper;
        private MapViewer _selfMapViewer;
        private Sprite _sprite;
        private GridSpriteMapper _targetGridSpriteMapper;

        private void Start()
        {
            _grid = GetComponent<Grid>();
            _selfMapViewer = GetComponent<MapViewer>();
            _selfGridSpriteMapper = GetComponent<GridSpriteMapper>();
            _targetGridSpriteMapper = targetMapViewer.GetComponent<GridSpriteMapper>();
        }

        private void OnMouseDown()
        {
            _isReleasedInside = false;
            if (_selfMapViewer && _selfMapViewer.Mode == MapViewer.MapMode.Attack) return;
            _grabbedFrom = GridConverter.ScreenToCell(Input.mousePosition, _grid, sceneCamera, manager.MapAreaSize);
            _sprite = _selfGridSpriteMapper.GetSpriteAt(ref _grabbedFrom);
            if (!_sprite) return;
            _grabbedShip = Instantiate(dragShipPrefab, GetMousePositionOnZeroZ(), Quaternion.identity);
            _grabbedShip.GetComponent<SpriteRenderer>().sprite = _sprite;
            if (removeFromSource) sourceTileMap.SetTile(_grabbedFrom, null);
        }

        private void OnMouseDrag()
        {
            if (_grabbedShip) _grabbedShip.transform.position = GetMousePositionOnZeroZ();
        }

        private void OnMouseUp()
        {
            if (!_grabbedShip) return;

            if (removeIfDraggedOut && !_isReleasedInside)
            {
                if (_targetGridSpriteMapper)
                    _targetGridSpriteMapper.RemoveSpritePosition(_sprite.GetInstanceID(), _grabbedFrom);
                Destroy(_grabbedShip);
                return;
            }

            foreach (var ship in manager.Ships)
                if (ship.sprite.Equals(_sprite))
                {
                    var droppedTo = GridConverter.ScreenToCell(Input.mousePosition,
                        targetMapViewer.GetComponent<Grid>(), sceneCamera, manager.MapAreaSize);

                    if (manager.DoesShipFitIn(ship, droppedTo))
                    {
                        targetMapViewer.SetShip(ship, droppedTo);
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

        private void OnMouseUpAsButton()
        {
            _isReleasedInside = true;
        }

        private Vector3 GetMousePositionOnZeroZ()
        {
            var position = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(position.x, position.y, 0);
        }
    }
}