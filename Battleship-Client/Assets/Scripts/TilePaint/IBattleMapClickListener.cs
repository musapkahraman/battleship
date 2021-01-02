using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public interface IBattleMapClickListener
    {
        void OnOpponentMapClicked(Vector3Int cell);
    }
}