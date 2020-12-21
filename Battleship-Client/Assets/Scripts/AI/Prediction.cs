using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class Prediction
    {
        private readonly Dictionary<int, List<Probability>> _probabilityMap = new Dictionary<int, List<Probability>>();

        private float CalculateProbability(int notShotPartsOfShip, int availableCellCountForShip)
        {
            return (float) notShotPartsOfShip / availableCellCountForShip;
        }

        private float GetProbabilityValue()
        {
            return _probabilityMap[0].Single(p => p.Cell == 1).Value;
        }

        private List<int> GetMostProbableCells(int cellCount, int shipId)
        {
            var cells = new List<int>();
            var probabilities = _probabilityMap[shipId];

            // If the probabilities of finding a ship in multiple cells are at most, select them randomly.
            float max = probabilities.Max(p => p.Value);
            int count = probabilities.Count(p => Mathf.Approximately(p.Value, max));
            if (count > cellCount)
            {
                var uncheckedCells = probabilities
                    .Where(p => Mathf.Approximately(p.Value, max))
                    .Select(p => p.Cell)
                    .ToList();
                for (var i = 0; i < cellCount; i++)
                {
                    int index = Random.Range(0, uncheckedCells.Count);
                    if (cells.Count >= cellCount) break;
                    cells.Add(uncheckedCells[index]);
                    uncheckedCells.Remove(cells[i]);
                }

                return cells;
            }

            var orderedProbabilities = probabilities.OrderByDescending(p => p.Value);
            foreach (var probability in orderedProbabilities)
            {
                if (cells.Count >= cellCount || probability.Value <= 0f) break;
                cells.Add(probability.Cell);
            }

            return cells;
        }
    }
}