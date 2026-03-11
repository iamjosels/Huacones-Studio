using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableNumber : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Numero representado (del 1 al 10)")]
    public int numero;

    [Header("Feedback")]
    public float dragScaleOnHold = 1.06f;
    public float invalidNudgeDistance = 18f;
    public float invalidNudgeDuration = 0.12f;
    public int invalidNudgeLoops = 2;
    public Color invalidTint = new Color(1f, 0.55f, 0.55f, 1f);

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas canvas;
    private Image image;

    private Vector3 baseScale;
    private Color baseColor;
    private Coroutine invalidFeedbackRoutine;

    public DropSlot CurrentSlot { get; private set; }
    public DropSlot DragOriginSlot { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        image = GetComponent<Image>();

        baseScale = transform.localScale;
        baseColor = image != null ? image.color : Color.white;
        CurrentSlot = GetComponentInParent<DropSlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (invalidFeedbackRoutine != null)
        {
            StopCoroutine(invalidFeedbackRoutine);
            invalidFeedbackRoutine = null;
            RestoreVisualState();
        }

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        DragOriginSlot = CurrentSlot;

        if (canvas != null)
        {
            transform.SetParent(canvas.transform, true);
        }

        transform.localScale = baseScale * dragScaleOnHold;

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

        transform.localScale = baseScale;
        DragOriginSlot = null;
    }

    public void PlaceInSlot(DropSlot slot)
    {
        CurrentSlot = slot;
        transform.SetParent(slot.transform, false);
        Vector2 targetPosition = slot.GetNumberAnchoredPosition();
        rectTransform.anchoredPosition = targetPosition;

        originalParent = slot.transform;
        originalPosition = targetPosition;
        transform.localScale = baseScale;
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
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPosition;
        transform.localScale = baseScale;
    }

    public void PlayInvalidDropFeedback()
    {
        if (invalidFeedbackRoutine != null)
        {
            StopCoroutine(invalidFeedbackRoutine);
        }

        invalidFeedbackRoutine = StartCoroutine(InvalidDropRoutine());
    }

    private IEnumerator InvalidDropRoutine()
    {
        Vector2 startPosition = rectTransform.anchoredPosition;

        if (image != null)
        {
            image.color = invalidTint;
        }

        for (int i = 0; i < invalidNudgeLoops; i++)
        {
            rectTransform.anchoredPosition = startPosition + new Vector2(invalidNudgeDistance, 0f);
            yield return new WaitForSeconds(invalidNudgeDuration * 0.5f);

            rectTransform.anchoredPosition = startPosition - new Vector2(invalidNudgeDistance, 0f);
            yield return new WaitForSeconds(invalidNudgeDuration * 0.5f);
        }

        rectTransform.anchoredPosition = startPosition;
        RestoreVisualState();
        invalidFeedbackRoutine = null;
    }

    private void RestoreVisualState()
    {
        transform.localScale = baseScale;

        if (image != null)
        {
            image.color = baseColor;
        }
    }
}
