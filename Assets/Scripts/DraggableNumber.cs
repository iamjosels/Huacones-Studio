using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableNumber : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Numero representado (del 1 al 10)")]
    public int numero;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas canvas;

    public DropSlot CurrentSlot { get; private set; }
    public DropSlot DragOriginSlot { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        CurrentSlot = GetComponentInParent<DropSlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        DragOriginSlot = CurrentSlot;

        if (canvas != null)
        {
            transform.SetParent(canvas.transform, true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Canvas activeCanvas = GetComponentInParent<Canvas>();
        if (activeCanvas == null)
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / activeCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (canvas != null && transform.parent == canvas.transform)
        {
            ReturnToOrigin();
        }

        DragOriginSlot = null;
    }

    public void PlaceInSlot(DropSlot slot)
    {
        CurrentSlot = slot;
        transform.SetParent(slot.transform);
        rectTransform.anchoredPosition = Vector2.zero;

        originalParent = slot.transform;
        originalPosition = Vector2.zero;
    }

    public void ClearCurrentSlotReference(DropSlot slot)
    {
        if (CurrentSlot == slot)
        {
            CurrentSlot = null;
        }
    }

    public void ReturnToOrigin()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}
