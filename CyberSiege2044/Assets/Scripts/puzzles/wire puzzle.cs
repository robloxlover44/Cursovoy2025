using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class WirePuzzle : MonoBehaviour
{
    public WirePoint[] leftPoints;
    public WirePoint[] rightPoints;
    public GameObject lineImagePrefab;    // Префаб обычного UI Image (просто пустой Image)
    public RectTransform linesParent;     // Panel для линий
    [HideInInspector] public Chest targetChest;

    [HideInInspector] public bool isDragging = false;

    private WirePoint dragStartPoint;
    private Image currentLineImage;
    private List<Image> allLines = new List<Image>();
    private int correctConnections = 0;

    // === Главное! Сброс состояния при активации ===
    private void OnEnable()
    {
        isDragging = false;
        dragStartPoint = null;
        currentLineImage = null;
        correctConnections = 0;
        // Сброс соединений точек
        foreach (var p in leftPoints) p.isConnected = false;
        foreach (var p in rightPoints) p.isConnected = false;
        // Удалить старые линии
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
    }

    void Update()
    {
        if (isDragging && currentLineImage != null)
        {
            Vector2 start = ((RectTransform)dragStartPoint.transform).position;
            Vector2 end = Input.mousePosition;
            DrawUILine(currentLineImage.rectTransform, start, end, dragStartPoint.wireColor, 8f); // 8 — толщина линии
        }
    }

    public void StartDrag(WirePoint fromPoint)
    {
        Debug.Log("Start drag from: " + fromPoint.name);
        isDragging = true;
        dragStartPoint = fromPoint;

        GameObject go = Instantiate(lineImagePrefab, linesParent);
        currentLineImage = go.GetComponent<Image>();
        currentLineImage.color = fromPoint.wireColor;

        // Ставим линию в стартовую позицию
        Vector2 start = ((RectTransform)fromPoint.transform).position;
        DrawUILine(currentLineImage.rectTransform, start, start, fromPoint.wireColor, 8f);
        allLines.Add(currentLineImage);
    }

    public void EndDrag()
    {
        isDragging = false;
        if (currentLineImage == null || dragStartPoint == null) return;

        // Проверяем, попал ли на правую точку
        WirePoint matchPoint = null;
        foreach (WirePoint rp in rightPoints)
        {
            if (rp.isConnected) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)rp.transform, Input.mousePosition, null))
            {
                matchPoint = rp;
                break;
            }
        }

        if (matchPoint != null && matchPoint.wireColor == dragStartPoint.wireColor && !matchPoint.isConnected)
        {
            // Верно
            Vector2 start = ((RectTransform)dragStartPoint.transform).position;
            Vector2 end = ((RectTransform)matchPoint.transform).position;
            DrawUILine(currentLineImage.rectTransform, start, end, dragStartPoint.wireColor, 8f);
            dragStartPoint.isConnected = true;
            matchPoint.isConnected = true;
            correctConnections++;

            if (correctConnections == leftPoints.Length)
                PuzzleWin();
        }
        else
        {
            // Удаляем линию
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

    // ---- Функция рисования линии через Image ----
    void DrawUILine(RectTransform lineRect, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 differenceVector = end - start;
        lineRect.sizeDelta = new Vector2(differenceVector.magnitude, thickness);
        lineRect.pivot = new Vector2(0, 0.5f);
        lineRect.position = start;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
        lineRect.GetComponent<Image>().color = color;
    }
}
