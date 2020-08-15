﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame
{
    [CreateAssetMenu]
    public class Ship : ScriptableObject
    {
        public Sprite sprite;
        public Tile tile;

        [Tooltip("Start with the sprite's pivot. First value must be (0, 0)")]
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public List<Vector2Int> PartCoordinates = new List<Vector2Int>();

        [Tooltip("Smallest number means the highest rank.")]
        public int rankOrder;

        [Tooltip("How many of this ship does each player have?")]
        public int amount;

        private void OnValidate()
        {
            var origin = Vector2Int.zero;
            if (PartCoordinates.Count == 0)
                PartCoordinates.Add(origin);
            else
                PartCoordinates[0] = origin;
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