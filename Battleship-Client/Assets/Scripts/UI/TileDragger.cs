using BattleshipGame.Common;
using BattleshipGame.Core;
using UnityEngine;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(Grid), typeof(BoxCollider2D), typeof(GridSpriteMapper))]
    public class TileDragger : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private GameManager manager;
        [SerializeField] private MapViewer opponentMap;
        [SerializeField] private GameObject dragShipPrefab;
        private GridSpriteMapper _spriteMapper;
        private Sprite _sprite;
        private Grid _grid;
        private GameObject _draggable;

        private void Start()
        {
            _spriteMapper = GetComponent<GridSpriteMapper>();
            _grid = GetComponent<Grid>();
        }

        private void OnMouseDown()
        {
            var clickedCell = GridConverter.ScreenToCell(Input.mousePosition, _grid, sceneCamera, manager.MapAreaSize);
            foreach (var spriteIdPositionPair in _spriteMapper.GetSpritePositions())
            {
                foreach (var spritePosition in spriteIdPositionPair.Value)
                {
                    foreach (var shipPart in _spriteMapper.GetPartsList())
                    {
                        var cell = Vector3Int.FloorToInt(spritePosition + (Vector3) shipPart.Coordinate);
                        if (cell.Equals(clickedCell))
                        {
                            _spriteMapper.GetSprites().TryGetValue(spriteIdPositionPair.Key, out _sprite);
                        }
                    }
                }
            }

            if (_sprite)
            {
                _draggable = Instantiate(dragShipPrefab, GetMousePositionOnZeroZ(), Quaternion.identity);
                _draggable.GetComponent<SpriteRenderer>().sprite = _sprite;
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
                        var clickedCell = GridConverter.ScreenToCell(Input.mousePosition,
                            opponentMap.GetComponent<Grid>(), sceneCamera, manager.MapAreaSize);

                        if (manager.DoesShipFitIn(ship, clickedCell))
                        {
                            opponentMap.SetShip(ship, clickedCell);
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
    }
}