using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovableWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform window;

    Vector2 dragStartPosition;
    bool isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPosition = window.anchoredPosition - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
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
