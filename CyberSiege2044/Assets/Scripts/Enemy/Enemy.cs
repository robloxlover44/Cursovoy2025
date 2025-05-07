using UnityEngine;
using System.Collections;
using static LeanTween; // убедись, что плагин подключён

public class ChaseEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Exploding }

    [Header("Trigger Settings")]
    public Collider2D triggerZone;

    [Header("Movement Settings")]
    public float chaseSpeed = 4f;
    public float stoppingDistance = 0.1f;

    [Header("Damage Settings")]
    public int damage = 10;
    public int maxHealth = 150;

    [Header("Animation Settings")]
    public Sprite[] runAnimation;
    public Sprite[] explodeAnimation;
    public float frameRate = 0.15f;

    [Header("Loot Drop Settings")]
    [Tooltip("Префаб осколков")]
    public GameObject shardLootPrefab;
    [Tooltip("Префаб денег")]
    public GameObject moneyLootPrefab;
    [Tooltip("Диапазон суммы денег [min, max]")]
    public Vector2 moneyRange = new Vector2(5, 20);
    [Tooltip("Шанс выпадения осколков")]
    public float shardDropChance = 0.5f;

    [Header("Drop Animation Settings")]
    public float dropRadius = 1f;
    public float fallDuration = 0.7f;
    public float bounceHeight = 0.3f;
    public LeanTweenType fallEase = LeanTweenType.easeInQuad;
    public LeanTweenType bounceEase = LeanTweenType.easeOutBounce;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private EnemyState currentState = EnemyState.Idle;
    private Coroutine animationCoroutine;
    private int currentHealth;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();

        triggerZone.isTrigger = true;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentState == EnemyState.Chasing && player != null)
            ChasePlayer();
    }

    void ChasePlayer()
    {
        if (Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * chaseSpeed * Time.deltaTime;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Idle)
        {
            currentState = EnemyState.Chasing;
            StartAnimation(runAnimation);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentState != EnemyState.Exploding)
        {
            collision.gameObject.GetComponent<PlayerController>()?.TakeDamage(damage);
            Die();
        }
    }

    IEnumerator AnimateSprite(Sprite[] frames)
    {
        int index = 0;
        while (true)
        {
            spriteRenderer.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }

    IEnumerator PlayExplosion()
    {
        GetComponent<Collider2D>().enabled = false;
        chaseSpeed = 0;

        foreach (Sprite frame in explodeAnimation)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }

        SpawnLoot();
        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Exploding) return;
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        currentState = EnemyState.Exploding;
        StopAllCoroutines();
        StartCoroutine(PlayExplosion());
    }

    private void SpawnLoot()
    {
        // Осколки с шансом
        if (shardLootPrefab != null && Random.value <= shardDropChance)
            AnimateLoot(shardLootPrefab, 1, false);

        // Деньги всегда (или можно добавить шанс)
        if (moneyLootPrefab != null)
        {
            int amount = Random.Range((int)moneyRange.x, (int)moneyRange.y + 1);
            AnimateLoot(moneyLootPrefab, amount, true);
        }
    }

    // Анимирует выпадение одного префаба
    private void AnimateLoot(GameObject prefab, int value, bool isMoney)
{
    Vector3 origin = transform.position;
    GameObject loot = Instantiate(prefab, origin, Quaternion.identity);

    // Запоминаем оригинальный scale из префаба
    Vector3 originalScale = loot.transform.localScale;
    // Сразу ставим 0, чтобы эффект «выпрыгивания»
    loot.transform.localScale = Vector3.zero;

    if (isMoney)
    {
        var mp = loot.GetComponent<MoneyPickup>();
        if (mp != null) mp.SetAmount(value);
    }

    // Целевая точка в радиусе
    Vector2 offset = Random.insideUnitCircle.normalized * dropRadius;
    Vector3 targetPos = origin + (Vector3)offset;

    // Появление: масштабируем до оригинала
    LeanTween.scale(loot, originalScale, fallDuration * 0.5f)
             .setEase(LeanTweenType.easeOutBack);

    // Падение и отскок
    LTSeq seq = LeanTween.sequence();
    seq.append(() =>
    {
        LeanTween.move(loot, targetPos, fallDuration).setEase(fallEase);
    });
    seq.append(() =>
    {
        LeanTween.moveY(loot, targetPos.y + bounceHeight, fallDuration * 0.5f).setEase(bounceEase);
    });
    seq.append(() =>
    {
        LeanTween.moveY(loot, targetPos.y, fallDuration * 0.5f).setEase(LeanTweenType.easeInQuad);
    });

    // Убираем бесконечное вращение (оно просто не создаётся)
}


    private void StartAnimation(Sprite[] frames)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateSprite(frames));
    }
}
