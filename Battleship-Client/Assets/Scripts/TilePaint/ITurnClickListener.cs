using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public interface ITurnClickListener
    {
        void HighlightTurn(int turn);

        void HighlightShotsInTheSameTurn(Vector3Int coordinate);
    }
}