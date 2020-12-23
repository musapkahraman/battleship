using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleshipGame.Localization
{
    [CreateAssetMenu(menuName = "Localization/Manager", fileName = "Manager")]
    public class LocalizationManager : ScriptableObject
    {
        private const string PlaceHolder = "Text";
        [SerializeField] private List<Language> languages = new List<Language>();
        public LocalizationOptions options;

        public bool AddLanguage(Language newLanguage)
        {
            bool isAlreadyAdded = languages.Any(language => language.title == newLanguage.title);
            if (isAlreadyAdded) return false;
            languages.Add(newLanguage);
            return true;
        }

        public string GetValue(Key key)
        {
            if (languages.Count == 0)
            {
                Debug.LogError("No language defined in the Localization Manager!");
                return PlaceHolder;
            }

            if (GetValueFromOptedLanguage(key, out string value)) return value;

            foreach (var item in languages.Select(language => GetItemsWithKey(language, key))
                .Select(items => items.FirstOrDefault()).Where(item => item != null))
                return item.value;

            Debug.LogError($"No localized text in any languages for the key: <color=yellow>{key}</color>");
            return PlaceHolder;
        }

        private bool GetValueFromOptedLanguage(Key key, out string value)
        {
            var optedLanguages = from language in languages where language.title == options.Language select language;
            var optedLanguage = optedLanguages.FirstOrDefault();
            if (optedLanguage == null)
            {
                Debug.Log($"{options.Language} is missing in the Localization Manager!");
                value = null;
                return false;
            }

            var items = GetItemsWithKey(optedLanguage, key);
            var item = items.FirstOrDefault();
            if (item == null)
            {
                Debug.Log($"{key} is missing in {options.Language}");
                value = null;
                return false;
            }

            value = item.value;
            return true;
        }

        private static IEnumerable<LocalizationItem> GetItemsWithKey(Language language, Key key)
        {
            return from item in language.items where item.key.Equals(key) select item;
        }
    }
}