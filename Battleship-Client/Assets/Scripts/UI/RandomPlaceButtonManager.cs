using BattleshipGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class RandomPlaceButtonManager : MonoBehaviour
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
            manager.RandomAvailable += EnableButton;
            manager.RandomHidden += HideButton;
            _button.onClick.AddListener(manager.PlaceShipsRandomly);
        }

        private void OnDestroy()
        {
            manager.RandomAvailable -= EnableButton;
            manager.RandomHidden -= DisableButton;
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

        private void HideButton()
        {
            gameObject.SetActive(false);
        }
    }
}