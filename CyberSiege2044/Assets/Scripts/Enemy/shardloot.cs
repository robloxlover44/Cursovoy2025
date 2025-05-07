using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class shardloot : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Сколько осколков даёт этот лут")]
    public int shardAmount = 1;

    private void Reset()
    {
        // Убедимся, что триггер установлен
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // даём шардсы
            PlayerDataManager.Instance.AddShards(shardAmount);
            // показываем анимированный текст
            UIManager.Instance.ShowLootText(shardAmount, "shards");
            // и исчезаем
            Destroy(gameObject);
        }
    }
}
