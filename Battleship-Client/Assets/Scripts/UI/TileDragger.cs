using System.Linq;
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
        [SerializeField] private bool removeTileFromSource;
        private GridSpriteMapper _gridSpriteMapper;
        private Sprite _sprite;
        private Grid _grid;
        private GameObject _draggable;
        private Vector3Int _grabbedCell;

        private void Start()
        {
            _gridSpriteMapper = GetComponent<GridSpriteMapper>();
            _grid = GetComponent<Grid>();
        }

        private void OnMouseDown()
        {
            _grabbedCell = GridConverter.ScreenToCell(Input.mousePosition, _grid, sceneCamera, manager.MapAreaSize);
            SearchForClickedSprite();
            if (_sprite)
            {
                _draggable = Instantiate(dragShipPrefab, GetMousePositionOnZeroZ(), Quaternion.identity);
                _draggable.GetComponent<SpriteRenderer>().sprite = _sprite;
            }
        }

        private void SearchForClickedSprite()
        {
            foreach (var spriteIdPositionPair in _gridSpriteMapper.GetSpritePositions())
            {
                foreach (var spritePosition in spriteIdPositionPair.Value)
                {
                    foreach (var shipPartCoordinate in FindShip(spriteIdPositionPair.Key).PartCoordinates)
                    {
                        var cell = spritePosition + (Vector3Int) shipPartCoordinate;
                        if (cell.Equals(_grabbedCell))
                        {
                            _gridSpriteMapper.GetSprites().TryGetValue(spriteIdPositionPair.Key, out _sprite);
                            _grabbedCell = spritePosition;
                            return;
                        }
                    }
                }
            }
        }

        private void OnMouseDrag()
        {
            if (_draggable)
            {
                _draggable.transform.position = GetMousePositionOnZeroZ();
            }
        }

        private void OnMouseUp()
        {
            if (_draggable)
            {
                foreach (var ship in manager.Ships)
                {
                    if (ship.sprite.Equals(_sprite))
                    {
                        var droppedCell = GridConverter.ScreenToCell(Input.mousePosition,
                            targetMapViewer.GetComponent<Grid>(), sceneCamera, manager.MapAreaSize);

                        if (manager.DoesShipFitIn(ship, droppedCell))
                        {
                            targetMapViewer.SetShip(ship, droppedCell);
                            if (removeTileFromSource) sourceTileMap.SetTile(_grabbedCell, null);
                        }
                    }
                }

                _sprite = null;
                Destroy(_draggable);
            }
        }

        private Vector3 GetMousePositionOnZeroZ()
        {
            var position = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(position.x, position.y, 0);
        }

        private Ship FindShip(int spriteId)
        {
            _gridSpriteMapper.GetSprites().TryGetValue(spriteId, out var sprite);
            return sprite is null ? null : manager.Ships.FirstOrDefault(ship => ship.sprite.Equals(sprite));
        }
    }
}