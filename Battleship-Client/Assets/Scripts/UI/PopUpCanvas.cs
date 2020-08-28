using System;
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
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField passwordInput;
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
            rectTransform.anchoredPosition = new Vector2(0, rectTransform.anchoredPosition.y);
            if (acceptCall != null) confirm.onClick.AddListener(acceptCall);
            confirm.onClick.AddListener(Close);
            Destroy(cancel.gameObject);
        }

        public void Show(string headerText, string messageText, string acceptButtonText, string declineButtonText,
            UnityAction acceptCall = null, UnityAction declineCall = null, bool showNameInputIfAvailable = true,
            Action<string, string> confirmed = null)
        {
            _canvas.enabled = true;
            header.text = headerText;
            message.text = messageText;
            if (nameInput && !showNameInputIfAvailable) Destroy(nameInput.gameObject);
            if (passwordInput)
                confirm.onClick.AddListener(() =>
                    confirmed?.Invoke(nameInput && showNameInputIfAvailable ? nameInput.text : "", passwordInput.text));
            confirm.GetComponentInChildren<TMP_Text>().text = acceptButtonText;
            cancel.GetComponentInChildren<TMP_Text>().text = declineButtonText;
            if (acceptCall != null) confirm.onClick.AddListener(acceptCall);
            if (declineCall != null) cancel.onClick.AddListener(declineCall);
            confirm.onClick.AddListener(Close);
            cancel.onClick.AddListener(Close);
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}