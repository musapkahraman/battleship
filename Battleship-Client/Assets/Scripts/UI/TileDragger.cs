using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(Tilemap),typeof(Collider))]
    public class TileDragger : MonoBehaviour
    {
        private Tilemap _tilemap;
        private void Start()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        private void OnMouseDown()
        {
            throw new NotImplementedException();
        }

        private void OnMouseDrag()
        {
            throw new NotImplementedException();
        }
    }
}
