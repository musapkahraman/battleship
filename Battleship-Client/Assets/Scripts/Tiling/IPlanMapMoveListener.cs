using BattleshipGame.Core;
using UnityEngine;

namespace BattleshipGame.Tiling
{
    public interface IPlanMapMoveListener
    {
        bool OnShipMoved(Ship ship, Vector3Int from, Vector3Int to, bool isMovedIn);
    }
}