using BattleshipGame.Common;
using BattleshipGame.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(Grid), typeof(BoxCollider2D), typeof(GridSpriteMapper))]
    public class TileDragger : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private GameManager manager;
        [SerializeField] private Tilemap tilemap;
        private GridSpriteMapper _spriteMapper;
        private Grid _grid;

        private void Start()
        {
            _spriteMapper = GetComponent<GridSpriteMapper>();
            _grid = GetComponent<Grid>();
        }

        private void OnMouseDown()
        {
            var cell = GridConverter.ScreenToCell(Input.mousePosition, _grid, sceneCamera, manager.MapAreaSize);
            print(cell);
            foreach (var part in _spriteMapper.GetPartsList())
            {
                print(part.Coordinate);
                if (((Vector3) part.Coordinate).Equals(cell))
                {
                    print("Found one!");
                }
            }
        }
    }
}