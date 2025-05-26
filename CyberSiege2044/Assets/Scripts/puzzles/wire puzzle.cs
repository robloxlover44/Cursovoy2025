using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WirePuzzle : MonoBehaviour
{
    public WirePoint[] leftPoints;
    public WirePoint[] rightPoints;
    public GameObject lineImagePrefab;
    public RectTransform linesParent;
    [HideInInspector] public Chest targetChest;
    [HideInInspector] public DoorController doorToOpen; // <- ДЛЯ ДВЕРИ

    [Header("Randomize Wires")]
    public Color[] possibleColors;

    [Header("Timer")]
    public float timerSeconds = 15f;
    public TextMeshProUGUI timerText;

    [Header("Animation")]
    public RectTransform panelToAnimate;
    public float popupScale = 1.08f;

    [HideInInspector] public bool isDragging = false;

    private WirePoint dragStartPoint;
    private Image currentLineImage;
    private List<Image> allLines = new List<Image>();
    private int correctConnections = 0;
    private Canvas canvas;

    private float timerCurrent = 0f;
    private bool timerActive = false;
    private bool puzzleFailed = false;
    private bool puzzleWon = false;

    private Vector3 panelOriginalScale;

    void Awake()
    {
        if (panelToAnimate != null)
            panelOriginalScale = panelToAnimate.localScale;
    }

    private void OnEnable()
    {
        isDragging = false;
        dragStartPoint = null;
        currentLineImage = null;
        correctConnections = 0;
        puzzleWon = false;
        foreach (var p in leftPoints) p.isConnected = false;
        foreach (var p in rightPoints) p.isConnected = false;
        foreach (var l in allLines) if (l) Destroy(l.gameObject);
        allLines.Clear();

        RandomizeWires();

        timerCurrent = timerSeconds;
        timerActive = true;
        puzzleFailed = false;
        if (timerText) timerText.gameObject.SetActive(true);

        AnimatePanelOpen();
    }

    void AnimatePanelOpen()
    {
        if (panelToAnimate != null)
        {
            panelToAnimate.localScale = Vector3.zero;
            LeanTween.scale(panelToAnimate, panelOriginalScale * popupScale, 0.33f)
                .setEase(LeanTweenType.easeOutBack)
                .setIgnoreTimeScale(true)
                .setOnComplete(() => {
                    LeanTween.scale(panelToAnimate, panelOriginalScale, 0.14f)
                        .setEase(LeanTweenType.easeInQuad)
                        .setIgnoreTimeScale(true);
                });
        }
    }

    void AnimatePanelClose(System.Action onComplete = null)
    {
        if (panelToAnimate != null)
        {
            LeanTween.scale(panelToAnimate, Vector3.zero, 0.38f)
                .setEase(LeanTweenType.easeInBack)
                .setIgnoreTimeScale(true)
                .setOnComplete(() => {
                    if (onComplete != null)
                        onComplete();
                });
        }
        else
        {
            if (onComplete != null)
                onComplete();
        }
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
        if (timerActive && !puzzleFailed && !puzzleWon)
        {
            timerCurrent -= Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(1f - timerCurrent / timerSeconds);

            if (timerText)
            {
                Color timerColor = Color.Lerp(Color.green, Color.red, t);
                timerText.color = timerColor;
                timerText.text = timerCurrent > 0 ? timerCurrent.ToString("F1") : "0.0";
            }

            if (timerCurrent <= 0f)
            {
                puzzleFailed = true;
                timerActive = false;
                OnPuzzleFailed();
            }
        }

        if (isDragging && currentLineImage != null)
        {
            Vector2 startAnchoredPos = linesParent.InverseTransformPoint(dragStartPoint.transform.position);

            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld = canvas.worldCamera.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = linesParent.position.z;
            Vector2 endAnchoredPos = linesParent.InverseTransformPoint(mouseWorld);

            DrawUILine(currentLineImage.rectTransform, startAnchoredPos, endAnchoredPos, dragStartPoint.wireColor, 8f);
        }

        if (puzzleWon && timerText)
        {
            timerText.text = "Great Job!";
            timerText.color = Color.HSVToRGB((Time.unscaledTime * 0.33f) % 1f, 1, 1);
        }
    }

    void RandomizeWires()
    {
        List<Color> colors = new List<Color>(possibleColors);
        List<Color> selectedColors = new List<Color>();

        int count = Mathf.Min(leftPoints.Length, rightPoints.Length, colors.Count);

        for (int i = 0; i < count; i++)
        {
            int rand = Random.Range(0, colors.Count);
            selectedColors.Add(colors[rand]);
            colors.RemoveAt(rand);
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < count; i++) indices.Add(i);

        for (int i = 0; i < count; i++)
        {
            int rand = Random.Range(i, count);
            int temp = indices[i];
            indices[i] = indices[rand];
            indices[rand] = temp;
        }

        for (int i = 0; i < count; i++)
        {
            int idx = indices[i];
            Color c = selectedColors[idx];
            leftPoints[i].wireIndex = idx;
            leftPoints[i].wireColor = c;
            leftPoints[i].GetComponent<Image>().color = c;
        }

        List<int> rightIndices = new List<int>();
        for (int i = 0; i < count; i++) rightIndices.Add(i);

        for (int i = 0; i < count; i++)
        {
            int rand = Random.Range(i, count);
            int temp = rightIndices[i];
            rightIndices[i] = rightIndices[rand];
            rightIndices[rand] = temp;
        }

        for (int i = 0; i < count; i++)
        {
            int idx = rightIndices[i];
            Color c = selectedColors[idx];
            rightPoints[i].wireIndex = idx;
            rightPoints[i].wireColor = c;
            rightPoints[i].GetComponent<Image>().color = c;
        }
    }

    public void StartDrag(WirePoint fromPoint)
    {
        isDragging = true;
        dragStartPoint = fromPoint;

        GameObject go = Instantiate(lineImagePrefab, linesParent);
        currentLineImage = go.GetComponent<Image>();
        currentLineImage.color = fromPoint.wireColor;

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
        timerActive = false;
        puzzleWon = true;

        if (timerText)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "Great Job!";
            timerText.color = Color.HSVToRGB((Time.unscaledTime * 0.33f) % 1f, 1, 1);
        }

        StartCoroutine(ClosePanelAfterDelay(true));
    }

    void OnPuzzleFailed()
    {
        if (timerText)
        {
            timerText.color = Color.red;
            timerText.text = "Loser!";
        }
        if (targetChest != null)
        {
            Destroy(targetChest.gameObject);
        }
        StartCoroutine(ClosePanelAfterDelay(false));
    }

    // Корутина для закрытия панели через сек после текста
    private System.Collections.IEnumerator ClosePanelAfterDelay(bool win)
    {
        float t = 1f;
        while (t > 0)
        {
            t -= Time.unscaledDeltaTime;
            yield return null;
        }

        AnimatePanelClose(() =>
        {
            if (timerText) timerText.gameObject.SetActive(false);

            if (win)
            {
                if (targetChest != null)
                {
                    targetChest.UnlockChest();
                    targetChest = null;
                }
                else if (doorToOpen != null)
                {
                    doorToOpen.OpenDoor(); // <- ДВЕРЬ ОТКРЫВАЕМ!
                    doorToOpen = null;
                }
            }
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        });
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
