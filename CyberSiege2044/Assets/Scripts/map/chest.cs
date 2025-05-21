using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Дроп")]
    public GameObject fileLootPrefab; // Префаб fileloot (MoneyPickup/FilePickup)
    public int minDrops = 2;
    public int maxDrops = 3;
    public int minMoneyPerDrop = 5;
    public int maxMoneyPerDrop = 20;

    public float dropRadius = 1f;
    public float fallDuration = 0.7f;
    public float bounceHeight = 0.3f;
    public LeanTweenType fallEase = LeanTweenType.easeInQuad;
    public LeanTweenType bounceEase = LeanTweenType.easeOutBounce;

    [Header("Левитирование")]
    public float levitateDistance = 0.13f;
    public float levitateDuration = 1.1f;

    [Header("Открытие")]
    public float growScale = 1.15f;   // во сколько раз увеличится перед схлопыванием
    public float growTime = 0.11f;
    public float shrinkTime = 0.13f;

    private bool canOpen = false;
    private bool isOpened = false;
    private Vector3 initialPos;
    private Vector3 initialScale;

    void Start()
    {
        // Сохраняем стартовые значения
        initialPos = transform.position;
        initialScale = transform.localScale;

        // Запускаем бесконечное левитирование через LeanTween
        Levitate();
    }

    void Levitate()
    {
        LeanTween.moveY(gameObject, initialPos.y + levitateDistance, levitateDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) canOpen = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) canOpen = false;
    }

    void Update()
    {
        if (canOpen && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;
        LeanTween.cancel(gameObject); // отменяем левитирование

        // Сначала — небольшой рост
        LeanTween.scale(gameObject, initialScale * growScale, growTime)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => {
                // Затем быстрое схлопывание
                LeanTween.scale(gameObject, Vector3.zero, shrinkTime)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() => {
                        // Дропаем файлы
                        int drops = Random.Range(minDrops, maxDrops + 1);
                        for (int i = 0; i < drops; i++)
                        {
                            int value = Random.Range(minMoneyPerDrop, maxMoneyPerDrop + 1);
                            AnimateLoot(fileLootPrefab, value);
                        }
                        Destroy(gameObject);
                    });
            });
    }

    void AnimateLoot(GameObject prefab, int value)
    {
        Vector3 origin = transform.position;
        GameObject loot = Instantiate(prefab, origin, Quaternion.identity);

        // Присваиваем значение FileLoot'у (MoneyPickup/FilePickup/как у тебя)
        var pickup = loot.GetComponent<MoneyPickup>(); // или FilePickup
        if (pickup != null)
            pickup.SetAmount(value);

        Vector2 offset = Random.insideUnitCircle.normalized * dropRadius;
        Vector3 targetPos = origin + (Vector3)offset;

        Vector3 originalScale = loot.transform.localScale;
        loot.transform.localScale = Vector3.zero;
        LeanTween.scale(loot, originalScale, fallDuration * 0.5f).setEase(fallEase);

        var seq = LeanTween.sequence();
        seq.append(() => LeanTween.move(loot, targetPos, fallDuration).setEase(fallEase));
        seq.append(() => LeanTween.moveY(loot, targetPos.y + bounceHeight, fallDuration * 0.5f).setEase(bounceEase));
        seq.append(() => LeanTween.moveY(loot, targetPos.y, fallDuration * 0.5f).setEase(LeanTweenType.easeInQuad));
    }
}
