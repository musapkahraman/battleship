using System.IO;
using BattleshipGame.Core;
using BattleshipGame.Localization;
using UnityEngine;
using static BattleshipGame.Core.StatusData.Status;

namespace BattleshipGame.UI
{
    public class OptionsMenuController : MonoBehaviour
    {
        [SerializeField] private StatusData statusData;
        [SerializeField] private Options options;
        [SerializeField] private MainMenuController mainMenuController;
        [SerializeField] private LanguageMenuController languageMenuController;
        [SerializeField] private ButtonController vibrationSwitchButton;
        [SerializeField] private Key vibrationOnText;
        [SerializeField] private Key vibrationOffText;
        [SerializeField] private ButtonController languageButton;
        [SerializeField] private ButtonController optionsBackButton;
        private Canvas _canvas;
        private Options _lastSavedOptions;
        private string _optionsFilepath;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _optionsFilepath = Path.Combine(Application.persistentDataPath, "options.json");
            _lastSavedOptions = ScriptableObject.CreateInstance<Options>();
            LoadOptions();
        }

        private void Start()
        {
            optionsBackButton.AddListener(Close);
            languageButton.AddListener(() =>
            {
                _canvas.enabled = false;
                languageMenuController.Show();
            });
            
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
            vibrationSwitchButton.ChangeText(options.vibration ? vibrationOffText : vibrationOnText);
            vibrationSwitchButton.AddListener(() =>
            {
                if (options.vibration)
                {
                    options.vibration = false;
                    vibrationSwitchButton.ChangeText(vibrationOnText);
                }
                else
                {
                    options.vibration = true;
                    vibrationSwitchButton.ChangeText(vibrationOffText);
                    Handheld.Vibrate();
                }
            });
#else
            vibrationSwitchButton.Hide();
#endif
        }

        public void Show()
        {
            _canvas.enabled = true;
            statusData.State = OptionsMenu;
        }

        public void Close()
        {
            SaveOptions();
            _canvas.enabled = false;
            mainMenuController.Show();
        }

        private void SaveOptions()
        {
            if (IsAnyOptionChanged())
            {
                string dataAsJson = JsonUtility.ToJson(options);
                File.WriteAllText(_optionsFilepath, dataAsJson);
                CloneOptionsState();
            }
        }

        private void LoadOptions()
        {
            if (File.Exists(_optionsFilepath))
            {
                string json = File.ReadAllText(_optionsFilepath);
                JsonUtility.FromJsonOverwrite(json, options);
                CloneOptionsState();
            }
            else
            {
                options.aiDifficulty = Difficulty.Easy;
                options.vibration = true;
            }
        }

        private void CloneOptionsState()
        {
            _lastSavedOptions.aiDifficulty = options.aiDifficulty;
            _lastSavedOptions.vibration = options.vibration;
        }

        private bool IsAnyOptionChanged()
        {
            return _lastSavedOptions.aiDifficulty != options.aiDifficulty ||
                   _lastSavedOptions.vibration != options.vibration;
        }
    }
}