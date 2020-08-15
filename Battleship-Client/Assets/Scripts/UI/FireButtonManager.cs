using BattleshipGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class FireButtonManager : MonoBehaviour
    {
        [SerializeField] private GameManager manager;
        private Button _button;
        private Color _textColorCache;
        private TMP_Text _tmpText;

        private void Start()
        {
            _button = GetComponent<Button>();
            _tmpText = GetComponentInChildren<TMP_Text>();
            _textColorCache = _tmpText.color;
            DisableButton();
            manager.FireReady += EnableButton;
            manager.FireNotReady += DisableButton;
            _button.onClick.AddListener(GameManager.FireShots);
            _button.onClick.AddListener(DisableButton);
        }

        private void OnDestroy()
        {
            manager.FireReady -= EnableButton;
            manager.FireNotReady -= DisableButton;
        }

        private void EnableButton()
        {
            _button.interactable = true;
            _tmpText.color = _textColorCache;
        }

        private void DisableButton()
        {
            _button.interactable = false;
            _tmpText.color = Color.gray;
        }
    }
}