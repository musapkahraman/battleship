using System.Collections.Generic;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace BattleshipGame.Tiling
{
    public class TurnHighlighter : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Rules rules;
        [SerializeField] private Tilemap layer;
        [SerializeField] private Tile tile;

        private Grid _grid;
        private OpponentStatus _status;
        private ITurnClickListener _clickListener;

        private void Start()
        {
            _grid = GetComponent<Grid>();
            _status = GetComponent<OpponentStatus>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var coordinate = GridUtils.ScreenToCell(eventData.position, sceneCamera, _grid, rules.areaSize);
            if (_status)
            {
                // This is OpponentStatusMap
                int shotTurn = _status.GetShotTurn(coordinate);
                _clickListener.HighlightTurn(shotTurn);
            }
            else
            {
                _clickListener.HighlightShotsInTheSameTurn(coordinate);
            }
        }

        public void SetClickListener(ITurnClickListener clickListener)
        {
            _clickListener = clickListener;
        }

        public void HighlightTurnShotsOnOpponentMap(IEnumerable<int> cells)
        {
            layer.ClearAllTiles();
            foreach (int cell in cells)
                layer.SetTile(GridUtils.CellIndexToCoordinate(cell, rules.areaSize.x), tile);
        }

        public void HighlightTurnShotsOnOpponentStatusMap(int turn)
        {
            layer.ClearAllTiles();
            foreach (var coordinate in _status.GetCoordinates(turn))
                layer.SetTile(coordinate, tile);
        }
    }
}