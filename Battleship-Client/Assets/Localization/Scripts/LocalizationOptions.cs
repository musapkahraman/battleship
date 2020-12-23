using System;
using UnityEngine;

namespace BattleshipGame.Localization
{
    [CreateAssetMenu(fileName = "LocalizationOptions", menuName = "Localization/Options")]
    public class LocalizationOptions : ScriptableObject
    {
        [SerializeField] private SystemLanguage language = SystemLanguage.English;

        public SystemLanguage Language
        {
            get => language;
            set
            {
                language = value;
                OnLanguageChanged?.Invoke();
            }
        }

        public event Action OnLanguageChanged;

        private void OnValidate()
        {
            if (language == SystemLanguage.English || language == SystemLanguage.Turkish)
                Language = language;
            else
                language = Language;
        }
    }
}