using UnityEngine;

public class Chest : MonoBehaviour
{
    public GameObject wirePuzzlePanel; // Панель мини-игры (WirePuzzlePanel, на ней скрипт WirePuzzle)
    public GameObject lootPrefab;      // Префаб лута (файл/монетка)
    public Transform lootSpawnPoint;   // Где спавнить лут (можно сам сундук)
    public int minLoot = 1;
    public int maxLoot = 3;

    [Header("Levitating Settings")]
    public float levitateHeight = 0.15f;   // высота левитации (юнитов)
    public float levitateSpeed = 1.7f;     // скорость качания

    private bool canOpen = false;
    private bool isOpened = false;
    private float levitateTimeOffset;
    private Vector3 startPos;

    void Start()
    {
        // Сохраняем стартовую позицию для плавного возврата
        startPos = transform.position;
        levitateTimeOffset = Random.Range(0f, 99f);
    }

    void Update()
    {
        // Левитация, пока сундук жив
        if (!isOpened)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * levitateSpeed + levitateTimeOffset) * levitateHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }

        // Активация мини-игры
        if (canOpen && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            if (wirePuzzlePanel != null)
            {
                wirePuzzlePanel.SetActive(true);   // Включаем панель мини-игры
                Time.timeScale = 0f;               // Ставим игру на паузу
                // Передаём ссылку на этот сундук в WirePuzzle:
                WirePuzzle puzzle = wirePuzzlePanel.GetComponent<WirePuzzle>();
                if (puzzle != null)
                    puzzle.targetChest = this;
            }
        }
    }

    // Этот метод вызывается WirePuzzle после победы в мини-игре!
    public void UnlockChest()
    {
        if (isOpened) return;

        isOpened = true;

        // Отключаем левитацию (чтобы не дёргался)
        // и фиксируем позицию для анимации
        LeanTween.cancel(gameObject);
        transform.position = new Vector3(startPos.x, startPos.y, startPos.z);

        // COLLPASE-АНИМАЦИЯ!
        float collapseTime = 0.5f;
        LeanTween.scale(gameObject, Vector3.one * 1.15f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.zero, collapseTime)
                .setEase(LeanTweenType.easeInBack)
                .setOnComplete(() =>
                {
                    // Спавним лут после анимации
                    DropLoot();
                    // Скрываем сундук (можно удалить, если надо)
                    gameObject.SetActive(false);
                });
            });

        // Скрываем мини-игру (на всякий)
        if (wirePuzzlePanel != null)
            wirePuzzlePanel.SetActive(false);

        // Возвращаем время в игре
        Time.timeScale = 1f;
    }

    void DropLoot()
    {
        int lootCount = Random.Range(minLoot, maxLoot + 1);
        for (int i = 0; i < lootCount; i++)
        {
            GameObject loot = Instantiate(lootPrefab, lootSpawnPoint.position, Quaternion.identity);

            // Делаем выплёвывание лута: разлёт в разные стороны
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float power = Random.Range(2f, 4f);
            Rigidbody2D rb = loot.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(randomDir * power, ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpen = true;
            // Включи UI подсказку "Press E to hack chest" если надо
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpen = false;
            // Выключи UI подсказку если надо
        }
    }
}
