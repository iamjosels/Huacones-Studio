using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Numero actualmente colocado")]
    public DraggableNumber currentNumber;

    [Header("Ajuste del numero en el slot")]
    public Vector2 numberOffset = Vector2.zero;

    [Header("Feedback visual")]
    public Color validHoverColor = new Color(0.72f, 1f, 0.75f, 0.95f);
    public Color invalidHoverColor = new Color(1f, 0.72f, 0.72f, 0.95f);

    private Image slotImage;
    private Color baseColor = Color.white;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null)
        {
            baseColor = slotImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        DraggableNumber dragged = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<DraggableNumber>()
            : null;

        if (dragged == null)
        {
            return;
        }

        bool valid = IsDropValidFor(dragged);
        ApplyHover(valid);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHover();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableNumber droppedNumber = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<DraggableNumber>()
            : null;

        if (droppedNumber == null)
        {
            ResetHover();
            return;
        }

        DropSlot sourceSlot = droppedNumber.DragOriginSlot;

        if (sourceSlot == this)
        {
            SetCurrentNumber(droppedNumber);
            ResetHover();
            return;
        }

        if (currentNumber == null)
        {
            if (sourceSlot != null)
            {
                sourceSlot.ClearSlotIfMatches(droppedNumber);
            }

            SetCurrentNumber(droppedNumber);
            GameNumberR.Instance?.CheckWinCondition();
            ResetHover();
            return;
        }

        // Desde el panel de numeros no se reemplaza un slot ocupado.
        if (sourceSlot == null)
        {
            droppedNumber.ReturnToOrigin();
            droppedNumber.PlayInvalidDropFeedback();
            ApplyHover(false);
            return;
        }

        DraggableNumber displacedNumber = currentNumber;
        SetCurrentNumber(droppedNumber);
        sourceSlot.ReceiveSwappedNumber(displacedNumber);

        GameNumberR.Instance?.CheckWinCondition();
        ResetHover();
    }

    public void ClearSlot()
    {
        if (currentNumber != null)
        {
            currentNumber.ClearCurrentSlotReference(this);
        }

        currentNumber = null;
    }

    public void ClearSlotIfMatches(DraggableNumber number)
    {
        if (currentNumber != number)
        {
            return;
        }

        currentNumber = null;
        number.ClearCurrentSlotReference(this);
    }

    public void SetCurrentNumber(DraggableNumber number)
    {
        currentNumber = number;
        number.PlaceInSlot(this);
    }

    public void ReceiveSwappedNumber(DraggableNumber number)
    {
        currentNumber = number;
        number.PlaceInSlot(this);
    }

    public int? GetCurrentNumber()
    {
        return currentNumber != null ? currentNumber.numero : null;
    }

    public Vector2 GetNumberAnchoredPosition()
    {
        return numberOffset;
    }

    private bool IsDropValidFor(DraggableNumber dragged)
    {
        DropSlot sourceSlot = dragged.DragOriginSlot;

        if (sourceSlot == this)
        {
            return true;
        }

        if (currentNumber == null)
        {
            return true;
        }

        if (sourceSlot == null)
        {
            return false;
        }

        return true;
    }

    private void ApplyHover(bool valid)
    {
        if (slotImage != null)
        {
            slotImage.color = valid ? validHoverColor : invalidHoverColor;
        }
    }

    private void ResetHover()
    {
        if (slotImage != null)
        {
            slotImage.color = baseColor;
        }
    }
}
