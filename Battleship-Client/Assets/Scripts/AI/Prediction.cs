using System.Collections.Generic;
using System.Linq;
using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class Prediction
    {
        private readonly Dictionary<int, List<Probability>> _probabilityMap = new Dictionary<int, List<Probability>>();

        public Prediction(Rules rules)
        {
            InitProbabilityMap(rules);
        }

        private void InitProbabilityMap(Rules rules)
        {
            int cellCount = rules.areaSize.x * rules.areaSize.y;
            var shipId = 0;
            foreach (var ship in rules.ships)
            {
                float shipProbability = CalculateProbability(ship.PartCoordinates.Count, cellCount);
                for (var i = 0; i < ship.amount; i++)
                {
                    var shipProbabilities = new List<Probability>();
                    for (var cell = 0; cell < cellCount; cell++)
                        shipProbabilities.Add(new Probability(cell, shipProbability));

                    _probabilityMap.Add(shipId, shipProbabilities);
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

                return cells;
            }

            var orderedProbabilities = probabilities.OrderByDescending(p => p.Value);
            foreach (var probability in orderedProbabilities)
            {
                if (cells.Count >= size || probability.Value <= 0f) break;
                cells.Add(probability.Cell);
            }

            return cells;
        }
    }
}