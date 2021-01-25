using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleshipGame.AI
{
    public class Enemy : MonoBehaviour
    {
        private const int EmptyCell = -1;
        private const int OutOfMap = -1;
        [SerializeField] private Rules rules;
        [SerializeField] private Options options;
        private readonly List<int> _playerShipsHealth = new List<int>();
        private readonly SortedDictionary<int, Ship> _pool = new SortedDictionary<int, Ship>();
        private readonly WaitForSeconds _thinkingSeconds = new WaitForSeconds(1f);
        private readonly List<int> _uncheckedCells = new List<int>();
        private Prediction _prediction;

        private void OnEnable()
        {
            InitUncheckedCells();
            PopulatePool();
            _prediction = new Prediction(rules, _uncheckedCells, _playerShipsHealth);

            void InitUncheckedCells()
            {
                _uncheckedCells.Clear();
                int cellCount = rules.areaSize.x * rules.areaSize.y;
                for (var i = 0; i < cellCount; i++) _uncheckedCells.Add(i);
            }

            void PopulatePool()
            {
                _pool.Clear();
                _playerShipsHealth.Clear();
                var shipId = 0;
                foreach (var ship in rules.ships)
                    for (var i = 0; i < ship.amount; i++)
                    {
                        _pool.Add(shipId, ship);
                        _playerShipsHealth.Add(ship.partCoordinates.Count);
                        shipId++;
                    }
            }
        }

        private void OnDisable()
        {
            _prediction = null;
        }

        public void ResetForRematch()
        {
            OnEnable();
        }

        public void UpdatePlayerShips(int changedShipPart, int shotTurn)
        {
            var partIndex = 0;
            foreach (var ship in _pool)
            foreach (var _ in ship.Value.partCoordinates)
            {
                if (changedShipPart == partIndex)
                    _playerShipsHealth[ship.Key]--;
                partIndex++;
            }

            if (options.aiDifficulty == Difficulty.Hard) _prediction?.Update(_playerShipsHealth.ToList(), _pool);
        }

        public IEnumerator GetShots(Action<int[]> onComplete)
        {
            yield return _thinkingSeconds;
            switch (options.aiDifficulty)
            {
                case Difficulty.Easy:
                    onComplete?.Invoke(GetRandomCells());
                    break;
                case Difficulty.Hard:
                    onComplete?.Invoke(GetPredictedCells());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int[] GetPredictedCells()
        {
            if (_prediction == null || _uncheckedCells.Count == 0) return null;
            var shipsRemaining = new List<int>();
            for (var shipId = 0; shipId < _playerShipsHealth.Count; shipId++)
                if (_playerShipsHealth[shipId] > 0)
                    shipsRemaining.Add(shipId);

            int size = rules.shotsPerTurn;
            int[] cells = _prediction.GetMostProbableCells(size, shipsRemaining).ToArray();
            for (var i = 0; i < size; i++) _uncheckedCells.Remove(cells[i]);
            return cells;
        }

        private int[] GetRandomCells()
        {
            if (_prediction == null || _uncheckedCells == null) return null;
            int size = rules.shotsPerTurn;
            var cells = new int[size];
            if (_uncheckedCells.Count == 0) return cells;
            for (var i = 0; i < size; i++)
            {
                int index = Random.Range(0, _uncheckedCells.Count);
                cells[i] = _uncheckedCells[index];
                _uncheckedCells.Remove(cells[i]);
            }

            return cells;
        }

        public int[] PlaceShipsRandomly()
        {
            int cellCount = rules.areaSize.x * rules.areaSize.y;
            var cells = new int[cellCount];
            for (var i = 0; i < cellCount; i++) cells[i] = EmptyCell;
            foreach (var kvp in _pool)
            {
                var from = new List<int>();
                for (var i = 0; i < cellCount; i++) from.Add(i);
                var isPlaced = false;
                while (!isPlaced)
                {
                    if (from.Count == 0) break;
                    int cell = from[Random.Range(0, from.Count)];
                    from.Remove(cell);
                    isPlaced = PlaceShip(kvp.Key, kvp.Value, GridUtils.CellIndexToCoordinate(cell, rules.areaSize.x));
                }
            }

            return cells;

            bool PlaceShip(int shipId, Ship ship, Vector3Int pivot)
            {
                (int shipWidth, int shipHeight) = ship.GetShipSize();
                if (!GridUtils.IsInsideBoundaries(shipWidth, shipHeight, pivot, rules.areaSize)) return false;
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
                    if (y < 0 || y > rules.areaSize.y - 1) continue; // Avoid this row if it is out of the map
                    for (int x = xMin; x <= xMax; x++)
                    {
                        if (x < 0 || x > rules.areaSize.x - 1) continue; // Avoid this column if it is out of the map
                        int cellIndex = GridUtils.CoordinateToCellIndex(new Vector3Int(x, y, 0), rules.areaSize);
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
                foreach (int cellIndex in ship.partCoordinates
                    .Select(part => new Vector3Int(pivot.x + part.x, pivot.y + part.y, 0))
                    .Select(coordinate => GridUtils.CoordinateToCellIndex(coordinate, rules.areaSize)))
                    if (cellIndex != OutOfMap)
                        cells[cellIndex] = shipId;
            }
        }
    }
}