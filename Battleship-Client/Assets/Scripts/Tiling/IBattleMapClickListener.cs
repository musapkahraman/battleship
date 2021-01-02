using UnityEngine;

namespace BattleshipGame.Tiling
{
    public interface IBattleMapClickListener
    {
        void OnOpponentMapClicked(Vector3Int cell);
    }
}