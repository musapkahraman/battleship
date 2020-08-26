using System.Collections.Generic;
using BattleshipGame.Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public void PopulateRoomList(Dictionary<string, Room> rooms)
        {
            Elements.Clear();
            var i = 0;
            foreach (var room in rooms)
            {
                var element = Instantiate(roomListElementPrefab, transform).GetComponent<RoomListElement>();
                element.RoomId = room.Key;
                element.roomName.text = room.Value.metadata.name;
                element.locked.enabled = room.Value.metadata.requiresPassword;
                element.clients.text= $"{room.Value.clients}/{room.Value.maxClients}";
                Elements.Add(element);
                element.OnClick += OnRoomClicked;
                var elementRectTransform = element.GetComponent<RectTransform>();
                float elementHeight = elementRectTransform.rect.height;
                var listSize = _rectTransform.sizeDelta;
                _rectTransform.sizeDelta = new Vector2(listSize.x, listSize.y + elementHeight);
                elementRectTransform.anchoredPosition = new Vector2(0, i++ * -elementHeight);
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