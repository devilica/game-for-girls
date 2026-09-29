using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DressUpGame.UI
{
    /// <summary>
    /// Forwards drag and scroll wheel events to a parent ScrollRect so lists with
    /// clickable buttons inside can still be scrolled with the mouse.
    /// </summary>
    public class ScrollDragForwarder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [SerializeField] private ScrollRect scrollRect;

        public void Initialize(ScrollRect target)
        {
            scrollRect = target;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            scrollRect?.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            scrollRect?.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            scrollRect?.OnEndDrag(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            scrollRect?.OnScroll(eventData);
        }
    }
}
