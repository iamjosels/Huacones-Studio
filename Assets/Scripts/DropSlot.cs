using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    [Header("Numero actualmente colocado")]
    public DraggableNumber currentNumber;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableNumber droppedNumber = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<DraggableNumber>()
            : null;

        if (droppedNumber == null)
        {
            return;
        }

        DropSlot sourceSlot = droppedNumber.DragOriginSlot;

        if (sourceSlot == this)
        {
            SetCurrentNumber(droppedNumber);
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
            return;
        }

        // Si se arrastra desde el panel de numeros a un slot ocupado, mantener el estado actual.
        if (sourceSlot == null)
        {
            droppedNumber.ReturnToOrigin();
            return;
        }

        DraggableNumber displacedNumber = currentNumber;

        SetCurrentNumber(droppedNumber);
        sourceSlot.ReceiveSwappedNumber(displacedNumber);

        GameNumberR.Instance?.CheckWinCondition();
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

    private void ReceiveSwappedNumber(DraggableNumber number)
    {
        currentNumber = number;
        number.PlaceInSlot(this);
    }

    public int? GetCurrentNumber()
    {
        return currentNumber != null ? currentNumber.numero : null;
    }
}
