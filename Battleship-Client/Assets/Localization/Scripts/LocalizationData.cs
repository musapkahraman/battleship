using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleshipGame.Localization
{
    [Serializable]
    public class LocalizationData
    {
        public List<LocalizationItem> items;
        public SystemLanguage language;

        public LocalizationData(SystemLanguage language, List<LocalizationItem> items)
        {
            this.language = language;
            this.items = items;
        }
    }

    [Serializable]
    public class LocalizationJsonData
    {
        public List<LocalizationJsonItem> items;
        public SystemLanguage language;

        public LocalizationJsonData(SystemLanguage language, List<LocalizationJsonItem> items)
        {
            this.language = language;
            this.items = items;
        }
    }

    [Serializable]
    public class LocalizationItem
    {
        public Key key;
        public string value;
    }

    [Serializable]
    public class LocalizationJsonItem
    {
        public string key;
        public string value;
    }
}