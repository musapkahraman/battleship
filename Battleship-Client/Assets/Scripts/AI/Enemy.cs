using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Common;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleshipGame.AI
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Rules rules;
        private const int EmptyCell = -1;
        private const int OutOfMap = -1;
        private readonly WaitForSeconds _thinkingSeconds = new WaitForSeconds(1f);
        private List<int> _uncheckedCells;

        private void OnEnable()
        {
            _uncheckedCells = new List<int>();
            int cellCount = rules.AreaSize.x * rules.AreaSize.y;
            for (var i = 0; i < cellCount; i++) _uncheckedCells.Add(i);
        }

        private void OnDisable()
        {
            _uncheckedCells = null;
        }

        public IEnumerator GetRandomCells(Action<int[]> onComplete)
        {
            yield return _thinkingSeconds;
            int size = rules.shotsPerTurn;
            var cells = new int[size];
            if (_uncheckedCells.Count == 0) yield break;
            for (var i = 0; i < size; i++)
            {
                int index = Random.Range(0, _uncheckedCells.Count);
                cells[i] = _uncheckedCells[index];
                _uncheckedCells.Remove(cells[i]);
            }

            onComplete?.Invoke(cells);
        }

        public int[] PlaceShipsRandomly()
        {
            int cellCount = rules.AreaSize.x * rules.AreaSize.y;
            var cells = new int[cellCount];
            for (var i = 0; i < cellCount; i++) cells[i] = EmptyCell;
            var pool = new SortedDictionary<int, Ship>();
            PopulatePool();
            foreach (var kvp in pool)
            {
                var from = new List<int>();
                for (var i = 0; i < cellCount; i++) from.Add(i);
                var isPlaced = false;
                while (!isPlaced)
                {
                    if (from.Count == 0) break;
                    int cell = from[Random.Range(0, from.Count)];
                    from.Remove(cell);
                    isPlaced = PlaceShip(kvp.Key, kvp.Value, GridUtils.CellIndexToCoordinate(cell, rules.AreaSize.x));
                }
            }

            return cells;

            void PopulatePool()
            {
                var shipId = 0;
                foreach (var ship in rules.ships)
                    for (var i = 0; i < ship.amount; i++)
                    {
                        pool.Add(shipId, ship);
                        shipId++;
                    }
            }

            bool PlaceShip(int shipId, Ship ship, Vector3Int pivot)
            {
                (int shipWidth, int shipHeight) = ship.GetShipSize();
                if (!GridUtils.DoesShipFitIn(shipWidth, shipHeight, pivot, rules.AreaSize.x)) return false;
                if (DoesCollideWithOtherShip(shipId, pivot, shipWidth, shipHeight)) return false;
                RegisterShipToCells(shipId, ship, pivot);
                return true;
            }

            bool DoesCollideWithOtherShip(int shipId, Vector3Int pivot, int shipWidth, int shipHeight)
            {
                // Create a frame of one cell thickness
                int xMin = pivot.x - 1;
                int xMax = pivot.x + shipWidth;
                int yMin = pivot.y - shipHeight;
                int yMax = pivot.y + 1;
                for (int y = yMin; y <= yMax; y++)
                {
                    if (y < 0 || y > rules.AreaSize.y - 1) continue; // Avoid this row if it is out of the map
                    for (int x = xMin; x <= xMax; x++)
                    {
                        if (x < 0 || x > rules.AreaSize.x - 1) continue; // Avoid this column if it is out of the map
                        int cellIndex = GridUtils.CoordinateToCellIndex(new Vector3Int(x, y, 0), rules.AreaSize);
                        if (cellIndex != OutOfMap &&
                            (cells[cellIndex] == EmptyCell || cells[cellIndex] == shipId)) continue;

                        return true;
                    }
                }

                return false;
            }

            void RegisterShipToCells(int shipId, Ship ship, Vector3Int pivot)
            {
                // Clear the previous placement of this ship
                for (var i = 0; i < cellCount; i++)
                    if (cells[i] == shipId)
                        cells[i] = EmptyCell;

                // Find each cell the ship covers and register the ship on them
                foreach (int cellIndex in ship.PartCoordinates
                    .Select(part => new Vector3Int(pivot.x + part.x, pivot.y + part.y, 0))
                    .Select(coordinate => GridUtils.CoordinateToCellIndex(coordinate, rules.AreaSize)))
                    if (cellIndex != OutOfMap)
                        cells[cellIndex] = shipId;
            }
        }
    }
}