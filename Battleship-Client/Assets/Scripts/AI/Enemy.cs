using System.Collections.Generic;
using UnityEngine;

namespace BattleshipGame.AI
{
    public class Enemy
    {
        private readonly List<int> _uncheckedCells = new List<int>();

        public Enemy(int totalCellCount)
        {
            for (var i = 0; i < totalCellCount; i++)
            {
                _uncheckedCells.Add(i);
            }
        }

        public int[] GetRandomCells(int size)
        {
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
    }
}