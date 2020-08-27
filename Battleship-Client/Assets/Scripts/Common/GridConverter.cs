using UnityEngine;

namespace BattleshipGame.Common
{
    public static class GridConverter
    {
        public static Vector3Int ToCoordinate(int cellIndex, Vector2Int areaSize)
        {
            return new Vector3Int(cellIndex % areaSize.x, cellIndex / areaSize.y, 0);
        }

        public static int ToCellIndex(Vector3Int coordinate, int height)
        {
            return coordinate.y * height + coordinate.x;
        }

        public static Vector3Int ScreenToCell(Vector3 screenPoint, Grid grid, Camera sceneCamera, Vector2Int areaSize)
        {
            var worldPoint = sceneCamera.ScreenToWorldPoint(screenPoint);
            var cell = grid.WorldToCell(worldPoint);
            cell.Clamp(new Vector3Int(0, 0, 0), new Vector3Int(areaSize.x - 1, areaSize.y - 1, 0));
            return cell;
        }
    }
}