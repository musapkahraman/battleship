using System;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public class OpponentMap : MonoBehaviour
    {
        private Tile _cursorTile;
        private Grid _grid;
        private Vector3Int _maxCellCoordinate;
        private Vector3Int _minCellCoordinate;
        private MapMode _mode;
        [SerializeField] private Camera cam;
        [SerializeField] private Tilemap cursorLayer;
        [SerializeField] private Tile[] cursorTiles;
        [SerializeField] private GameManager manager;
        [SerializeField] private Tilemap markerLayer;
        [SerializeField] private int size = 9;
    
        private void Start()
        {
            if (cam == null) cam = Camera.main;
            _grid = GetComponent<Grid>();
        }

        private void OnMouseDown()
        {
            var cellPosition = ConvertToCellPosition();
            switch (_mode)
            {
                case MapMode.Attack:
                    manager.TakeTurn(cellPosition);
                    break;
                case MapMode.Place:
                case MapMode.Disabled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void Update()
        {
            cursorLayer.ClearAllTiles();
            if (_mode == MapMode.Disabled) return;
            var cellPosition = ConvertToCellPosition();
            cursorLayer.SetTile(cellPosition, _cursorTile);
        }
        
        private Vector3Int ConvertToCellPosition()
        {
            var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            var cellPosition = _grid.WorldToCell(worldPosition);
            cellPosition.Clamp(_minCellCoordinate, _maxCellCoordinate);
            return cellPosition;
        }

        public void SetDisabled()
        {
            _mode = MapMode.Disabled;
        }

        public void SetAttackMode()
        {
            _mode = MapMode.Attack;
            _cursorTile = cursorTiles[(int) Marker.Target];
            _minCellCoordinate = new Vector3Int(0, 0, 0);
            _maxCellCoordinate = new Vector3Int(size - 1, size - 1, 0);
        }

        public void SetMarker(int index, Marker marker)
        {
            var coordinate = new Vector3Int(index % size, index / size, 0);
            markerLayer.SetTile(coordinate, cursorTiles[(int) marker]);
        }
    }
}


