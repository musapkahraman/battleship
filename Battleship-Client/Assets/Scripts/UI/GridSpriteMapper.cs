using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Core;
using BattleshipGame.Scriptables;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    public class GridSpriteMapper : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Tilemap tilemap;

        private readonly Dictionary<int, List<Vector3Int>> _spritePositionsOnTileMap =
            new Dictionary<int, List<Vector3Int>>();

        private readonly Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();

        private void Awake()
        {
            CacheSpritePositions();
        }

        private void CacheSpritePositions()
        {
            foreach (var vector3Int in tilemap.cellBounds.allPositionsWithin)
            {
                var sprite = tilemap.GetSprite(vector3Int);
                if (!sprite) continue;
                int id = sprite.GetInstanceID();
                if (!_sprites.ContainsKey(id))
                    _sprites.Add(id, sprite);
                else
                    _sprites[id] = sprite;

                if (!_spritePositionsOnTileMap.ContainsKey(id))
                    _spritePositionsOnTileMap.Add(sprite.GetInstanceID(), new List<Vector3Int> {vector3Int});
                else
                    _spritePositionsOnTileMap[sprite.GetInstanceID()].Add(vector3Int);
            }
        }

        public void ChangeSpritePosition(Sprite sprite, Vector3Int oldPosition, Vector3Int newPosition)
        {
            int spriteId = sprite.GetInstanceID();

            if (!_sprites.ContainsKey(spriteId))
                _sprites.Add(spriteId, sprite);

            if (_spritePositionsOnTileMap.ContainsKey(spriteId))
            {
                var oldPositionsList = _spritePositionsOnTileMap[spriteId];
                oldPositionsList.Remove(oldPosition);
                oldPositionsList.Add(newPosition);
            }
            else
            {
                _spritePositionsOnTileMap.Add(spriteId, new List<Vector3Int> {newPosition});
            }
        }

        public void RemoveSpritePosition(int spriteId, Vector3Int oldPosition)
        {
            if (_spritePositionsOnTileMap.ContainsKey(spriteId))
                _spritePositionsOnTileMap[spriteId].Remove(oldPosition);
        }

        public Dictionary<int, List<Vector3Int>> GetSpritePositions()
        {
            return new Dictionary<int, List<Vector3Int>>(_spritePositionsOnTileMap);
        }

        public Sprite GetSpriteAt(ref Vector3Int position)
        {
            foreach (var keyValuePair in _spritePositionsOnTileMap)
            {
                int spriteId = keyValuePair.Key;
                var spritePositions = keyValuePair.Value;
                if (!_sprites.TryGetValue(spriteId, out var sprite)) continue;
                Ship ship = null;
                foreach (var s in gameManager.Ships)
                    if (s.sprite.Equals(sprite))
                        ship = s;

                if (ship is null) continue;
                foreach (var spritePosition in spritePositions)
                foreach (var cell in ship.PartCoordinates.Select(part => spritePosition + (Vector3Int) part))
                {
                    if (!cell.Equals(position)) continue;
                    _sprites.TryGetValue(spriteId, out var result);
                    position = spritePosition;
                    return result;
                }
            }

            return null;
        }
    }
}