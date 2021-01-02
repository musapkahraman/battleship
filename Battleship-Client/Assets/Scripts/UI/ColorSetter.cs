using UnityEngine;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(Image))]
    public class ColorSetter : MonoBehaviour
    {
        [SerializeField] private ColorVariable color;
        private Image _image;

        private void Awake()
        {
            SetColor();
        }

        private void OnValidate()
        {
            SetColor();
        }

        private void SetColor()
        {
            if (color == null) return;
            _image = GetComponent<Image>();
            if (_image == null) return;
            _image.color = color.Value;
        }
    }
}