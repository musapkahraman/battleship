using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(MapViewer), typeof(Grid))]
    public class CellLabelPrinter : MonoBehaviour
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int CellSize = 50;
        private static readonly Stack<GameObject> PrintedObjects = new Stack<GameObject>();
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject cellLabelPrefab;
        [SerializeField] private Axis axis;
        [SerializeField] private Direction direction;
        [SerializeField] private Place place;
        [SerializeField] private Type type;

#if UNITY_EDITOR
        public void Print()
        {
            if (cellLabelPrefab.GetComponent<TMP_Text>() == null)
            {
                Debug.LogError($"{nameof(cellLabelPrefab)} does not have a {nameof(TMP_Text)}!");
                return;
            }

            var mapViewer = GetComponent<MapViewer>();
            var grid = GetComponent<Grid>();

            var horizontalInterval = Vector2.right * CellSize * (grid.cellGap.x + grid.cellSize.x);
            var verticalInterval = Vector2.up * CellSize * (grid.cellGap.y + grid.cellSize.y);

            var interval = axis == Axis.Horizontal ? horizontalInterval : verticalInterval;
            var director = direction == Direction.Forward ? 1 : -1;

            Vector2 startingPoint;
            switch (place)
            {
                case Place.Start:
                    startingPoint = axis == Axis.Horizontal ? -verticalInterval : -horizontalInterval;
                    if (direction == Direction.Backward)
                        startingPoint += axis == Axis.Vertical
                            ? verticalInterval * (mapViewer.MapSize - 1)
                            : horizontalInterval * (mapViewer.MapSize - 1);

                    break;
                case Place.End:
                    startingPoint = axis == Axis.Horizontal
                        ? verticalInterval * mapViewer.MapSize
                        : horizontalInterval * mapViewer.MapSize;
                    if (direction == Direction.Backward)
                        startingPoint += axis == Axis.Vertical
                            ? verticalInterval * (mapViewer.MapSize - 1)
                            : horizontalInterval * (mapViewer.MapSize - 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (var i = 0; i < mapViewer.MapSize; i++)
            {
                var o = (GameObject) PrefabUtility.InstantiatePrefab(cellLabelPrefab, canvas.transform);
                o.name = axis + "CellLabel_" + i;
                o.GetComponent<RectTransform>().anchoredPosition = startingPoint + i * director * interval;
                o.GetComponent<TMP_Text>().text = type == Type.Digits ? (i + 1).ToString() : Alphabet[i].ToString();
                PrintedObjects.Push(o);
            }
        }

        public void Undo()
        {
            var mapViewer = GetComponent<MapViewer>();
            if (PrintedObjects.Count <= 0) return;
            for (var i = 0; i < mapViewer.MapSize; i++) DestroyImmediate(PrintedObjects.Pop());
        }
#endif

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
    }
}