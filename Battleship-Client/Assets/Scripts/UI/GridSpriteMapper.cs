using System.Collections.Generic;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public class GridSpriteMapper : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Tilemap tilemap;
        private readonly List<ShipPart> _opponentShipPart = new List<ShipPart>();
        private readonly Dictionary<int, List<Vector3Int>> _spritePositions = new Dictionary<int, List<Vector3Int>>();
        private void Awake()
        {
            InitializeStatusList();
            CacheSpritePositions();
        }

        private void InitializeStatusList()
        {
            foreach (var ship in gameManager.Ships)
                for (var i = 0; i < ship.amount; i++)
                    foreach (var coordinate in ship.PartCoordinates)
                        _opponentShipPart.Add(new ShipPart(-1, coordinate, ship.sprite.GetInstanceID()));
        }

        private void CacheSpritePositions()
        {
            foreach (var vector3Int in tilemap.cellBounds.allPositionsWithin)
            {
                var sprite = tilemap.GetSprite(vector3Int);
                if (sprite != null)
                {
                    int id = sprite.GetInstanceID();
                    if (!_spritePositions.ContainsKey(id))
                    {
                        _spritePositions.Add(sprite.GetInstanceID(), new List<Vector3Int> {vector3Int});
                    }
                    else
                    {
                        _spritePositions[sprite.GetInstanceID()].Add(vector3Int);
                    }
                }
            }
        }
        public IEnumerable<ShipPart> GetPartsList()
        {
            return new List<ShipPart>(_opponentShipPart);
        }
        public Dictionary<int, List<Vector3Int>> GetSpritePositions()
        {
            return new Dictionary<int, List<Vector3Int>>(_spritePositions);
        }

        public struct ShipPart
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
