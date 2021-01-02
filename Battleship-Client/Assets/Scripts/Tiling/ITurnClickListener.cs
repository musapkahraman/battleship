using UnityEngine;

namespace BattleshipGame.Tiling
{
    public interface ITurnClickListener
    {
        void HighlightTurn(int turn);

        void HighlightShotsInTheSameTurn(Vector3Int coordinate);
    }
}