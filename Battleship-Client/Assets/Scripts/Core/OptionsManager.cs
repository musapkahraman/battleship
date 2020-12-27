using System.IO;
using BattleshipGame.AI;
using BattleshipGame.Localization;
using BattleshipGame.UI;
using UnityEngine;

namespace BattleshipGame.Core
{
    [RequireComponent(typeof(MenuManager))]
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
        [SerializeField] private Options options;
        private LocalizationOptions _lastSavedLocalizationOptions;
        private Options _lastSavedOptions;
        private string _localizationOptionsFilepath;
        private MenuManager _menu;
        private string _optionsFilepath;

        private void Awake()
        {
            _localizationOptionsFilepath = Path.Combine(Application.persistentDataPath, "localization.json");
            _lastSavedLocalizationOptions = ScriptableObject.CreateInstance<LocalizationOptions>();
            LoadLocalizationOptions();
            _optionsFilepath = Path.Combine(Application.persistentDataPath, "options.json");
            _lastSavedOptions = ScriptableObject.CreateInstance<Options>();
            LoadOptions();
            _menu = GetComponent<MenuManager>();
            _menu.OnNavigateBack += OnNavigateBack;
        }

        private void Start()
        {
            optionsBackButton.AddListener(OnNavigateBack);
            languageBackButton.AddListener(OnNavigateBack);
            languageButton.AddListener(() =>
            {
                optionsCanvas.enabled = false;
                languageCanvas.enabled = true;
            });
            englishButton.AddListener(() => { localizationOptions.Language = SystemLanguage.English; });
            turkishButton.AddListener(() => { localizationOptions.Language = SystemLanguage.Turkish; });
        }

        private void OnDestroy()
        {
            _menu.OnNavigateBack -= OnNavigateBack;
        }

        private void OnNavigateBack()
        {
            if (optionsCanvas.enabled)
            {
                optionsCanvas.enabled = false;
                SaveOptions();
            }

            if (languageCanvas.enabled)
            {
                optionsCanvas.enabled = true;
                languageCanvas.enabled = false;
                SaveLocalizationOptions();
            }
        }

        private void SaveOptions()
        {
            if (IsAnyOptionChanged())
            {
                string dataAsJson = JsonUtility.ToJson(options);
                File.WriteAllText(_optionsFilepath, dataAsJson);
                CopyOptions();
            }
        }

        private void SaveLocalizationOptions()
        {
            if (IsLocalizationOptionsChanged())
            {
                string dataAsJson = JsonUtility.ToJson(localizationOptions);
                File.WriteAllText(_localizationOptionsFilepath, dataAsJson);
                CopyLocalizationOptions();
            }
        }

        private void LoadOptions()
        {
            if (File.Exists(_optionsFilepath))
            {
                string json = File.ReadAllText(_optionsFilepath);
                JsonUtility.FromJsonOverwrite(json, options);
                CopyOptions();
            }
            else
            {
                options.aiDifficulty = Difficulty.Easy;
            }
        }

        private void LoadLocalizationOptions()
        {
            if (File.Exists(_localizationOptionsFilepath))
            {
                string json = File.ReadAllText(_localizationOptionsFilepath);
                JsonUtility.FromJsonOverwrite(json, _lastSavedLocalizationOptions);
                CopyLocalizationOptions();
            }
            else
            {
                localizationOptions.Language = Application.systemLanguage;
            }
        }

        private void CopyOptions()
        {
            _lastSavedOptions.aiDifficulty = options.aiDifficulty;
        }

        private void CopyLocalizationOptions()
        {
            _lastSavedLocalizationOptions.Language = localizationOptions.Language;
        }

        private bool IsAnyOptionChanged()
        {
            return _lastSavedOptions.aiDifficulty != options.aiDifficulty;
        }

        private bool IsLocalizationOptionsChanged()
        {
            return _lastSavedLocalizationOptions.Language != localizationOptions.Language;
        }
    }
}