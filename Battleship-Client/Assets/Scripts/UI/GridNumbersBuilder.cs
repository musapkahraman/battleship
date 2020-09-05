using System;
using System.Collections.Generic;
using BattleshipGame.ScriptableObjects;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(Grid))]
    public class GridNumbersBuilder : MonoBehaviour
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int CellSize = 50;
        private static readonly Stack<GameObject> PrintedObjects = new Stack<GameObject>();
        [SerializeField] private Axis axis;
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject cellLabelPrefab;
        [SerializeField] private Direction direction;
        [SerializeField] private Rules rules;
        [SerializeField] private Place place;
        [SerializeField] private Type type;
        private Vector2Int _mapSize;

        private enum Axis
        {
            Horizontal,
            Vertical
        }

        private enum Place
        {
            Start,
            End
        }

        private enum Direction
        {
            Forward,
            Backward
        }

        private enum Type
        {
            Digits,
            Letters
        }

#if UNITY_EDITOR
        public void Print()
        {
            _mapSize = rules.AreaSize;

            if (cellLabelPrefab.GetComponent<TMP_Text>() == null)
            {
                Debug.LogError($"{nameof(cellLabelPrefab)} does not have a {nameof(TMP_Text)}!");
                return;
            }

            var grid = GetComponent<Grid>();

            var horizontalInterval = Vector2.right * CellSize * (grid.cellGap.x + grid.cellSize.x);
            var verticalInterval = Vector2.up * CellSize * (grid.cellGap.y + grid.cellSize.y);

            var interval = axis == Axis.Horizontal ? horizontalInterval : verticalInterval;
            int director = direction == Direction.Forward ? 1 : -1;

            Vector2 startingPoint;
            switch (place)
            {
                case Place.Start:
                    startingPoint = axis == Axis.Horizontal ? -verticalInterval : -horizontalInterval;
                    if (direction == Direction.Backward)
                        startingPoint += axis == Axis.Vertical
                            ? verticalInterval * (_mapSize.y - 1)
                            : horizontalInterval * (_mapSize.x - 1);

                    break;
                case Place.End:
                    startingPoint = axis == Axis.Horizontal
                        ? verticalInterval * _mapSize
                        : horizontalInterval * _mapSize;
                    if (direction == Direction.Backward)
                        startingPoint += axis == Axis.Vertical
                            ? verticalInterval * (_mapSize.y - 1)
                            : horizontalInterval * (_mapSize.x - 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int size = axis == Axis.Horizontal ? _mapSize.x : _mapSize.y;
            for (var i = 0; i < size; i++)
            {
                var label = (GameObject) PrefabUtility.InstantiatePrefab(cellLabelPrefab, canvas.transform);
                label.name = axis + "CellLabel_" + i;
                label.GetComponent<RectTransform>().anchoredPosition = startingPoint + i * director * interval;
                label.GetComponent<TMP_Text>().text = type == Type.Digits ? (i + 1).ToString() : Alphabet[i].ToString();
                PrintedObjects.Push(label);
            }
        }

        public void Undo()
        {
            if (PrintedObjects.Count <= 0) return;
            int size = axis == Axis.Horizontal ? _mapSize.x : _mapSize.y;
            for (var i = 0; i < size; i++) DestroyImmediate(PrintedObjects.Pop());
        }
#endif
    }
}