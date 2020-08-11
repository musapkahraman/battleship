using System.Collections.Generic;
using UnityEngine;

namespace BattleshipGame
{
    [CreateAssetMenu]
    public class Ship : ScriptableObject
    {
        [Tooltip("Smallest number means the highest rank.")]
        public int sortOrder;

        public Sprite sprite;
        public List<Vector2> partCoordinates;
    }
}