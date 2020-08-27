using BattleshipGame.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class ResultCanvasManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text resultMessageText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button rematchButton;
        [SerializeField] private Button giveUpButton;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void Show(NetworkClient client, bool isWinner)
        {
            _canvas.enabled = true;
            resultMessageText.text = isWinner ? "You Win!" : "You Lost!!!";
            messageText.text = isWinner ? "Winners never quit!" : "Quitters never win!";
            rematchButton.GetComponentInChildren<TMP_Text>().text = "Rematch";
            giveUpButton.GetComponentInChildren<TMP_Text>().text = isWinner ? "Quit" : "Give Up";
            rematchButton.onClick.AddListener(() =>
            {
                rematchButton.interactable = false;
                giveUpButton.interactable = false;
                messageText.text = "Waiting for the opponent decide.";
                client.SendRematch(true);
            });
            giveUpButton.onClick.AddListener(() =>
            {
                Close();
                client.SendRematch(false);
            });
        }

        public void Close()
        {
            _canvas.enabled = false;
            rematchButton.onClick.RemoveAllListeners();
            giveUpButton.onClick.RemoveAllListeners();
        }
    }
}