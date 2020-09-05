using UnityEngine;

namespace BattleshipGame.Common
{
    public static class GridUtils
    {
        public static Vector3Int ToCoordinate(int cellIndex, int width)
        {
            return new Vector3Int(cellIndex % width, cellIndex / width, 0);
        }

        public static int ToCellIndex(Vector3Int coordinate, int width)
        {
            return coordinate.y * width + coordinate.x;
        }

        public static Vector3Int ScreenToCell(Vector3 screenPoint, Grid grid, Camera sceneCamera, Vector2Int areaSize)
        {
            var worldPoint = sceneCamera.ScreenToWorldPoint(screenPoint);
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