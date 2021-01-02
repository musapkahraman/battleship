using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Network;
using UnityEngine;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RoomListManager : MonoBehaviour
    {
        [SerializeField] private GameObject roomListElementPrefab;
        [SerializeField] private int contentOffset;
        private readonly Dictionary<string, RoomListElement> _elements = new Dictionary<string, RoomListElement>();
        private RectTransform _rectTransform;
        private IRoomClickListener _roomClickListener;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void PopulateRoomList(Dictionary<string, Room> rooms, IRoomClickListener roomClickListener)
        {
            _roomClickListener = roomClickListener;
            foreach (var roomListElement in _elements)
                Destroy(roomListElement.Value.gameObject);
            _elements.Clear();
            ResetRectSize();
            var positionFactor = 0;
            var sortedDict = from room in rooms orderby room.Value.clients select room;
            foreach (var room in sortedDict)
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

        private float PutElementInPosition(Component element, ref int positionFactor)
        {
            var elementRectTransform = element.GetComponent<RectTransform>();
            float elementHeight = elementRectTransform.rect.height;
            elementRectTransform.anchoredPosition = new Vector2(0, positionFactor++ * -elementHeight - contentOffset);
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
            _roomClickListener.OnRoomClicked(roomListElement.RoomId);
        }
    }
}