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

        private void Awake()
        {
            _button = GetComponent<Button>();
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
                }
                else
                {
                    buttonTextColor.a *= 0.5f;
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