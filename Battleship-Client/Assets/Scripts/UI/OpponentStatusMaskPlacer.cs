using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Common;
using UnityEngine;

namespace BattleshipGame.UI
{
    public class OpponentStatusMaskPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject maskPrefab;
        private List<GridSpriteMapper.ShipPart> _opponentShipPart;
        private readonly Dictionary<int, Stack<Vector3Int>> _spritePositions = new Dictionary<int, Stack<Vector3Int>>();
        private GridSpriteMapper _spriteMapper;

        private void Start()
        {
            _spriteMapper = GetComponent<GridSpriteMapper>();
            _opponentShipPart = _spriteMapper.GetPartsList().ToList();
            foreach (var spritePosition in _spriteMapper.GetSpritePositions())
            {
                _spritePositions.Add(spritePosition.Key, spritePosition.Value.CloneToStack());
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
    }
}