using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    [Header("Número actualmente colocado")]
    public DraggableNumber currentNumber;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableNumber droppedNumber = eventData.pointerDrag?.GetComponent<DraggableNumber>();

        if (droppedNumber != null && currentNumber == null)
        {
            droppedNumber.transform.SetParent(transform);
            droppedNumber.transform.localPosition = Vector3.zero;

            currentNumber = droppedNumber;

            // Usamos GameNumberR
            GameNumberR.Instance.CheckWinCondition();
        }
    }

    public void ClearSlot()
    {
        currentNumber = null;
    }

    public int? GetCurrentNumber()
    {
        return currentNumber != null ? currentNumber.numero : null;
    }
}
