using BattleshipGame.Localization;
using UnityEngine;
using UnityEngine.Events;

namespace BattleshipGame.UI
{
    [CreateAssetMenu(fileName = "NewMessageDialog", menuName = "Dialog/Message")]
    public class MessageDialog : ScriptableObject
    {
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private Key headerText;
        [SerializeField] private Key messageText;
        [SerializeField] private Key confirmButtonText;

        public void Show(UnityAction confirmCall = null)
        {
            Instantiate(popUpPrefab).GetComponent<PopUpWindow>()
                .Show(headerText, messageText, confirmButtonText, confirmCall);
        }
    }
}