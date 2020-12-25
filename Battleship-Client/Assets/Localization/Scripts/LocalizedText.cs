using System;
using TMPro;
using UnityEngine;

namespace BattleshipGame.Localization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private Key key;
        [SerializeField] private LocalizationManager localizationManager;
        [SerializeField] private LocalizationOptions options;
        private bool _isKeyNull;
        private TextMeshProUGUI _textField;

        private void Awake()
        {
            _textField = GetComponent<TextMeshProUGUI>();
            if (key == null)
            {
                _isKeyNull = true;
            }
        }

        private void OnValidate()
        {
            Awake();
            Start();
        }

        private void Start()
        {
            SetText();
        }

        private void OnEnable()
        {
            options.OnLanguageChanged += SetText;
        }

        private void OnDisable()
        {
            options.OnLanguageChanged -= SetText;
        }

        private void SetText()
        {
            if (_isKeyNull) return;
            _textField.text = localizationManager.GetValue(key);
        }

        public void SetText(Key localizedTextKey)
        {
            key = localizedTextKey;
            _isKeyNull = false;
            SetText();
        }

        public void ClearText()
        {
            _textField.text = string.Empty;
        }
    }
}