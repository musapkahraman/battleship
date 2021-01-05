using System.IO;
using BattleshipGame.Core;
using BattleshipGame.Localization;
using UnityEngine;
using static BattleshipGame.Core.StatusData.Status;

namespace BattleshipGame.UI
{
    public class LanguageMenuController : MonoBehaviour
    {
        [SerializeField] private StatusData statusData;
        [SerializeField] private LocalizationOptions localizationOptions;
        [SerializeField] private OptionsMenuController optionsMenuController;
        [SerializeField] private ButtonController englishButton;
        [SerializeField] private ButtonController turkishButton;
        [SerializeField] private ButtonController languageBackButton;
        private LocalizationOptions _lastSavedLocalizationOptions;
        private string _localizationOptionsFilepath;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _localizationOptionsFilepath = Path.Combine(Application.persistentDataPath, "localization.json");
            _lastSavedLocalizationOptions = ScriptableObject.CreateInstance<LocalizationOptions>();
            LoadLocalizationOptions();
        }

        private void Start()
        {
            languageBackButton.AddListener(Close);
            englishButton.AddListener(() => { localizationOptions.Language = SystemLanguage.English; });
            turkishButton.AddListener(() => { localizationOptions.Language = SystemLanguage.Turkish; });
        }

        public void Show()
        {
            _canvas.enabled = true;
            statusData.State = LanguageOptionsMenu;
        }

        public void Close()
        {
            SaveLocalizationOptions();
            _canvas.enabled = false;
            optionsMenuController.Show();
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

        private void CopyLocalizationOptions()
        {
            _lastSavedLocalizationOptions.Language = localizationOptions.Language;
        }

        private bool IsLocalizationOptionsChanged()
        {
            return _lastSavedLocalizationOptions.Language != localizationOptions.Language;
        }
    }
}