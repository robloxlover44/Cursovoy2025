using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthSlider;

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
            healthSlider.minValue = 0;
            if (PlayerDataManager.Instance != null)
            {
                healthSlider.maxValue = PlayerDataManager.Instance.GetMaxHealth();
                PlayerDataManager.Instance.OnHealthChanged += UpdateHealthDisplay; // Подписываемся на событие
            }
        }

        UpdateHealthDisplay(); // Начальное обновление
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnHealthChanged -= UpdateHealthDisplay; // Отписываемся при уничтожении
        }
    }

    private void UpdateHealthDisplay()
    {
        if (PlayerDataManager.Instance != null && healthText != null && healthSlider != null)
        {
            int currentHealth = PlayerDataManager.Instance.GetHealth();
            int maxHealth = PlayerDataManager.Instance.GetMaxHealth();

            healthText.text = $"{currentHealth}/{maxHealth}";
            healthSlider.value = currentHealth;
            healthSlider.maxValue = maxHealth;
        }
    }
}