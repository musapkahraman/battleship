using System;
using UnityEngine;

namespace BattleshipGame.UI
{
    [CreateAssetMenu(menuName = "Variable/Color")]
    public class ColorVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        public Color initialValue;
        [NonSerialized] public Color Value;

        public void OnAfterDeserialize()
        {
            Value = initialValue;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}