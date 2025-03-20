using UnityEngine;
using TMPro;
using static LeanTween;
using UnityEngine.UI;

public class ToggleObject : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // Объект окна
    [SerializeField] private float animationDuration = 0.5f; // Длительность анимации
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeInOutQuad; // Тип анимации

    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text shardsText;
    [SerializeField] private TMP_Text healthText; // Добавляем ссылку на TextMeshPro для здоровья
    [SerializeField] private Button resetButton; // Ссылка на кнопку сброса

    private bool isVisible = false;

    void Start()
    {
        LeanTween.init();

        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned!");
            return;
        }

        if (moneyText == null || shardsText == null || healthText == null) // Добавляем проверку для healthText
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

        targetObject.transform.localScale = Vector3.zero;
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

        if (isVisible)
        {
            LeanTween.scale(targetObject, Vector3.zero, animationDuration)
                .setEase(easeType)
                .setOnComplete(() => targetObject.SetActive(false));
        }
        else
        {
            targetObject.SetActive(true);
            targetObject.transform.localScale = Vector3.zero;

            UpdateCurrencyDisplay();

            LeanTween.scale(targetObject, Vector3.one, animationDuration)
                .setEase(easeType);
        }

        isVisible = !isVisible;
    }

    private void UpdateCurrencyDisplay()
    {
        if (PlayerDataManager.Instance != null && moneyText != null && shardsText != null && healthText != null) // Добавляем healthText в проверку
        {
            moneyText.text = PlayerDataManager.Instance.GetMoney().ToString(); // Только число
            shardsText.text = PlayerDataManager.Instance.GetShards().ToString(); // Только число
            healthText.text = PlayerDataManager.Instance.GetHealth().ToString(); // Добавляем отображение здоровья
        }
    }

    private void ResetPlayerData()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetData(); // Сбрасываем данные
            UpdateCurrencyDisplay(); // Обновляем отображение
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