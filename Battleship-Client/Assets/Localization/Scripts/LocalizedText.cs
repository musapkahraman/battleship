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
        private TextMeshProUGUI _textField;

        private void Start()
        {
            _textField = GetComponent<TextMeshProUGUI>();
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
            if (key == null || _textField == null) return;
            _textField.text = localizationManager.GetValue(key);
        }

        public void SetText(Key localizedTextKey)
        {
            key = localizedTextKey;
            SetText();
        }
    }
}