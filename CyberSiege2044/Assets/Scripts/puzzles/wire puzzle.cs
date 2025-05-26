using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WirePuzzle : MonoBehaviour
{
    public WirePoint[] leftPoints;
    public WirePoint[] rightPoints;
    public GameObject lineImagePrefab;    // Префаб UI Image
    public RectTransform linesParent;     // Панель для линий
    [HideInInspector] public Chest targetChest;

    [HideInInspector] public bool isDragging = false;

    private WirePoint dragStartPoint;
    private Image currentLineImage;
    private List<Image> allLines = new List<Image>();
    private int correctConnections = 0;
    private Canvas canvas;

    private void OnEnable()
    {
        isDragging = false;
        dragStartPoint = null;
        currentLineImage = null;
        correctConnections = 0;
        foreach (var p in leftPoints) p.isConnected = false;
        foreach (var p in rightPoints) p.isConnected = false;
        foreach (var l in allLines) if (l) Destroy(l.gameObject);
        allLines.Clear();
    }

    void Start()
    {
        for (int i = 0; i < leftPoints.Length; i++)
        {
            leftPoints[i].parentPuzzle = this;
            rightPoints[i].parentPuzzle = this;
        }
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
{
    if (isDragging && currentLineImage != null)
    {
        Vector2 startAnchoredPos = linesParent.InverseTransformPoint(dragStartPoint.transform.position);

        // КУРСОР! Преобразуем позицию мыши из screen в world point через камеру, потом в local
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = canvas.worldCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = linesParent.position.z; // Выровнять по Z
        Vector2 endAnchoredPos = linesParent.InverseTransformPoint(mouseWorld);

        DrawUILine(currentLineImage.rectTransform, startAnchoredPos, endAnchoredPos, dragStartPoint.wireColor, 8f);
    }
}



    public void StartDrag(WirePoint fromPoint)
{
    isDragging = true;
    dragStartPoint = fromPoint;

    GameObject go = Instantiate(lineImagePrefab, linesParent);
    currentLineImage = go.GetComponent<Image>();
    currentLineImage.color = fromPoint.wireColor;

    // Получаем локальную позицию точки относительно linesParent
    Vector2 startAnchoredPos = linesParent.InverseTransformPoint(dragStartPoint.transform.position);

    DrawUILine(currentLineImage.rectTransform, startAnchoredPos, startAnchoredPos, dragStartPoint.wireColor, 8f);
    allLines.Add(currentLineImage);
}



    public void EndDrag()
{
    isDragging = false;
    if (currentLineImage == null || dragStartPoint == null) return;

    WirePoint matchPoint = null;
    foreach (WirePoint rp in rightPoints)
    {
        if (rp.isConnected) continue;
        RectTransform rightRect = (RectTransform)rp.transform;
        if (RectTransformUtility.RectangleContainsScreenPoint(rightRect, Input.mousePosition, canvas.worldCamera))
        {
            matchPoint = rp;
            break;
        }
    }

    // СРАВНИВАЕМ ИМЕННО wireIndex!!!
    if (matchPoint != null && matchPoint.wireIndex == dragStartPoint.wireIndex && !matchPoint.isConnected)
    {
        Vector2 leftAnchored = linesParent.InverseTransformPoint(dragStartPoint.transform.position);
        Vector2 rightAnchored = linesParent.InverseTransformPoint(matchPoint.transform.position);

        DrawUILine(currentLineImage.rectTransform, leftAnchored, rightAnchored, dragStartPoint.wireColor, 8f);

        dragStartPoint.isConnected = true;
        matchPoint.isConnected = true;
        correctConnections++;

        if (correctConnections == leftPoints.Length)
            PuzzleWin();
    }
    else
    {
        Destroy(currentLineImage.gameObject);
        allLines.Remove(currentLineImage);
    }

    currentLineImage = null;
    dragStartPoint = null;
}



    private void PuzzleWin()
    {
        Debug.Log("Победа! Все провода соединены.");
        if (targetChest != null)
        {
            targetChest.UnlockChest();
            targetChest = null;
        }
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    void DrawUILine(RectTransform lineRect, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 differenceVector = end - start;
        lineRect.sizeDelta = new Vector2(differenceVector.magnitude, thickness);
        lineRect.pivot = new Vector2(0, 0.5f);
        lineRect.anchoredPosition = start;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
        lineRect.GetComponent<Image>().color = color;
    }
}
