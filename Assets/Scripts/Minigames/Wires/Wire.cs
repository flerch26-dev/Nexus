using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Wire : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public bool isLeftWire;
    public Color customColor;

    private Image image;
    private LineRenderer lineRenderer;
    private Canvas canvas;
    private WireTask wireTask;

    private bool isDragStarted = false;
    public bool isSuccess = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        lineRenderer = GetComponent<LineRenderer>();
        canvas = GetComponentInParent<Canvas>();
        wireTask = GetComponentInParent<WireTask>();
    }

    private void Update()
    {
        if (isDragStarted)
        {
            Vector2 movePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle
                (canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out movePos);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, canvas.transform.TransformPoint(movePos));
        }
        else
        {
            if (!isSuccess)
            {
                lineRenderer.SetPosition(0, Vector2.zero);
                lineRenderer.SetPosition(1, Vector2.zero);
            }
        }

        bool isHovered = RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition, canvas.worldCamera);
        if (isHovered) wireTask.currentHoveredWire = this;
    }

    public void SetColor(Color color)
    {
        image.color = color;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        customColor = color;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //needed for drag but not used
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isLeftWire) return;
        if (isSuccess) return;

        isDragStarted = true;
        wireTask.currentDraggedWire = this;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (wireTask.currentHoveredWire != null)
        {
            if (wireTask.currentHoveredWire.customColor == customColor && !wireTask.currentHoveredWire.isLeftWire)
            {
                lineRenderer.SetPosition(1, wireTask.currentHoveredWire.gameObject.transform.position);
                isSuccess = true;
                wireTask.currentHoveredWire.isSuccess = true;
            }
        }

        isDragStarted = false;
        wireTask.currentDraggedWire = null;
    }
}
