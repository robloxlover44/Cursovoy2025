using UnityEngine;
using TMPro;

public class MoneyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText; // Текст для отображения денег

    void Start()
    {
        UpdateMoneyText(); // Обновляем текст при запуске
    }

    void OnEnable()
    {
        // Подписываемся на изменения денег (если они будут добавлены в PlayerDataManager)
        UpdateMoneyText(); // Обновляем сразу при активации
    }

    void Update()
    {
        UpdateMoneyText(); // Обновляем текст каждый кадр
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = PlayerDataManager.Instance.GetMoney().ToString();
        }
    }
}