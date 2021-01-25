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
        private TMP_Text _textField;

        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            options.OnLanguageChanged -= SetText;
        }

        private void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            if (_textField == null) _textField = GetComponent<TMP_Text>();
            if (key == null) _isKeyNull = true;
            options.OnLanguageChanged += SetText;
            SetText();
        }

        private void SetText()
        {
            _textField.text = _isKeyNull ? string.Empty : localizationManager.GetValue(key);
        }

        public void SetText(Key localizedTextKey)
        {
            key = localizedTextKey;
            _isKeyNull = false;
            SetText();
        }

        public void ClearText()
        {
            key = null;
            _isKeyNull = true;
            SetText();
        }
    }
}