using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Loot Text Settings")]
    public RectTransform lootTextParent;
    public TextMeshProUGUI lootTextPrefab;
    public float moveDuration = 0.5f;
    public float holdDuration = 0.5f;
    public float colorFlashRate = 0.05f;
    public float minOffset = 50f;
    public float maxOffset = 150f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Показывает всплывающий текст с анимацией через LeanTween
    /// </summary>
    public void ShowLootText(int amount, string type)
    {
        // Создаём копию текста в UI
        TextMeshProUGUI txt = Instantiate(lootTextPrefab, lootTextParent);
        txt.text = $"+{amount} {type}";

        // Рандомный отступ от центра
        float xOff = Random.Range(-maxOffset, maxOffset);
        float yOff = Random.Range(-maxOffset, maxOffset);

        Vector2 startPos = new Vector2(0, -minOffset) + new Vector2(xOff, yOff);
        Vector2 topPos   = Vector2.zero + new Vector2(xOff, yOff);
        Vector2 endPos   = new Vector2(0, -minOffset) + new Vector2(xOff, yOff);

        RectTransform rt = txt.GetComponent<RectTransform>();
        rt.anchoredPosition = startPos;

        // Scale эффект
        txt.transform.localScale = Vector3.zero;
        LeanTween.scale(txt.gameObject, Vector3.one, moveDuration * 0.5f).setEase(LeanTweenType.easeOutBack);

        // Движение вверх, зависание, вниз
        LeanTween.move(rt, topPos, moveDuration).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
        {
            LeanTween.delayedCall(holdDuration, () =>
            {
                LeanTween.move(rt, endPos, moveDuration).setEase(LeanTweenType.easeInCubic).setOnComplete(() =>
                {
                    Destroy(txt.gameObject);
                });
            });
        });

        // Радужная подсветка
        LeanTween.value(txt.gameObject, 0f, 1f, colorFlashRate)
            .setLoopPingPong()
            .setOnUpdate((float t) =>
            {
                txt.color = Color.HSVToRGB(t, 1f, 1f);
            });
    }
}