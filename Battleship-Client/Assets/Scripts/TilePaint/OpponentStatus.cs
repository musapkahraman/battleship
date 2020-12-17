using System.Collections.Generic;
using System.Linq;
using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public class OpponentStatus : MonoBehaviour
    {
        private const int NotShot = -1;
        [SerializeField] private GameObject maskPrefab;
        [SerializeField] private Rules rules;
        private readonly List<(int ShotTurn, Vector3Int Coordinate)> _shipParts = new List<(int, Vector3Int)>();

        private void Start()
        {
            var spritePositions = GetComponent<GridSpriteMapper>().GetSpritePositions();
            foreach (var ship1 in rules.ships)
                for (var i1 = 0; i1 < ship1.amount; i1++)
                    foreach (var partCoordinate1 in ship1.PartCoordinates.Select(coordinate =>
                        spritePositions[ship1.tile.sprite.GetInstanceID()][i1] + (Vector3Int) coordinate))
                        _shipParts.Add((NotShot, partCoordinate1));
        }

        public int GetShotTurn(Vector3Int coordinate)
        {
            foreach ((int shotTurn, var vector3Int) in _shipParts)
                if (vector3Int.Equals(coordinate))
                    return shotTurn;

            return NotShot;
        }

        public void DisplayShotEnemyShipParts(int changedShipPart, int shotTurn)
        {
            var part = _shipParts[changedShipPart];
            part.ShotTurn = shotTurn;
            _shipParts[changedShipPart] = part;
            var maskPositionInWorldSpace = transform.position + part.Coordinate + new Vector3(0.5f, 0.5f);
            Instantiate(maskPrefab, maskPositionInWorldSpace, Quaternion.identity);
        }
    }
}