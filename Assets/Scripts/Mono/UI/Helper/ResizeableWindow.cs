using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeableWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform window;
    public UnityEngine.UI.Image handleImage;
    public UnityEngine.UI.Image titleBar;

    float dragStartPosition;
    float startWidth;
    bool isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPosition = Input.mousePosition.x;
        startWidth = window.sizeDelta.x;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void Update()
    {
        handleImage.color = titleBar.color;

        if (isDragging)
        {
            window.sizeDelta = new Vector2(startWidth + Input.mousePosition.x - dragStartPosition, window.sizeDelta.y);
        }
    }
}
