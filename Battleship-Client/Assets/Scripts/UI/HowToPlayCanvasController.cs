using UnityEngine;

namespace BattleshipGame.UI
{
    public class HowToPlayCanvasController : MonoBehaviour
    {
        private Canvas _canvas;

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void Toggle()
        {
            _canvas.enabled = !_canvas.enabled;
        }

        public void Close()
        {
            _canvas.enabled = false;
        }
    }
}
