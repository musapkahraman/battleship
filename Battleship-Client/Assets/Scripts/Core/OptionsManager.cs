using BattleshipGame.Localization;
using BattleshipGame.UI;
using UnityEngine;

namespace BattleshipGame.Core
{
    public class OptionsManager : MonoBehaviour
    {
        [SerializeField] private ButtonController optionsBackButton;
        [SerializeField] private ButtonController languageButton;
        [SerializeField] private ButtonController languageBackButton;
        [SerializeField] private ButtonController englishButton;
        [SerializeField] private ButtonController turkishButton;
        [SerializeField] private Canvas optionsCanvas;
        [SerializeField] private Canvas languageCanvas;
        [SerializeField] private LocalizationOptions localizationOptions;

        private void Start()
        {
            optionsBackButton.AddListener(() => { optionsCanvas.enabled = false; });
            languageButton.AddListener(() =>
            {
                optionsCanvas.enabled = false;
                languageCanvas.enabled = true;
            });
            languageBackButton.AddListener(() =>
            {
                languageCanvas.enabled = false;
                optionsCanvas.enabled = true;
            });
            englishButton.AddListener(() => { localizationOptions.Language = SystemLanguage.English; });
            turkishButton.AddListener(() => { localizationOptions.Language = SystemLanguage.Turkish; });
        }
    }
}