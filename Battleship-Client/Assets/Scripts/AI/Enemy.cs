using System;
using System.Collections;
using System.Collections.Generic;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleshipGame.AI
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Rules rules;
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
    }
}