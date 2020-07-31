using UnityEngine;

namespace BattleshipGame.Common
{
    public static class Converter
    {
        public static Vector3Int ToCoordinate(int cellIndex, int mapSize)
        {
            return new Vector3Int(cellIndex % mapSize, cellIndex / mapSize, 0);
        }

        public static int ToCellIndex(Vector3Int coordinate, int size)
        {
            return coordinate.y * size + coordinate.x;
        }
    }
}