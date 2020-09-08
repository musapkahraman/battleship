using BattleshipGame.Common;
using BattleshipGame.Core;
using BattleshipGame.ScriptableObjects;
using UnityEngine;

namespace BattleshipGame.TilePaint
{
    public class TurnHighlighter : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Rules rules;
        [SerializeField] private BattleManager manager;
        [SerializeField] private BattleMap battleMap;

        private Grid _grid;
        private OpponentStatusMaskPlacer _statusMaskPlacer;

        private void Start()
        {
            _grid = GetComponent<Grid>();
            _statusMaskPlacer = GetComponent<OpponentStatusMaskPlacer>();
        }

        private void OnMouseDown()
        {
            if (battleMap.InteractionMode != MapInteractionMode.TurnHighlighting) return;
            var coordinate = GridUtils.ScreenToCoordinate(Input.mousePosition, _grid, sceneCamera, rules.AreaSize);
            Debug.Log($"Highlight {coordinate}");
            manager.HighlightTurn(_statusMaskPlacer.GetShotTurn(coordinate));
        }
    }
}