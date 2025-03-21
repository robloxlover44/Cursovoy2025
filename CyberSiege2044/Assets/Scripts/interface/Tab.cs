using UnityEngine;
using TMPro;
using static LeanTween;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // Объект окна
    [SerializeField] private float animationDuration = 0.5f; // Длительность анимации
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text shardsText;
    [SerializeField] private TMP_Text healthText; // Добавляем ссылку на TextMeshPro для здоровья
    [SerializeField] private Button resetButton; // Ссылка на кнопку сброса

    private bool isVisible = false;
    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;
    private CanvasGroup canvasGroup;

    void Start()
    {
        LeanTween.init();

        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned!");
            return;
        }

        if (moneyText == null || shardsText == null || healthText == null)
        {
            Debug.LogError("MoneyText, ShardsText or HealthText is not assigned in the inspector!");
        }

        if (resetButton == null)
        {
            Debug.LogError("ResetButton is not assigned in the inspector!");
        }
        else
        {
            resetButton.onClick.AddListener(ResetPlayerData);
        }

        canvasGroup = targetObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;

        RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
        hiddenPosition = rectTransform.anchoredPosition + new Vector2(Screen.width, 0);
        visiblePosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = hiddenPosition;

        targetObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && targetObject != null)
        {
            ToggleVisibility();
        }
    }

    private void ToggleVisibility()
    {
        LeanTween.cancel(targetObject);

        RectTransform rectTransform = targetObject.GetComponent<RectTransform>();

        if (isVisible)
        {
            LeanTween.alphaCanvas(canvasGroup, 0f, animationDuration * 0.5f);
            LeanTween.moveX(rectTransform, hiddenPosition.x, animationDuration).setOnComplete(() => targetObject.SetActive(false));
        }
        else
        {
            targetObject.SetActive(true);
            rectTransform.anchoredPosition = hiddenPosition;
            LeanTween.moveX(rectTransform, visiblePosition.x, animationDuration);
            LeanTween.alphaCanvas(canvasGroup, 1f, animationDuration * 0.5f);
            UpdateCurrencyDisplay();
        }

        isVisible = !isVisible;
    }

    private void UpdateCurrencyDisplay()
    {
        if (PlayerDataManager.Instance != null && moneyText != null && shardsText != null && healthText != null)
        {
            moneyText.text = PlayerDataManager.Instance.GetMoney().ToString();
            shardsText.text = PlayerDataManager.Instance.GetShards().ToString();
            healthText.text = PlayerDataManager.Instance.GetHealth().ToString();
        }
    }

    private void ResetPlayerData()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetData();
            UpdateCurrencyDisplay();
            Debug.Log("Player data reset to zero!");
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance is null, cannot reset data!");
        }
    }

    void OnDestroy()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetPlayerData);
        }
    }
} 
