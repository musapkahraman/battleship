using BattleshipGame.Localization;
using UnityEngine;
using UnityEngine.Events;

namespace BattleshipGame.UI
{
    [CreateAssetMenu(fileName = "NewOptionDialog", menuName = "Dialog/Option")]
    public class OptionDialog : ScriptableObject
    {
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private Key headerText;
        [SerializeField] private Key messageText;
        [SerializeField] private Key confirmButtonText;
        [SerializeField] private Key declineButtonText;

        public void Show(UnityAction confirmCall = null, UnityAction declineCall = null)
        {
            Instantiate(popUpPrefab).GetComponent<PopUpWindow>()
                .Show(headerText, messageText, confirmButtonText, declineButtonText, confirmCall, declineCall);
        }
    }
}