using UnityEngine;
using UnityEngine.EventSystems;

public class WirePoint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Color wireColor;     // Для визуала
    public int wireIndex;       // Индекс, по которому ищем пару!
    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public WirePuzzle parentPuzzle;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isConnected && parentPuzzle != null && !parentPuzzle.isDragging)
            parentPuzzle.StartDrag(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (parentPuzzle != null && parentPuzzle.isDragging)
            parentPuzzle.EndDrag();
    }
}
