using System.IO;
using BattleshipGame.Core;
using UnityEngine;
using static BattleshipGame.Core.GameStateContainer.GameState;

namespace BattleshipGame.UI
{
    public class OptionsMenuController : MonoBehaviour
    {
        [SerializeField] private GameStateContainer gameStateContainer;
        [SerializeField] private Options options;
        [SerializeField] private MainMenuController mainMenuController;
        [SerializeField] private LanguageMenuController languageMenuController;
        [SerializeField] private ButtonController languageButton;
        [SerializeField] private ButtonController optionsBackButton;
        private Options _lastSavedOptions;
        private string _optionsFilepath;
        private Canvas _canvas;

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
        }

        public void Show()
        {
            _canvas.enabled = true;
            gameStateContainer.State = OptionsMenu;
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
                CopyOptions();
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

        private void CopyOptions()
        {
            _lastSavedOptions.aiDifficulty = options.aiDifficulty;
        }

        private bool IsAnyOptionChanged()
        {
            return _lastSavedOptions.aiDifficulty != options.aiDifficulty;
        }
    }
}