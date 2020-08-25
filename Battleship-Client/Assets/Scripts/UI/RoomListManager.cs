using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RoomListManager : MonoBehaviour
    {
        private static readonly List<RoomListElement> Elements = new List<RoomListElement>();
        [SerializeField] private GameObject roomListElementPrefab;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void PopulateRoomList(int size)
        {
            Elements.Clear();
            for (var i = 0; i < size; i++)
            {
                var element = Instantiate(roomListElementPrefab, transform).GetComponent<RoomListElement>();
                Elements.Add(element);
                element.OnClick += OnRoomClicked;
                var elementRectTransform = element.GetComponent<RectTransform>();
                float elementHeight = elementRectTransform.rect.height;
                var listSize = _rectTransform.sizeDelta;
                _rectTransform.sizeDelta = new Vector2(listSize.x, listSize.y + elementHeight);
                elementRectTransform.anchoredPosition = new Vector2(0, i * -elementHeight);
                element.GetComponentInChildren<TMP_Text>().text = $"Room {i + 1}";
            }
        }

        private static void OnRoomClicked(RoomListElement roomListElement)
        {
            foreach (var element in Elements) element.ChangeBackgroundColorAsDefault();

            roomListElement.ChangeBackgroundColorAsSelected();
            Debug.Log("<color=green>Selected room: " +
                      $"\'{roomListElement.GetComponentInChildren<TMP_Text>().text}\'</color>");
        }
    }
}