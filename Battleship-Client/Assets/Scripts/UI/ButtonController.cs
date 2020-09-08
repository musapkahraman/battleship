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
                if (state)
                    buttonText.color /= 0.5f;
                else
                    buttonText.color *= 0.5f;
            }

            _button.interactable = state;
        }

        public void SetText(string text)
        {
            buttonText.text = text;
        }

        public void AddListener(UnityAction call)
        {
            _button.onClick.AddListener(call);
        }
    }
}