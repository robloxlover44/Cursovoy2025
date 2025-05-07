using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindUIRefs();
    }

    private void TryFindUIRefs()
    {
        if (lootTextParent == null)
        {
            var go = GameObject.Find("LootTextParent");
            if (go != null)
                lootTextParent = go.GetComponent<RectTransform>();
        }

        if (lootTextPrefab == null)
        {
            // Альтернатива: можно найти префаб из ресурсов
            lootTextPrefab = Resources.Load<TextMeshProUGUI>("LootTextPrefab");
        }
    }

    public void ShowLootText(int amount, string type)
    {
        if (lootTextParent == null || lootTextPrefab == null)
        {
            Debug.LogWarning("Loot text references missing! Trying to rebind...");
            TryFindUIRefs();

            if (lootTextParent == null || lootTextPrefab == null)
            {
                Debug.LogError("Cannot show loot text: references not found.");
                return;
            }
        }

        TextMeshProUGUI txt = Instantiate(lootTextPrefab, lootTextParent);
        txt.text = $"+{amount} {type}";

        float xOff = Random.Range(-maxOffset, maxOffset);
        float yOff = Random.Range(-maxOffset, maxOffset);

        Vector2 startPos = new Vector2(0, -minOffset) + new Vector2(xOff, yOff);
        Vector2 topPos = Vector2.zero + new Vector2(xOff, yOff);
        Vector2 endPos = new Vector2(0, -minOffset) + new Vector2(xOff, yOff);

        RectTransform rt = txt.GetComponent<RectTransform>();
        rt.anchoredPosition = startPos;

        txt.transform.localScale = Vector3.zero;
        LeanTween.scale(txt.gameObject, Vector3.one, moveDuration * 0.5f).setEase(LeanTweenType.easeOutBack);

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

        LeanTween.value(txt.gameObject, 0f, 1f, colorFlashRate)
            .setLoopPingPong()
            .setOnUpdate((float t) =>
            {
                txt.color = Color.HSVToRGB(t, 1f, 1f);
            });
    }
}
