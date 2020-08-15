using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public class OpponentStatusMaskPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject maskPrefab;
        [SerializeField] private List<Ship> ships;
        private readonly List<ShipPart> _opponentShipPart = new List<ShipPart>();
        private readonly Dictionary<int, Stack<Vector3Int>> _spritePositions = new Dictionary<int, Stack<Vector3Int>>();

        private void Start()
        {
            InitializeStatusList();
            CacheSpritePositions();
        }

        private void InitializeStatusList()
        {
            var rankOrderedShips = ships.OrderBy(ship => ship.rankOrder);
            foreach (var ship in rankOrderedShips)
                for (var i = 0; i < ship.amount; i++)
                    foreach (var coordinate in ship.PartCoordinates)
                        _opponentShipPart.Add(new ShipPart(-1, coordinate, ship.sprite.GetInstanceID()));
        }

        private void CacheSpritePositions()
        {
            var tileMap = GetComponentInChildren<Tilemap>();
            foreach (var vector3Int in tileMap.cellBounds.allPositionsWithin)
            {
                var sprite = tileMap.GetSprite(vector3Int);
                if (sprite != null)
                {
                    int id = sprite.GetInstanceID();
                    if (!_spritePositions.ContainsKey(id))
                    {
                        var stack = new Stack<Vector3Int>();
                        stack.Push(vector3Int);
                        _spritePositions.Add(sprite.GetInstanceID(), stack);
                    }
                    else
                    {
                        _spritePositions[sprite.GetInstanceID()].Push(vector3Int);
                    }
                }
            }
        }

        private Vector3 GetPosition(int changedShipPart, int status)
        {
            var part = _opponentShipPart[changedShipPart];
            part.Status = status;
            _opponentShipPart[changedShipPart] = part;

            var spritePositionStack = _spritePositions[part.SpriteId];
            var position = spritePositionStack.Count > 1 ? spritePositionStack.Pop() : spritePositionStack.Peek();
            return transform.position + position + (Vector3) part.Coordinate + new Vector3(0.5f, 0.5f);
        }

        public void PlaceMask(int changedShipPart, int status)
        {
            Instantiate(maskPrefab, GetPosition(changedShipPart, status), Quaternion.identity);
        }

        private struct ShipPart
        {
            public int Status;
            public readonly Vector2 Coordinate;
            public readonly int SpriteId;

            public ShipPart(int status, Vector2 coordinate, int spriteId)
            {
                Status = status;
                Coordinate = coordinate;
                SpriteId = spriteId;
            }
        }
    }
}