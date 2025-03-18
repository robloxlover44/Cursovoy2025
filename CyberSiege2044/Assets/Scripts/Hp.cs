using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText; // Ссылка на TextMeshPro для отображения здоровья
    [SerializeField] private Slider healthSlider; // Ссылка на слайдер для HP-бара
    [SerializeField] private int maxHealth = 100; // Максимальное здоровье (настраивается в инспекторе)

    private void Start()
    {
        if (healthText == null)
        {
            Debug.LogError("HealthText is not assigned in the inspector!");
        }

        if (healthSlider == null)
        {
            Debug.LogError("HealthSlider is not assigned in the inspector!");
        }
        else
        {
            healthSlider.minValue = 0; // Устанавливаем минимальное значение слайдера
            healthSlider.maxValue = maxHealth; // Устанавливаем максимальное значение слайдера
        }

        UpdateHealthDisplay(); // Обновляем при старте
    }

    private void Update()
    {
        UpdateHealthDisplay(); // Обновляем каждый кадр (можно оптимизировать, если нужно)
    }

    private void UpdateHealthDisplay()
    {
        if (PlayerDataManager.Instance != null && healthText != null && healthSlider != null)
        {
            int currentHealth = PlayerDataManager.Instance.GetHealth();
            healthText.text = $"{currentHealth}/{maxHealth}"; // Отображаем текущее и максимальное здоровье
            healthSlider.value = currentHealth; // Синхронизируем слайдер с текущим здоровьем
        }
    }
}