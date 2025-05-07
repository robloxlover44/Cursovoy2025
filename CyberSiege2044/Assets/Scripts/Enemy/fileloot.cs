// MoneyPickup.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MoneyPickup : MonoBehaviour
{
    [Header("Money Amount Range")]
    [Tooltip("Минимальное количество денег")]
    public int minAmount = 5;
    [Tooltip("Максимальное количество денег")]
    public int maxAmount = 20;

    private int amount;

    /// <summary>
    /// Позволяет внешнему коду (например, ChaseEnemy) задать точную сумму.
    /// </summary>
    public void SetAmount(int val)
    {
        amount = val;
    }

    void Start()
    {
        // Если сумма не была задана SetAmount, генерируем случайно из инспекторного диапазона
        if (amount == 0)
            amount = Random.Range(minAmount, maxAmount + 1);

        // Убедимся, что коллайдер настроен как триггер
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Добавляем деньги в PlayerData и сохраняем
            PlayerDataManager.Instance.AddMoney(amount);

            // Показываем анимированный текст через UIManager
            UIManager.Instance.ShowLootText(amount, "files");

            // Уничтожаем объект лута
            Destroy(gameObject);
        }
    }
}
