using System;
using UnityEngine;

namespace BattleshipGame.Readme
{
    public class Readme : ScriptableObject
    {
        public string title;
        public Section[] sections;

        [Serializable]
        public class Section
        {
            public string heading, text, linkText, url;
        }
    }
}