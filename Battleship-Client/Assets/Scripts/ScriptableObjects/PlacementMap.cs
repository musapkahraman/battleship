using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleshipGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Placement Map", menuName = "Battleship/Placement Map", order = 3)]
    public class PlacementMap : ScriptableObject
    {
        public List<Placement> placements = new List<Placement>();

        [Serializable]
        public struct Placement
        {
            public Ship ship;
            public Vector3Int Coordinate;
        }
    }
}