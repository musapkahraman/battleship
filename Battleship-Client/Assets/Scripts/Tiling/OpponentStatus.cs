using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using UnityEngine;

namespace BattleshipGame.Tiling
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
            foreach (var ship in rules.ships)
                for (var i = 0; i < ship.amount; i++)
                    foreach (var partCoordinate in ship.partCoordinates.Select(coordinate =>
                        spritePositions[ship.tile.sprite.GetInstanceID()][i] + (Vector3Int) coordinate))
                        _shipParts.Add((NotShot, partCoordinate));
        }

        public int GetShotTurn(Vector3Int coordinate)
        {
            foreach ((int shotTurn, var vector3Int) in _shipParts)
                if (vector3Int.Equals(coordinate))
                    return shotTurn;

            return NotShot;
        }

        public List<Vector3Int> GetCoordinates(int turn)
        {
            var result = new List<Vector3Int>();
            foreach ((int shotTurn, var vector3Int) in _shipParts)
                if (shotTurn == turn)
                    result.Add(vector3Int);
            return result;
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