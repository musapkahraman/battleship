using System.Collections.Generic;
using BattleshipGame.UI;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.TilePaint
{
    public class Pool : MonoBehaviour
    {
        [SerializeField] private ButtonController clearButton;
        [SerializeField] private Tilemap tilemap;
        private readonly Dictionary<Vector3Int, TileBase> _cache = new Dictionary<Vector3Int, TileBase>();

        private void Start()
        {
            clearButton.AddListener(ResetPlacementMap);
            foreach (var coordinate in tilemap.cellBounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(coordinate)) continue;
                if (_cache.ContainsKey(coordinate))
                    _cache[coordinate] = tilemap.GetTile(coordinate);
                else
                    _cache.Add(coordinate, tilemap.GetTile(coordinate));
            }
        }

        private void ResetPlacementMap()
        {
            tilemap.ClearAllTiles();
            foreach (var kvp in _cache) tilemap.SetTile(kvp.Key, kvp.Value);
        }
    }
}