using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleshipGame.Core
{
    [CreateAssetMenu(fileName = "NewPlacementMap", menuName = "Battleship/Placement Map", order = 3)]
    public class PlacementMap : ScriptableObject
    {
        [SerializeField] private List<Placement> placements = new List<Placement>();

        public void PlaceShip(int shipIndex, Ship ship, Vector3Int coordinate)
        {
            int index = -1;
            for (var i = 0; i < placements.Count; i++)
            {
                if (!placements[i].shipId.Equals(shipIndex)) continue;
                index = i;
                break;
            }

            if (index > -1)
            {
                var placement = placements[index];
                placement.Coordinate = coordinate;
                placements[index] = placement;
            }
            else
            {
                placements.Add(new Placement(shipIndex, ship, coordinate));
            }
        }
        
        public List<Placement> GetPlacements()
        {
            return placements.ToList();
        }

        public void Clear()
        {
            placements.Clear();
        }

        [Serializable]
        public struct Placement
        {
            public int shipId;
            public Ship ship;
            public Vector3Int Coordinate;

            public Placement(int shipId, Ship ship, Vector3Int coordinate)
            {
                this.shipId = shipId;
                this.ship = ship;
                Coordinate = coordinate;
            }
        }
    }
}