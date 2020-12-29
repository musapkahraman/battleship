using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleshipGame.Core
{
    [CreateAssetMenu(fileName = "New Rules", menuName = "Battleship/Rules", order = 1)]
    public class Rules : ScriptableObject
    {
        public int shotsPerTurn = 3;

        [Tooltip("Add the types of ships only. Amounts and the sorting order are determined by the ship itself.")]
        public List<Ship> ships;

        public Vector2Int areaSize = new Vector2Int(9, 9);

        private void OnValidate()
        {
            var hashSet = new HashSet<Ship>();
            foreach (var ship in ships) hashSet.Add(ship);

            ships = hashSet.OrderBy(ship => ship.rankOrder).ToList();
        }
    }
}