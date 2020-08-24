using TMPro;
using UnityEngine;

namespace BattleshipGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RoomsListManager : MonoBehaviour
    {
        private RectTransform _rectTransform;
        [SerializeField] private GameObject roomListElementPrefab;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetRooms(int size)
        {
            for (var i = 0; i < size; i++)
            {
                var element = Instantiate(roomListElementPrefab, transform);
                var elementRectTransform = element.GetComponent<RectTransform>();
                float elementHeight = elementRectTransform.rect.height;
                var listSize = _rectTransform.sizeDelta;
                _rectTransform.sizeDelta = new Vector2(listSize.x, listSize.y + elementHeight);
                elementRectTransform.anchoredPosition = new Vector2(0, i * -elementHeight);
                element.GetComponentInChildren<TMP_Text>().text = $"Game {i + 1}";
            }
        }
    }
}