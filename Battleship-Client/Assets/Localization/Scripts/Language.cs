using System.Collections.Generic;
using UnityEngine;

namespace BattleshipGame.Localization
{
    [CreateAssetMenu(menuName = "Localization/Language")]
    public class Language : ScriptableObject
    {
        public SystemLanguage title;
        public List<LocalizationItem> items;
        public string keyName = "new_key";
    }
}