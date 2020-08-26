using System.Collections.Generic;
using BattleshipGame.Network;
using BattleshipGame.Schemas;
using UnityEngine;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RoomListManager : MonoBehaviour
    {
        private readonly Dictionary<string, RoomListElement> _elements = new Dictionary<string, RoomListElement>();
        [SerializeField] private GameObject roomListElementPrefab;
        [SerializeField] private LobbyManager lobbyManager;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void PopulateRoomList(Dictionary<string, Room> rooms)
        {
            foreach (var roomListElement in _elements)
                Destroy(roomListElement.Value.gameObject);
            _elements.Clear();
            ResetRectSize();
            var positionFactor = 0;
            foreach (var room in rooms)
            {
                var element = Instantiate(roomListElementPrefab, transform).GetComponent<RoomListElement>();
                element.RoomId = room.Key;
                element.OnClick += OnRoomClicked;
                element.roomName.text = room.Value.metadata.name;
                element.locked.enabled = room.Value.metadata.requiresPassword;
                element.clients.text = $"{room.Value.clients}/{room.Value.maxClients}";

                if (_elements.ContainsKey(room.Key))
                    _elements[room.Key] = element;
                else
                    _elements.Add(room.Key, element);

                SetRectSize(PutElementInPosition(element, ref positionFactor));
            }
        }

        private static float PutElementInPosition(Component element, ref int positionFactor)
        {
            var elementRectTransform = element.GetComponent<RectTransform>();
            float elementHeight = elementRectTransform.rect.height;
            elementRectTransform.anchoredPosition = new Vector2(0, positionFactor++ * -elementHeight);
            return elementHeight;
        }

        private void SetRectSize(float elementHeight)
        {
            var listSize = _rectTransform.sizeDelta;
            _rectTransform.sizeDelta = new Vector2(listSize.x, listSize.y + elementHeight);
        }

        private void ResetRectSize()
        {
            var listSize = _rectTransform.sizeDelta;
            _rectTransform.sizeDelta = new Vector2(listSize.x, 0);
        }

        private void OnRoomClicked(string id)
        {
            foreach (var element in _elements) element.Value.ChangeBackgroundColorAsDefault();

            if (!_elements.TryGetValue(id, out var roomListElement)) return;
            roomListElement.ChangeBackgroundColorAsSelected();
            lobbyManager.SetRoomId(roomListElement.RoomId);
        }
    }
}