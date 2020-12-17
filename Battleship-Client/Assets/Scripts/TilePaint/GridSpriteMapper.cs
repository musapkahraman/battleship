using System.Collections.Generic;
using System.Linq;
using BattleshipGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.TilePaint
{
    public class GridSpriteMapper : MonoBehaviour
    {
        [SerializeField] private Rules rules;
        [SerializeField] private Tilemap tilemap;

        private readonly Dictionary<int, List<Vector3Int>> _spritePositionsOnTileMap =
            new Dictionary<int, List<Vector3Int>>();

        private readonly Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();

        private void Awake()
        {
            CacheSpritePositions();
        }

        public void CacheSpritePositions()
        {
            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                var sprite = tilemap.GetSprite(position);
                if (!sprite) continue;
                int spriteId = sprite.GetInstanceID();
                if (!_sprites.ContainsKey(spriteId))
                    _sprites.Add(spriteId, sprite);
                else
                    _sprites[spriteId] = sprite;

                if (!_spritePositionsOnTileMap.ContainsKey(spriteId))
                    _spritePositionsOnTileMap.Add(spriteId, new List<Vector3Int> {position});
                else
                    _spritePositionsOnTileMap[spriteId].Add(position);
            }
        }

        public void ClearSpritePositions()
        {
            _sprites.Clear();
            _spritePositionsOnTileMap.Clear();
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
                foreach (var s in rules.ships.Where(s => s.tile.sprite.Equals(sprite))) ship = s;
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