using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleshipGame.UI
{
    public class RoomListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Color32 selectedColor;
        private Image _backGroundImage;
        private Color32 _defaultColor;

        private void Start()
        {
            _backGroundImage = GetComponent<Image>();
            _defaultColor = _backGroundImage.color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }

        public event Action<RoomListElement> OnClick;

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