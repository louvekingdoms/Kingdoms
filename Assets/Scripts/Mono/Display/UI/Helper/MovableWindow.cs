using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovableWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform window;

    Vector2 dragStartPosition;
    bool isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPosition = window.anchoredPosition - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Game.isMouseBusy = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Game.isMouseBusy = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Game.isMouseBusy = false;
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            window.anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y) + dragStartPosition;
        }
    }

}
