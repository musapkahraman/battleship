using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class PopUpWindow : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private TMP_Text message;
        [SerializeField] private Button confirm;
        [SerializeField] private Button decline;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button passwordShowHide;
        [SerializeField] private Sprite show;
        [SerializeField] private Sprite hide;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab) || !passwordInput || !nameInput) return;
            if (nameInput.isFocused)
                passwordInput.ActivateInputField();
            else
                nameInput.ActivateInputField();
        }

        public void Show(string headerText, string messageText, string confirmButtonText,
            UnityAction confirmCall = null)
        {
            _canvas.enabled = true;
            header.text = headerText;
            message.text = messageText;
            confirm.GetComponentInChildren<TMP_Text>().text = confirmButtonText;
            var rectTransform = confirm.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, rectTransform.anchoredPosition.y);
            if (confirmCall != null) confirm.onClick.AddListener(confirmCall);
            confirm.onClick.AddListener(Close);
            Destroy(decline.gameObject);
        }

        public void Show(string headerText, string messageText, string confirmButtonText, string declineButtonText,
            UnityAction confirmCall = null, UnityAction declineCall = null, bool showNameInputIfAvailable = true,
            Action<string, string> confirmPasswordCallback = null)
        {
            _canvas.enabled = true;
            header.text = headerText;
            message.text = messageText;
            if (nameInput && !showNameInputIfAvailable)
            {
                var nameRectPos = nameInput.GetComponent<RectTransform>().anchoredPosition;
                Destroy(nameInput.gameObject);
                var passwordRect = passwordInput.GetComponent<RectTransform>();
                var middlePoint = Vector2.Lerp(nameRectPos, passwordRect.anchoredPosition, 0.5f);
                passwordRect.anchoredPosition = middlePoint;
            }

            if (passwordInput)
            {
                confirm.onClick.AddListener(() =>
                    confirmPasswordCallback?.Invoke(nameInput && showNameInputIfAvailable ? nameInput.text : "",
                        passwordInput.text));
                passwordShowHide.onClick.AddListener(() =>
                {
                    if (passwordInput.contentType == TMP_InputField.ContentType.Password)
                    {
                        passwordShowHide.GetComponent<Image>().sprite = hide;
                        passwordInput.contentType = TMP_InputField.ContentType.Standard;
                    }
                    else
                    {
                        passwordShowHide.GetComponent<Image>().sprite = show;
                        passwordInput.contentType = TMP_InputField.ContentType.Password;
                    }

                    passwordInput.ActivateInputField();
                });
            }

            confirm.GetComponentInChildren<TMP_Text>().text = confirmButtonText;
            decline.GetComponentInChildren<TMP_Text>().text = declineButtonText;
            if (confirmCall != null) confirm.onClick.AddListener(confirmCall);
            if (declineCall != null) decline.onClick.AddListener(declineCall);
            confirm.onClick.AddListener(Close);
            decline.onClick.AddListener(Close);
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}