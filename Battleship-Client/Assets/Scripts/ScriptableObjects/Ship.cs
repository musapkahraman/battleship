using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Ship", menuName = "Battleship/Ship", order = 0)]
    public class Ship : ScriptableObject
    {
        public Tile tile;

        [Tooltip("Smallest number means the highest rank.")]
        public int rankOrder;

        [Tooltip("How many of this ship does each player have?")]
        public int amount;

        [Tooltip("Start with the sprite's pivot. First value must be (0, 0)")]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public List<Vector2Int> PartCoordinates = new List<Vector2Int>();

        private void OnValidate()
        {
            if (PartCoordinates.Count == 0)
                PartCoordinates.Add(Vector2Int.zero);
            else
                PartCoordinates[0] = Vector2Int.zero;
        }

        public (int width, int height) GetShipSize()
        {
            var minX = 0;
            var maxX = 0;
            var minY = 0;
            var maxY = 0;

            foreach (var partCoordinate in PartCoordinates)
            {
                minX = Math.Min(minX, partCoordinate.x);
                maxX = Math.Max(maxX, partCoordinate.x);
                minY = Math.Min(minY, partCoordinate.y);
                maxY = Math.Max(maxY, partCoordinate.y);
            }

            return (maxX - minX + 1, maxY - minY + 1);
        }
    }
}