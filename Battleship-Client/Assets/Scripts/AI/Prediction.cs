using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class Prediction
    {
        private readonly List<int> _allShots = new List<int>();
        private readonly List<int> _missedShots = new List<int>();
        private readonly List<int> _noFireZone = new List<int>();
        private readonly SortedDictionary<int, List<Pattern>> _patterns = new SortedDictionary<int, List<Pattern>>();
        private readonly Dictionary<int, Pattern> _patternsTrackedLastTurn = new Dictionary<int, Pattern>();
        private readonly List<int> _playerShipsHealthAtStart;
        private readonly Dictionary<int, List<Probability>> _probabilityMap = new Dictionary<int, List<Probability>>();
        private readonly Rules _rules;
        private readonly Dictionary<int, List<int>> _shotsWithHits = new Dictionary<int, List<int>>();
        private bool _isAnyHitOccuredInLastTurn;
        private List<int> _playerShipsHealthAtLastTurn; // To figure out diffs on each Prediction.Update call.
        private List<int> _shotsAtLastTurn = new List<int>();
        private readonly List<int> _uncheckedCells;

        public Prediction(Rules rules, List<int> uncheckedCells, IReadOnlyCollection<int> playerShipsHealth)
        {
            _rules = rules;
            _uncheckedCells = uncheckedCells;
            _playerShipsHealthAtStart = playerShipsHealth.ToList();
            _playerShipsHealthAtLastTurn = playerShipsHealth.ToList();
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
                                p = CalculateProbability(_playerShipsHealthAtLastTurn[shipId],
                                    numberOfFittingCellsForShip);
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
                    CanPatternBePlaced(cell - (Vector3Int) shipPartCoordinate, ship, shipId));
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

        public List<int> GetMostProbableCells(int size, IEnumerable<int> shipIds)
        {
            foreach (int cell in _noFireZone) _uncheckedCells.Remove(cell);

            // if (_uncheckedCells.Count == 0) _uncheckedCells.AddRange(_noFireZone.ToList());

            if (_isAnyHitOccuredInLastTurn)
            {
                _isAnyHitOccuredInLastTurn = false;
            }
            else
            {
                _missedShots.AddRange(_shotsAtLastTurn.ToList());
                for (var shipId = 0; shipId < _playerShipsHealthAtStart.Count; shipId++)
                    RemoveTrackedPatternsLastTurn(shipId);
            }

            UpdateProbabilityMap();

            var cells = new List<int>();
            _patternsTrackedLastTurn.Clear();
            for (var shipId = 0; shipId < _playerShipsHealthAtStart.Count; shipId++)
            {
                if (cells.Count >= size) break;
                CheckOverShipPattern(shipId);
            }

            var probabilities = (from cell in _uncheckedCells
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
                _allShots.AddRange(_shotsAtLastTurn.ToList());
                Debug.Log(cells.Count);
                return cells;
            }

            var orderedProbabilities = probabilities.OrderByDescending(p => p.Value);
            foreach (var probability in orderedProbabilities)
            {
                if (cells.Count >= size || probability.Value <= 0f) break;
                cells.Add(probability.Cell);
            }

            _shotsAtLastTurn = cells.ToList();
            _allShots.AddRange(_shotsAtLastTurn.ToList());
            Debug.Log(cells.Count);
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
                            if (_patternsTrackedLastTurn.ContainsKey(shipId))
                                _patternsTrackedLastTurn[shipId] = pattern;
                            else
                                _patternsTrackedLastTurn.Add(shipId, pattern);

                            if (_patterns[shipId].Count != 1 || cells.Count >= size) return;
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

        // Prediction.Update is only called if any ship is shot.
        public void Update(List<int> playerShipsHealth, SortedDictionary<int, Ship> pool)
        {
            _isAnyHitOccuredInLastTurn = true;
            for (var shipId = 0; shipId < playerShipsHealth.Count; shipId++)
            {
                int turnDamage = _playerShipsHealthAtLastTurn[shipId] - playerShipsHealth[shipId];
                if (turnDamage > 0)
                {
                    if (IsShipSunk(shipId))
                    {
                        Debug.Log($"Ship {shipId} is sunk.");
                        if (_patterns.ContainsKey(shipId))
                        {
                            _patterns.Remove(shipId);
                            Debug.Log($"Removed all patterns from ship: {shipId}");
                        }

                        if (_patternsTrackedLastTurn.ContainsKey(shipId))
                        {
                            var pattern = _patternsTrackedLastTurn[shipId];
                            MarkPerimeterAsNoFireZone(pattern);
                        }

                        continue;
                    }

                    Debug.Log($"Ship {shipId} was damaged {turnDamage} units.");

                    if (!_shotsWithHits.ContainsKey(shipId)) _shotsWithHits.Add(shipId, new List<int>());
                    _shotsWithHits[shipId].AddRange(_shotsAtLastTurn.ToList());

                    if (_patterns.ContainsKey(shipId) && IsShipMultipleDamaged(shipId))
                    {
                        Debug.Log($"Ship {shipId} has multiple damage.");
                        var patterns = GetPatternsWithHits(shipId);
                        if (patterns.Count > 0) RemoveOtherPatterns(shipId, patterns);
                    }
                    else
                    {
                        FindPossiblePatterns(shipId);
                    }
                }
                else
                {
                    // No shots on this ship. Remove any tracked pattern intervening with this shot.
                    RemoveTrackedPatternsLastTurn(shipId);
                }
            }

            _playerShipsHealthAtLastTurn = playerShipsHealth.ToList();

            void FindPossiblePatterns(int shipId)
            {
                foreach (int shot in _shotsAtLastTurn)
                {
                    var shotCoordinate = GridUtils.CellIndexToCoordinate(shot, _rules.areaSize.x);
                    var ship = pool[shipId];
                    foreach (var shipPartCoordinate in ship.partCoordinates)
                    {
                        var pivot = shotCoordinate - (Vector3Int) shipPartCoordinate;
                        if (CanPatternBePlaced(pivot, ship, shipId))
                        {
                            Debug.Log($"<color=green>{shipPartCoordinate} fits for ship {shipId}:{ship.name}</color>");
                            if (!_patterns.ContainsKey(shipId)) _patterns.Add(shipId, new List<Pattern>());
                            if (!IsAlreadyInPatterns(shipId, pivot))
                                _patterns[shipId].Add(new Pattern(ship, pivot, shipPartCoordinate));
                        }
                    }
                }
            }

            bool IsAlreadyInPatterns(int shipId, Vector3Int pivot)
            {
                return _patterns[shipId].Any(p => p.Pivot.Equals(pivot));
            }

            List<Pattern> GetPatternsWithHits(int shipId)
            {
                var result = new List<Pattern>();
                result.AddRange(from pattern in _patterns[shipId]
                    let counter = (from shipPart in pattern.Ship.partCoordinates
                        select (Vector3Int) shipPart + pattern.Pivot
                        into partCoordinate
                        select GridUtils.CoordinateToCellIndex(partCoordinate, _rules.areaSize)
                        into partCellIndex
                        select _shotsWithHits[shipId].Count(shot => shot == partCellIndex)).Sum()
                    where counter > 1
                    select pattern);

                return result;
            }

            void RemoveOtherPatterns(int shipId, IEnumerable<Pattern> patterns)
            {
                _patterns[shipId].Clear();
                Debug.Log($"Removed all patterns from ship {shipId} except...");
                foreach (var pattern in patterns)
                {
                    if (CanPatternBePlaced(pattern.Pivot, pattern.Ship, shipId))
                    {
                        _patterns[shipId].Add(pattern);
                        Debug.Log($"...pattern with pivot: {pattern.Pivot}");
                    }
                }
            }

            bool IsShipSunk(int shipId)
            {
                return playerShipsHealth[shipId] <= 0;
            }

            bool IsShipMultipleDamaged(int shipId)
            {
                return playerShipsHealth[shipId] < _playerShipsHealthAtStart[shipId] - 1;
            }

            void MarkPerimeterAsNoFireZone(Pattern pattern)
            {
                (int width, int height) = pattern.Ship.GetShipSize();
                for (var x = 0; x < width + 2; x++)
                for (var y = 0; y < height + 2; y++)
                {
                    var coordinate = pattern.Pivot + new Vector3Int(x - 1, 1 - y, 0);
                    int cell = GridUtils.CoordinateToCellIndex(coordinate, _rules.areaSize);
                    if (cell != GridUtils.OutOfMap)
                    {
                        _noFireZone.Add(cell);
                        for (var shipId = 0; shipId < pool.Count; shipId++)
                            if (_shotsWithHits.ContainsKey(shipId))
                                _shotsWithHits[shipId].Remove(cell);
                    }
                }
            }
        }

        private void RemoveTrackedPatternsLastTurn(int shipId)
        {
            if (_patterns.ContainsKey(shipId) && _patternsTrackedLastTurn.ContainsKey(shipId))
            {
                Debug.Log($"Removed tracked pattern (pivot: {_patternsTrackedLastTurn[shipId].Pivot})");
                _patterns[shipId].Remove(_patternsTrackedLastTurn[shipId]);
            }
        }

        private bool IsMissedShot(int cellIndex)
        {
            return _missedShots.Any(missedShot => cellIndex == missedShot);
        }

        private bool IsUncheckedCell(int cellIndex)
        {
            return _uncheckedCells.Any(uncheckedCell => uncheckedCell == cellIndex);
        }

        private bool IsRelevantCell(int cellIndex, int shipId)
        {
            return _shotsWithHits.ContainsKey(shipId) && _shotsWithHits[shipId].Any(shot => cellIndex == shot);
        }

        private bool CanPatternBePlaced(Vector3Int pivot, Ship ship, int shipId)
        {
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            bool isInsideBoundaries = GridUtils.IsInsideBoundaries(shipWidth, shipHeight, pivot, _rules.areaSize);

            bool isOnMissedShot = ship.partCoordinates
                .Select(partCoordinate => pivot + (Vector3Int) partCoordinate)
                .Select(coordinate => GridUtils.CoordinateToCellIndex(coordinate, _rules.areaSize))
                .Any(IsMissedShot);

            bool isOnRelevantOrEmptyCells = ship.partCoordinates
                .Select(partCoordinate => pivot + (Vector3Int) partCoordinate)
                .Select(coordinate => GridUtils.CoordinateToCellIndex(coordinate, _rules.areaSize)).All(cellIndex =>
                    IsUncheckedCell(cellIndex) || IsRelevantCell(cellIndex, shipId));

            return isInsideBoundaries && !isOnMissedShot && isOnRelevantOrEmptyCells;
        }
    }
}