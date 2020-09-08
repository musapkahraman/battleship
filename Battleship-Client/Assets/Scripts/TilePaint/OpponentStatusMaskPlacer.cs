using System.Collections.Generic;
using BattleshipGame.Common;
using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public class OpponentStatusMaskPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject maskPrefab;
        [SerializeField] private Rules rules;
        private readonly List<ShipPart> _shipParts = new List<ShipPart>();
        private readonly Dictionary<int, Stack<Vector3Int>> _spritePositions = new Dictionary<int, Stack<Vector3Int>>();
        private GridSpriteMapper _spriteMapper;

        private void Start()
        {
            _spriteMapper = GetComponent<GridSpriteMapper>();
            InitializeStatusList();
            foreach (var spritePosition in _spriteMapper.GetSpritePositions())
                _spritePositions.Add(spritePosition.Key, spritePosition.Value.CloneToStack());
        }

        private void InitializeStatusList()
        {
            foreach (var ship in rules.ships)
                for (var i = 0; i < ship.amount; i++)
                    foreach (var coordinate in ship.PartCoordinates)
                        _shipParts.Add(new ShipPart(-1, coordinate, ship.tile.sprite.GetInstanceID()));
        }

        private Vector3 GetPosition(int changedShipPart, int shotTurn)
        {
            var part = _shipParts[changedShipPart];
            part.ShotTurn = shotTurn;
            _shipParts[changedShipPart] = part;

            var spritePositionStack = _spritePositions[part.SpriteId];
            var position = spritePositionStack.Count > 1 ? spritePositionStack.Pop() : spritePositionStack.Peek();
            return transform.position + position + (Vector3) part.Coordinate + new Vector3(0.5f, 0.5f);
        }

        public void PlaceMask(int changedShipPart, int shotTurn)
        {
            Instantiate(maskPrefab, GetPosition(changedShipPart, shotTurn), Quaternion.identity);
        }

        public int GetShotTurn(Vector3Int coordinate)
        {
            return 1;
        }

        private struct ShipPart
        {
            public int ShotTurn;
            public readonly Vector2 Coordinate;
            public readonly int SpriteId;

            public ShipPart(int shotTurn, Vector2 coordinate, int spriteId)
            {
                ShotTurn = shotTurn;
                Coordinate = coordinate;
                SpriteId = spriteId;
            }
        }
    }
}