using System;
using BattleshipGame.Localization;
using UnityEngine;

namespace BattleshipGame.UI
{
    [CreateAssetMenu(fileName = "NewRoomDialog", menuName = "Dialog/Room")]
    public class RoomDialog : ScriptableObject
    {
        [SerializeField] private GameObject popUpPrefab;
        [SerializeField] private Key headerText;
        [SerializeField] private Key messageText;
        [SerializeField] private Key confirmButtonText;
        [SerializeField] private Key declineButtonText;

        public void Show(bool showNameInputIfAvailable = true, Action<string, string> confirmPasswordCallback = null)
        {
            Instantiate(popUpPrefab).GetComponent<PopUpWindow>().Show(headerText, messageText, confirmButtonText,
                declineButtonText, null, null, showNameInputIfAvailable, confirmPasswordCallback);
        }
    }
}