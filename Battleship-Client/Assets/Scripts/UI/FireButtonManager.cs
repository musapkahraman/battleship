using BattleshipGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class FireButtonManager : MonoBehaviour
    {
        private Button _button;
        private TMP_Text _tmpText;
        private Color _textColorCache;
        [SerializeField] private GameManager manager;

        private void Start()
        {
            _button = GetComponent<Button>();
            _tmpText = GetComponentInChildren<TMP_Text>();
            _textColorCache = _tmpText.color;
            DisableButton();
            
            manager.FireReady += EnableButton;
            Debug.Log("Subscribed to fire ready event");
            _button.onClick.AddListener(GameManager.FireShots);
            _button.onClick.AddListener(DisableButton);
        }

        private void OnDestroy()
        {
            manager.FireReady -= EnableButton;
            Debug.Log("Unsubscribed from fire ready event");
        }

        private void EnableButton()
        {
            Debug.Log("Button is enabled");
            _button.interactable = true;
            _tmpText.color = _textColorCache;
        }

        private void DisableButton()
        {
            Debug.Log("Button is disabled");
            _button.interactable = false;
            _tmpText.color = Color.gray;
        }
    }
}