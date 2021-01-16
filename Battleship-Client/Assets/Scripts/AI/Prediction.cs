using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class Prediction
    {
        private readonly List<int> _allShots = new List<int>();
        private readonly List<bool> _isShipFound = new List<bool>();
        private readonly List<int> _missedShots = new List<int>();
        private readonly SortedDictionary<int, List<Pattern>> _patterns = new SortedDictionary<int, List<Pattern>>();
        private readonly Dictionary<int, Pattern> _patternsTrackedLastTurn = new Dictionary<int, Pattern>();
        private readonly Dictionary<int, List<Probability>> _probabilityMap = new Dictionary<int, List<Probability>>();
        private readonly Rules _rules;
        private List<int> _playerShipsHealth = new List<int>(); // To figure out diffs on each Prediction.Update call.

        private List<int> _shotsAtLastTurn;

        public Prediction(Rules rules)
        {
            _rules = rules;

            foreach (var ship in _rules.ships)
                for (var i = 0; i < ship.amount; i++)
                {
                    // Initialize the local list to hold the health values of player ships. This is to figure out diffs.
                    _playerShipsHealth.Add(ship.partCoordinates.Count);
                    // Initialize the tracker for any ship to be found. This is to avoid rebuilding patterns.
                    _isShipFound.Add(false);
                }

            UpdateProbabilityMap();
        }

        private void UpdateProbabilityMap()
        {
            _probabilityMap.Clear();
            int cellCount = _rules.areaSize.x * _rules.areaSize.y;
            var shipId = 0;
            foreach (var ship in _rules.ships)
            {
                int numberOfFittingCellsForShip = GetNumberOfFittingCellsForShip(ship);
                for (var i = 0; i < ship.amount; i++)
                {
                    var shipProbabilities = new List<Probability>();
                    for (var cellIndex = 0; cellIndex < cellCount; cellIndex++)
                    {
                        var p = 0f;
                        if (!IsMissedShot(cellIndex))
                        {
                            var cell = GridUtils.CellIndexToCoordinate(cellIndex, _rules.areaSize.x);
                            if (CanAnyPartOfShipBePlacedOnACell(ship, cell))
                                p = CalculateProbability(_playerShipsHealth[shipId], numberOfFittingCellsForShip);
                        }

                        shipProbabilities.Add(new Probability(cellIndex, p));
                    }

                    _probabilityMap.Add(shipId, shipProbabilities);
                    shipId++;
                }
            }

            int GetNumberOfFittingCellsForShip(Ship ship)
            {
                var counter = 0;
                for (var cellIndex = 0; cellIndex < cellCount; cellIndex++)
                {
                    var cell = GridUtils.CellIndexToCoordinate(cellIndex, _rules.areaSize.x);
                    if (CanAnyPartOfShipBePlacedOnACell(ship, cell)) counter++;
                }

                return counter;
            }

            bool CanAnyPartOfShipBePlacedOnACell(Ship ship, Vector3Int cell)
            {
                return ship.partCoordinates.Any(shipPartCoordinate =>
                    CanPatternBePlaced(cell - (Vector3Int) shipPartCoordinate, ship));
            }

            static float CalculateProbability(int remainingShipPartCount, int availableCellCount)
            {
                if (availableCellCount == 0) return 0;
                return (float) remainingShipPartCount / availableCellCount;
            }
        }

        private float GetProbabilityValue(int shipId, int cell)
        {
            return _probabilityMap[shipId].Single(p => p.Cell == cell).Value;
        }

        public List<int> GetMostProbableCells(IEnumerable<int> unmarkedCells, int size, IEnumerable<int> shipIds)
        {
            var cells = new List<int>();
            _patternsTrackedLastTurn.Clear();
            for (var shipId = 0; shipId < _playerShipsHealth.Count; shipId++)
            {
                if (cells.Count >= size) break;
                CheckOverShipPattern(shipId);
            }

            var probabilities = (from cell in unmarkedCells
                let sum = shipIds.Sum(shipId => GetProbabilityValue(shipId, cell))
                select new Probability(cell, sum)).ToList();

            // If the probabilities of finding a ship in multiple cells are at most, select them randomly.
            float max = probabilities.Max(p => p.Value);
            int count = probabilities.Count(p => Mathf.Approximately(p.Value, max));
            if (count > size)
            {
                var randomPool = (from probability in probabilities
                    where Mathf.Approximately(probability.Value, max)
                    select probability.Cell).ToList();

                for (var i = 0; i < size; i++)
                {
                    int index = Random.Range(0, randomPool.Count);
                    if (cells.Count >= size) break;
                    cells.Add(randomPool[index]);
                    randomPool.Remove(cells[i]);
                }

                _shotsAtLastTurn = cells.ToList();
                _allShots.AddRange(_shotsAtLastTurn);
                return cells;
            }

            var orderedProbabilities = probabilities.OrderByDescending(p => p.Value);
            foreach (var probability in orderedProbabilities)
            {
                if (cells.Count >= size || probability.Value <= 0f) break;
                cells.Add(probability.Cell);
            }

            _shotsAtLastTurn = cells.ToList();
            _allShots.AddRange(_shotsAtLastTurn);
            return cells;

            void CheckOverShipPattern(int shipId)
            {
                if (!_patterns.ContainsKey(shipId)) return;
                Debug.Log($"Ship {shipId} has {_patterns[shipId].Count} patterns.");
                foreach (var pattern in _patterns[shipId])
                foreach (var shipPart in pattern.Ship.partCoordinates)
                    if (!IsPartChecked(shipPart, pattern.CheckedPartCoordinates.ToList()))
                    {
                        var coordinate = pattern.Pivot + (Vector3Int) shipPart;
                        int cellIndex = GridUtils.CoordinateToCellIndex(coordinate, _rules.areaSize);
                        if (!IsAlreadyShot(cellIndex))
                        {
                            cells.Add(cellIndex);
                            Debug.Log($"Shooting at pattern for ship {shipId} on {shipPart} at {coordinate}");
                            pattern.CheckedPartCoordinates.Add(shipPart);
                            _patternsTrackedLastTurn.Add(shipId, pattern);
                            return;
                        }
                    }
            }

            static bool IsPartChecked(Vector2Int shipPart, IEnumerable<Vector2Int> checkedParts)
            {
                return checkedParts.Any(shipPart.Equals);
            }

            bool IsAlreadyShot(int cellIndex)
            {
                return _allShots.Any(shot => cellIndex == shot);
            }
        }

        public void Update(List<int> playerShipsHealth, SortedDictionary<int, Ship> pool)
        {
            var damagedShips = new List<int>();
            var totalDamage = 0;
            for (var shipId = 0; shipId < playerShipsHealth.Count; shipId++)
            {
                int damage = _playerShipsHealth[shipId] - playerShipsHealth[shipId];
                totalDamage += damage;
                if (damage > 0)
                {
                    Debug.Log($"Ship {shipId} was damaged {damage} units.");
                    damagedShips.Add(shipId);
                    if (damage > 1)
                    {
                        Debug.Log($"Ship {shipId} had multiple shots.");
                        if (FindInPatterns(shipId, out var pattern))
                        {
                            _isShipFound[shipId] = true;
                            RemoveOtherPatterns(shipId, pattern);
                        }
                    }

                    if (IsShipSunk(shipId))
                    {
                        Debug.Log($"Ship {shipId} is sunk.");
                        if (_patterns.ContainsKey(shipId))
                        {
                            _patterns.Remove(shipId);
                            Debug.Log($"Removed all patterns from ship: {shipId}");
                        }
                    }
                }
                else
                {
                    // No shots on this ship. Remove any tracked pattern intervening with this shot.
                    if (_patterns.ContainsKey(shipId) && _patternsTrackedLastTurn.ContainsKey(shipId))
                    {
                        Debug.Log($"Removed tracked pattern (pivot: {_patternsTrackedLastTurn[shipId].Pivot})");
                        _patterns[shipId].Remove(_patternsTrackedLastTurn[shipId]);
                        _patternsTrackedLastTurn.Remove(shipId);
                    }
                }
            }

            if (_shotsAtLastTurn != null)
            {
                if (totalDamage > 0)
                    FindPossiblePatterns();
                else
                    _missedShots.AddRange(_shotsAtLastTurn);
            }

            _playerShipsHealth = playerShipsHealth.ToList();

            UpdateProbabilityMap();

            void FindPossiblePatterns()
            {
                foreach (int shot in _shotsAtLastTurn)
                {
                    var shotCoordinate = GridUtils.CellIndexToCoordinate(shot, _rules.areaSize.x);
                    Debug.Log($"Trying patterns for shot at {shotCoordinate}");
                    foreach (int shipId in damagedShips)
                    {
                        if (IsShipSunk(shipId) || _isShipFound[shipId]) continue;
                        var ship = pool[shipId];
                        foreach (var shipPartCoordinate in ship.partCoordinates)
                        {
                            var pivot = shotCoordinate - (Vector3Int) shipPartCoordinate;
                            if (CanPatternBePlaced(pivot, ship))
                            {
                                Debug.Log($"{shipPartCoordinate} fits for ship {shipId}:{ship.name}");
                                if (!_patterns.ContainsKey(shipId)) _patterns.Add(shipId, new List<Pattern>());
                                if (!IsAlreadyInPatterns(shipId, pivot))
                                {
                                    _patterns[shipId].Add(new Pattern(ship, pivot, shipPartCoordinate));
                                }
                            }
                        }
                    }
                }
            }

            bool IsAlreadyInPatterns(int shipId, Vector3Int pivot)
            {
                return _patterns[shipId].Any(p => p.Pivot.Equals(pivot));
            }

            bool FindInPatterns(int shipId, out Pattern pattern)
            {
                pattern = new Pattern();
                if (!_patterns.ContainsKey(shipId)) return false;
                foreach (var p in _patterns[shipId])
                {
                    int counter = (from shipPartCoordinate in p.Ship.partCoordinates
                        select (Vector3Int) shipPartCoordinate + p.Pivot
                        into partCoordinate
                        select GridUtils.CoordinateToCellIndex(partCoordinate, _rules.areaSize)
                        into partCellIndex
                        select _shotsAtLastTurn.Count(shot => shot == partCellIndex)).Sum();

                    if (counter > 1)
                    {
                        pattern = p;
                        return true;
                    }
                }

                return false;
            }

            void RemoveOtherPatterns(int shipId, Pattern pattern)
            {
                _patterns[shipId].Clear();
                _patterns[shipId].Add(pattern);
                Debug.Log($"Removed all patterns except pattern with pivot: {pattern.Pivot} from ship: {shipId}");
            }

            bool IsShipSunk(int shipId)
            {
                return playerShipsHealth[shipId] <= 0;
            }
        }

        private bool IsMissedShot(int cellIndex)
        {
            return _missedShots.Any(missedShot => cellIndex == missedShot);
        }

        private bool CanPatternBePlaced(Vector3Int pivot, Ship ship)
        {
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            bool isInsideBoundaries = GridUtils.IsInsideBoundaries(shipWidth, shipHeight, pivot, _rules.areaSize);

            // No missed shots beneath the pattern
            bool isOnMissedShot = ship.partCoordinates.Select(partCoordinate => pivot + (Vector3Int) partCoordinate)
                .Any(coordinate => IsMissedShot(GridUtils.CoordinateToCellIndex(coordinate, _rules.areaSize)));

            // No certainly marked ships and its 1 unit margin beneath the pattern

            // No shots that did not hit this ship from the same group of shots in this turn

            return isInsideBoundaries && !isOnMissedShot;
        }
    }
}