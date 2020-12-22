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
        private const int EmptyCell = -1;
        private const int OutOfMap = -1;
        private const int NotShot = -1;
        [SerializeField] private Rules rules;
        private readonly WaitForSeconds _thinkingSeconds = new WaitForSeconds(1f);
        private readonly List<int> _playerShipsHealth = new List<int>();
        private int[] _playerShipsParts;
        private Prediction _prediction;
        private List<int> _uncheckedCells;

        private void OnEnable()
        {
            _prediction = new Prediction(rules);
            InitUncheckedCells();
            InitPlayerShipParts();

            void InitUncheckedCells()
            {
                _uncheckedCells = new List<int>();
                int cellCount = rules.areaSize.x * rules.areaSize.y;
                for (var i = 0; i < cellCount; i++) _uncheckedCells.Add(i);
            }

            void InitPlayerShipParts()
            {
                var totalShipPartCount = 0;
                foreach (var ship in rules.ships)
                    for (var i = 0; i < ship.amount; i++)
                    {
                        totalShipPartCount += ship.PartCoordinates.Count;
                        _playerShipsHealth.Add(ship.PartCoordinates.Count);
                    }

                _playerShipsParts = new int[totalShipPartCount];
                for (var i = 0; i < _playerShipsParts.Length; i++) _playerShipsParts[i] = NotShot;
            }
        }

        private void OnDisable()
        {
            _prediction = null;
            _uncheckedCells = null;
        }

        public void ResetForRematch()
        {
            OnEnable();
        }

        public void UpdatePlayerShips(int changedShipPart, int shotTurn)
        {
            Debug.Log($"Player ships shot. Part: {changedShipPart}, Turn: {shotTurn}");
            _playerShipsParts[changedShipPart] = shotTurn;
            var shipIndex = 0;
            var partIndex = 0;
            foreach (var ship in rules.ships)
                for (var i = 0; i < ship.amount; i++)
                {
                    foreach (var _ in ship.PartCoordinates)
                    {
                        if (changedShipPart == partIndex)
                            _playerShipsHealth[shipIndex]--;
                        partIndex++;
                    }

                    shipIndex++;
                }

            for (var i = 0; i < _playerShipsHealth.Count; i++)
            {
                Debug.Log($"Ship: {i} Health: {_playerShipsHealth[i]}");
            }
        }

        public IEnumerator GetShots(Action<int[]> onComplete)
        {
            yield return _thinkingSeconds;
            switch (rules.aiDifficulty)
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
            if (_prediction == null || _uncheckedCells == null) return null;
            if (_uncheckedCells.Count == 0) return null;
            int size = rules.shotsPerTurn;
            int[] cells = _prediction
                .GetMostProbableCells(_uncheckedCells, size, new[] {0, 1, 2, 3, 4, 5, 6, 7, 8})
                .ToArray();
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
                    isPlaced = PlaceShip(kvp.Key, kvp.Value, GridUtils.CellIndexToCoordinate(cell, rules.areaSize.x));
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
                if (!GridUtils.DoesShipFitIn(shipWidth, shipHeight, pivot, rules.areaSize.x)) return false;
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
                foreach (int cellIndex in ship.PartCoordinates
                    .Select(part => new Vector3Int(pivot.x + part.x, pivot.y + part.y, 0))
                    .Select(coordinate => GridUtils.CoordinateToCellIndex(coordinate, rules.areaSize)))
                    if (cellIndex != OutOfMap)
                        cells[cellIndex] = shipId;
            }
        }
    }
}