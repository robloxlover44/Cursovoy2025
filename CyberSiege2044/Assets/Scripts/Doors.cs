using UnityEngine;
using TMPro;
using static LeanTween;

public class DoorController : MonoBehaviour
{
    [SerializeField] private int requiredShards = 10; // Необходимое количество осколков
    [SerializeField] private TMP_Text shardText; // Текст для отображения "X/Y"
    [SerializeField] private float shakeDuration = 0.5f; // Длительность тряски
    [SerializeField] private float shakeStrength = 0.1f; // Сила тряски

    private bool isPlayerNear = false; // Флаг, находится ли игрок рядом
    private Color originalColor; // Исходный цвет текста

    void Start()
    {
        if (shardText == null)
        {
            Debug.LogError("ShardText is not assigned in the inspector!");
            return;
        }

        // Сохраняем исходный цвет текста
        originalColor = shardText.color;

        // Инициализируем LeanTween
        LeanTween.init();

        // Обновляем текст при старте
        UpdateShardText();
    }

    void Update()
    {
        // Проверяем нажатие E, если игрок рядом
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager.Instance is null!");
            return;
        }

        int playerShards = PlayerDataManager.Instance.GetShards();

        if (playerShards >= requiredShards)
        {
            // Успешное открытие: тратим осколки и убираем дверь
            PlayerDataManager.Instance.SpendShards(requiredShards);
            Destroy(gameObject); // Дверь исчезает
            Debug.Log("Door opened successfully!");
        }
        else
        {
            // Не хватает осколков: анимация текста
            AnimateFailure();
            Debug.Log("Not enough shards to open the door!");
        }

        // Обновляем текст после попытки
        UpdateShardText();
    }

    private void UpdateShardText()
    {
        if (PlayerDataManager.Instance != null && shardText != null)
        {
            int playerShards = PlayerDataManager.Instance.GetShards();
            shardText.text = $"{playerShards}/{requiredShards}";
        }
    }

    private void AnimateFailure()
    {
        if (shardText == null) return;

        // Отменяем предыдущие анимации
        LeanTween.cancel(shardText.gameObject);

        // Анимация изменения цвета на красный и обратно
        LeanTween.value(shardText.gameObject, originalColor, Color.red, shakeDuration / 2)
            .setOnUpdate((Color val) => shardText.color = val)
            .setLoopPingPong(1); // Один цикл туда-обратно

        // Анимация тряски
        LeanTween.moveX(shardText.gameObject, shardText.transform.position.x + shakeStrength, shakeDuration / 2)
            .setEase(LeanTweenType.easeShake)
            .setLoopCount(2); // Тряска дважды
    }

    // Проверка, находится ли игрок рядом с дверью
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            UpdateShardText(); // Обновляем текст, когда игрок подошел
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}