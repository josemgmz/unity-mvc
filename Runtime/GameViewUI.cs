using UnityEngine.EventSystems;

namespace UnityMVC
{
    /// <summary>
    /// Represents the UI view component in the MVC pattern, handling various pointer and drag events.
    /// </summary>
    public class GameViewUI : GameView, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IPointerMoveHandler, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        #region Unity Methods

        public void OnBeginDrag(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnBeginDrag, other);
        }

        public void OnDrag(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnDrag, other);
        }

        public void OnEndDrag(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnEndDrag, other); 
        }
        
        public void OnPointerDown(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnPointerDown, other);
        }

        public void OnPointerExit(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnPointerExit, other);
        }

        public void OnPointerMove(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnPointerMove, other); 
        }
        
        public void OnPointerUp(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnPointerUp, other);
        }
        
        public void OnPointerEnter(PointerEventData other)
        {
            InvokeGameMethods(GameMethodEvents.OnPointerEnter, other);
        }

        #endregion

        public void OnSelect(BaseEventData eventData)
        {
            InvokeGameMethods(GameMethodEvents.OnSelect, eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            InvokeGameMethods(GameMethodEvents.OnDeselect, eventData);
        }
    }
}