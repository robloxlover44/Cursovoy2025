using UnityEngine;
using UnityEngine.EventSystems;

public class WirePoint : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Color wireColor;
    public bool isLeftPoint;
    public int pointIndex;

    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public WirePuzzle parentPuzzle;

    public void OnPointerDown(PointerEventData eventData)
    {
        // ТОЛЬКО для инициации drag
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isLeftPoint ||isConnected|| parentPuzzle.isDragging)
            return;
        parentPuzzle.StartDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Не нужно здесь — Update() в WirePuzzle уже рисует линию
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentPuzzle.isDragging)
            parentPuzzle.EndDrag();
    }
}