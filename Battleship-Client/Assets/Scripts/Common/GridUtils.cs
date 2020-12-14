using UnityEngine;

namespace BattleshipGame.Common
{
    public static class GridUtils
    {
        public const int OutOfMap = -1;

        public static int CoordinateToCellIndex(Vector3Int coordinate, Vector2Int areaSize)
        {
            int cellIndex = coordinate.y * areaSize.x + coordinate.x;
            if (cellIndex >= 0 && cellIndex < areaSize.x * areaSize.y) return cellIndex;
            Debug.LogError($"Out of range! Input: {coordinate} | Output: {cellIndex}");
            return OutOfMap;
        }

        public static Vector3Int CellIndexToCoordinate(int cellIndex, int width)
        {
            return new Vector3Int(cellIndex % width, cellIndex / width, 0);
        }

        public static Vector3Int ScreenToCell(Vector3 input, Camera sceneCamera, Grid grid, Vector2Int areaSize)
        {
            var worldPoint = sceneCamera.ScreenToWorldPoint(input);
            return WorldToCell(worldPoint, sceneCamera, grid, areaSize);
        }

        public static Vector3Int WorldToCell(Vector3 worldPoint, Camera sceneCamera, Grid grid, Vector2Int areaSize)
        {
            var cell = grid.WorldToCell(worldPoint);
            cell.Clamp(new Vector3Int(0, 0, 0), new Vector3Int(areaSize.x - 1, areaSize.y - 1, 0));
            return cell;
        }

        public static bool DoesShipFitIn(int shipWidth, int shipHeight, Vector3Int cellCoordinate,
            float horizontalAreaSize)
        {
            return cellCoordinate.x >= 0 && cellCoordinate.x + shipWidth <= horizontalAreaSize &&
                   cellCoordinate.y - (shipHeight - 1) >= 0;
        }
    }
}