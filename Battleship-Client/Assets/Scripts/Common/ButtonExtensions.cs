using TMPro;
using UnityEngine.UI;

namespace BattleshipGame.Common
{
    public static class ButtonExtensions
    {
        public static void SetInteractable(this Button button, bool state)
        {
            if (button.interactable == state) return;
            button.interactable = state;
            var buttonText = button.GetComponentInChildren<TMP_Text>();
            if (state)
                buttonText.color /= 0.25f;
            else
                buttonText.color *= 0.25f;
        }
    }
}