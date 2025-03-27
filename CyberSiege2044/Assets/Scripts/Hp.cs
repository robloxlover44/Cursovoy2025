using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText; // ������ �� TextMeshPro ��� ����������� ��������
    [SerializeField] private Slider healthSlider; // ������ �� ������� ��� HP-����
    [SerializeField] private int maxHealth = 100; // ������������ �������� (������������� � ����������)

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
            healthSlider.minValue = 0; // ������������� ����������� �������� ��������
            healthSlider.maxValue = maxHealth; // ������������� ������������ �������� ��������
        }

        UpdateHealthDisplay(); // ��������� ��� ������
    }

    private void Update()
    {
        UpdateHealthDisplay(); // ��������� ������ ���� (����� ��������������, ���� �����)
    }

    private void UpdateHealthDisplay()
    {
        if (PlayerDataManager.Instance != null && healthText != null && healthSlider != null)
        {
            int currentHealth = PlayerDataManager.Instance.GetHealth();
            healthText.text = $"{currentHealth}"; // ���������� ������� � ������������ ��������
            healthSlider.value = currentHealth; // �������������� ������� � ������� ���������
        }
    }
}