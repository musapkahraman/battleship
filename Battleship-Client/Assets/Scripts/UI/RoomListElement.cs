using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class RoomListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Color32 selectedColor;
        public TMP_Text roomName;
        public TMP_Text clients;
        public Image locked;
        private Image _backGroundImage;
        private Color32 _defaultColor;
        public string RoomId { get; set; }

        private void Start()
        {
            _backGroundImage = GetComponent<Image>();
            _defaultColor = _backGroundImage.color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(RoomId);
        }

        public event Action<string> OnClick;

        public void ChangeBackgroundColorAsSelected()
        {
            _backGroundImage.color = selectedColor;
        }

        public void ChangeBackgroundColorAsDefault()
        {
            _backGroundImage.color = _defaultColor;
        }
    }
}