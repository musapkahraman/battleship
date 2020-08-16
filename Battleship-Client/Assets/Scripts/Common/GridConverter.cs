using UnityEngine;

namespace BattleshipGame.Common
{
    public static class GridConverter
    {
        public static Vector3Int ToCoordinate(int cellIndex, int mapSize)
        {
            return new Vector3Int(cellIndex % mapSize, cellIndex / mapSize, 0);
        }

        public static int ToCellIndex(Vector3Int coordinate, int size)
        {
            return coordinate.y * size + coordinate.x;
        }

        public static Vector3Int ScreenToCell(Vector3 screenPoint, Grid grid, Camera sceneCamera, int areaSize)
        {
            var worldPoint = sceneCamera.ScreenToWorldPoint(screenPoint);
            var cell = grid.WorldToCell(worldPoint);
            cell.Clamp(new Vector3Int(0, 0, 0), new Vector3Int(areaSize - 1, areaSize - 1, 0));
            return cell;
        }
    }
}