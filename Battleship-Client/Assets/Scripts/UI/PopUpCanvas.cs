using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class PopUpCanvas : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private TMP_Text message;
        [SerializeField] private Button confirm;
        [SerializeField] private Button cancel;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }
        
        public void Show(string headerText, string messageText, string acceptButtonText, UnityAction acceptCall = null)
        {
            _canvas.enabled = true;
            header.text = headerText;
            message.text = messageText;
            confirm.GetComponentInChildren<TMP_Text>().text = acceptButtonText;
            var rectTransform = confirm.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0,rectTransform.anchoredPosition.y);
            if (acceptCall != null) confirm.onClick.AddListener(acceptCall);
            confirm.onClick.AddListener(Close);
            Destroy(cancel.gameObject);

            void Close()
            {
                Destroy(gameObject);
            }
        }

        public void Show(string headerText, string messageText, string acceptButtonText, string declineButtonText,
            UnityAction acceptCall = null, UnityAction declineCall = null)
        {
            _canvas.enabled = true;
            header.text = headerText;
            message.text = messageText;
            confirm.GetComponentInChildren<TMP_Text>().text = acceptButtonText;
            cancel.GetComponentInChildren<TMP_Text>().text = declineButtonText;
            if (acceptCall != null) confirm.onClick.AddListener(acceptCall);
            if (declineCall != null) cancel.onClick.AddListener(declineCall);
            confirm.onClick.AddListener(Close);
            cancel.onClick.AddListener(Close);

            void Close()
            {
                Destroy(gameObject);
            }
        }
    }
}