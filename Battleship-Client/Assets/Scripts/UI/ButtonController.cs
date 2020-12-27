using BattleshipGame.Common;
using BattleshipGame.Localization;
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
        private Image _buttonImage;
        private string _textCache;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = _button.GetComponent<Image>();
            _colorCache = _buttonImage.color;
            _textCache = buttonText.text;
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
                    _buttonImage.color = _colorCache;
                }
                else
                {
                    buttonTextColor.a *= 0.5f;
                    _buttonImage.color = new Color(0.5f,0.5f,0.55f,0.5f);
                }

                buttonText.color = buttonTextColor;
            }

            _button.interactable = state;
        }

        public void AddListener(UnityAction call)
        {
            _button.onClick.AddListener(call);
        }

        public void ChangeText(Key text)
        {
            var localizedText = _button.GetComponentInChildren<LocalizedText>();
            if (localizedText)
            {
                localizedText.SetText(text);
            }
        }

        public void ResetText()
        {
            buttonText.text = _textCache;
        }

        public void ChangeColor(ColorVariable color)
        {
            _buttonImage.color = color.Value;
        }

        public void ResetColor()
        {
            _buttonImage.color = _colorCache;
        }
    }
}