using BattleshipGame.Core;
using UnityEngine;

namespace BattleshipGame.AI
{
    public struct Pattern
    {
        public Ship Ship;
        public Vector2Int ShotPartOfShip;
        public int ShotCell;
        public Vector3Int ShotCellCoordinate;

        public Pattern(Ship ship, Vector2Int shotPartOfShip, int shotCell, Vector3Int shotCellCoordinate)
        {
            Ship = ship;
            ShotPartOfShip = shotPartOfShip;
            ShotCell = shotCell;
            ShotCellCoordinate = shotCellCoordinate;
        }
    }
}