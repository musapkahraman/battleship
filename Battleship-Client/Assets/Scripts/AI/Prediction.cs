using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class Prediction
    {
        private readonly SortedDictionary<int, List<Pattern>> _patterns = new SortedDictionary<int, List<Pattern>>();
        private readonly Dictionary<int, List<Probability>> _probabilityMap = new Dictionary<int, List<Probability>>();

        // private readonly List<int> _allShots = new List<int>();
        private readonly Rules _rules;
        private List<int> _playerShipsHealth = new List<int>(); // To figure out diffs on each Prediction.Update call.

        private List<int> _shotsAtLastTurn;

        public Prediction(Rules rules)
        {
            _rules = rules;
            InitProbabilityMap();
        }

        private void InitProbabilityMap()
        {
            int cellCount = _rules.areaSize.x * _rules.areaSize.y;
            var shipId = 0;
            foreach (var ship in _rules.ships)
            {
                float shipProbability = CalculateProbability(ship.partCoordinates.Count, cellCount);
                for (var i = 0; i < ship.amount; i++)
                {
                    var shipProbabilities = new List<Probability>();
                    for (var cell = 0; cell < cellCount; cell++)
                        shipProbabilities.Add(new Probability(cell, shipProbability));

                    _probabilityMap.Add(shipId, shipProbabilities);

                    // Initialize the local list to hold the health values of player ships. This is to figure out diffs.
                    _playerShipsHealth.Add(ship.partCoordinates.Count);

                    shipId++;
                }
            }
        }

        private static float CalculateProbability(int remainingShipPartCount, int availableCellCount)
        {
            return (float) remainingShipPartCount / availableCellCount;
        }

        private float GetProbabilityValue(int shipId, int cell)
        {
            return _probabilityMap[shipId].Single(p => p.Cell == cell).Value;
        }

        public List<int> GetMostProbableCells(IEnumerable<int> unmarkedCells, int size, IEnumerable<int> shipIds)
        {
            var cells = new List<int>();

            for (var shipId = 0; shipId < _playerShipsHealth.Count; shipId++)
            {
                if (cells.Count >= size) break;
                CheckOverShipPattern(shipId, cells);
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
                // _allShots.AddRange(_shotsAtLastTurn);
                return cells;
            }

            var orderedProbabilities = probabilities.OrderByDescending(p => p.Value);
            foreach (var probability in orderedProbabilities)
            {
                if (cells.Count >= size || probability.Value <= 0f) break;
                cells.Add(probability.Cell);
            }

            _shotsAtLastTurn = cells.ToList();
            // _allShots.AddRange(_shotsAtLastTurn);
            return cells;
        }

        private void CheckOverShipPattern(int shipId, ICollection<int> cells)
        {
            if (!_patterns.ContainsKey(shipId)) return;
            Debug.Log($"Ship {shipId} has {_patterns[shipId].Count} patterns.");
            foreach (var pattern in _patterns[shipId])
            foreach (var shipPart in pattern.Ship.partCoordinates)
            {
                var checkedParts = pattern.CheckedPartCoordinates.ToList();
                if (checkedParts.Any(checkedPart => !shipPart.Equals(checkedPart)))
                {
                    Debug.Log($"Shooting at pattern for ship {shipId}:{pattern.Ship.name} at {shipPart}");
                    cells.Add(GridUtils.CoordinateToCellIndex(pattern.Pivot + (Vector3Int) shipPart, _rules.areaSize));
                    pattern.CheckedPartCoordinates.Add(shipPart);
                    return;
                }
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
                    if (damage > 1) Debug.Log($"Ship {shipId} had multiple shots.");

                    if (playerShipsHealth[shipId] <= 0) Debug.Log($"Ship {shipId} was sunk.");
                }
            }

            if (totalDamage > 0) FindPossiblePatterns();

            _playerShipsHealth = playerShipsHealth.ToList();

            void FindPossiblePatterns()
            {
                if (_shotsAtLastTurn == null)
                {
                    Debug.Log("This is the first call.");
                    return;
                }

                foreach (int shot in _shotsAtLastTurn)
                {
                    var shotCoordinate = GridUtils.CellIndexToCoordinate(shot, _rules.areaSize.x);
                    Debug.Log($"Pattern try for shot at cell: {shot} -> {shotCoordinate}");
                    foreach (int shipId in damagedShips)
                    {
                        var ship = pool[shipId];
                        Debug.Log($"ship: {shipId}, {ship.name}");

                        foreach (var shipPartCoordinate in ship.partCoordinates)
                        {
                            var cellCoordinate = shotCoordinate - (Vector3Int) shipPartCoordinate;
                            if (CanPatternBePlaced(cellCoordinate, ship))
                            {
                                Debug.Log($"{shipPartCoordinate} fits.");
                                if (!_patterns.ContainsKey(shipId)) _patterns.Add(shipId, new List<Pattern>());
                                _patterns[shipId].Add(new Pattern(ship, cellCoordinate, shipPartCoordinate));
                            }
                        }
                    }
                }
            }
        }

        private bool CanPatternBePlaced(Vector3Int cellCoordinate, Ship ship)
        {
            (int shipWidth, int shipHeight) = ship.GetShipSize();
            bool dimensionCheck = GridUtils.DoesShipFitIn(shipWidth, shipHeight, cellCoordinate, _rules.areaSize);

            // No missed shots beneath the pattern
            // bool shotCellsCheck = false;
            //
            // foreach (int shot in _allShots)
            // {
            //     var shotCoordinate = GridUtils.CellIndexToCoordinate(shot, _rules.areaSize.x);
            //     foreach (var shipPartCoordinate in ship.partCoordinates)
            //     {
            //     var coordinate = shipPartCoordinate+
            //     
            //     }
            // }

            // No certainly marked ships and its 1 unit margin beneath the pattern

            // No shots that did not hit this ship from the same group of shots in this turn

            return dimensionCheck;
        }
    }
}