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
        private Vector3Int _grabbedFrom;
        private GameObject _grabbedShip;
        private Grid _grid;
        private GridSpriteMapper _gridSpriteMapper;
        private Sprite _sprite;
        private MapViewer _selfMapViewer;

        private void Start()
        {
            _gridSpriteMapper = GetComponent<GridSpriteMapper>();
            _grid = GetComponent<Grid>();
            _selfMapViewer = GetComponent<MapViewer>();
        }

        private void OnMouseDown()
        {
            if (_selfMapViewer && _selfMapViewer.Mode == MapViewer.MapMode.Attack) return;
            _grabbedFrom = GridConverter.ScreenToCell(Input.mousePosition, _grid, sceneCamera, manager.MapAreaSize);
            SearchForClickedSprite();
            if (!_sprite) return;
            _grabbedShip = Instantiate(dragShipPrefab, GetMousePositionOnZeroZ(), Quaternion.identity);
            _grabbedShip.GetComponent<SpriteRenderer>().sprite = _sprite;
            if (removeTileFromSource) sourceTileMap.SetTile(_grabbedFrom, null);
        }

        private void OnMouseDrag()
        {
            if (_grabbedShip) _grabbedShip.transform.position = GetMousePositionOnZeroZ();
        }

        private void OnMouseUp()
        {
            if (!_grabbedShip) return;

            foreach (var ship in manager.Ships)
                if (ship.sprite.Equals(_sprite))
                {
                    var droppedTo = GridConverter.ScreenToCell(Input.mousePosition,
                        targetMapViewer.GetComponent<Grid>(), sceneCamera, manager.MapAreaSize);

                    if (manager.DoesShipFitIn(ship, droppedTo))
                    {
                        targetMapViewer.SetShip(ship, droppedTo);
                        var targetGridSpriteMapper = targetMapViewer.GetComponent<GridSpriteMapper>();
                        if (targetGridSpriteMapper)
                            targetGridSpriteMapper.ChangeSpritePosition(_sprite, _grabbedFrom, droppedTo);
                    }
                    else if (removeTileFromSource)
                    {
                        sourceTileMap.SetTile(_grabbedFrom, ship.tile);
                    }
                }

            _sprite = null;
            Destroy(_grabbedShip);
        }

        private void SearchForClickedSprite()
        {
            foreach (var spriteIdPositionPair in _gridSpriteMapper.GetSpritePositions())
            foreach (var spritePosition in spriteIdPositionPair.Value)
            foreach (var shipPartCoordinate in FindShip(spriteIdPositionPair.Key).PartCoordinates)
            {
                var cell = spritePosition + (Vector3Int) shipPartCoordinate;
                if (!cell.Equals(_grabbedFrom)) continue;
                _gridSpriteMapper.GetSprites().TryGetValue(spriteIdPositionPair.Key, out _sprite);
                _grabbedFrom = spritePosition;
                return;
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