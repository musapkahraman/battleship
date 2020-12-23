using BattleshipGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleshipGame.Localization
{
    public class LanguageChanger : MonoBehaviour
    {
        public TextMeshProUGUI buttonLabel;
        public LocalizationOptions options;
        public Image image;
        public SystemLanguage targetLanguage;

        private void Start()
        {
            buttonLabel.SetText(targetLanguage.ToString());
            SetImage();
        }

        private void SetImage()
        {
            image.enabled = targetLanguage == options.Language;
        }

        // Called from language buttons in Options Menu
        public void Change()
        {
            options.Language = targetLanguage;
        }

        private void OnLanguageChanged()
        {
            SetImage();
        }

        private void OnEnable()
        {
            options.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            options.OnLanguageChanged -= OnLanguageChanged;
        }
    }
}