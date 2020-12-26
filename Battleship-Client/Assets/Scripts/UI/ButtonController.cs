using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonController : MonoBehaviour
    {
        [SerializeField] private TMP_Text buttonText;
        private Button _button;
        private Color _colorCache;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _colorCache = _button.GetComponent<Image>().color;
        }

        public void SetInteractable(bool state)
        {
            if (_button.interactable == state) return;
            if (buttonText)
            {
                var buttonTextColor = buttonText.color;
                if (state)
                {
                    buttonTextColor.a /= 0.5f;
                    _button.GetComponent<Image>().color = _colorCache;
                }
                else
                {
                    buttonTextColor.a *= 0.5f;
                    _button.GetComponent<Image>().color = new Color(0.5f,0.5f,0.55f,0.5f);
                }

                buttonText.color = buttonTextColor;
            }

            _button.interactable = state;
        }

        public void AddListener(UnityAction call)
        {
            _button.onClick.AddListener(call);
        }
    }
}