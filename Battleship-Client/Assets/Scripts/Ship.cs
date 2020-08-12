using System.Collections.Generic;
using UnityEngine;

namespace BattleshipGame
{
    [CreateAssetMenu]
    public class Ship : ScriptableObject
    {
        public Sprite sprite;
        
        [Tooltip("Start with the sprite's pivot.")]
        public List<Vector2> partCoordinates;

        [Tooltip("How many of this ship does each player have?")]
        public int amount;

        [Tooltip("Smallest number means the highest rank.")]
        public int rankOrder;
    }
}