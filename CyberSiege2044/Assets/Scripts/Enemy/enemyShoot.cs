using UnityEngine;
using System.Collections;
using static LeanTween; // Для анимации выпадения лута

public class ShooterEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Shooting }

    [Header("Trigger Settings")]
    public Collider2D triggerZone;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 5f;
    public float fireRate = 1f;

    [Header("Health Settings")]
    public int maxHealth = 100;

    [Header("Animation Settings")]
    public Sprite[] idleAnimation;
    public Sprite[] shootAnimation;
    public Sprite[] deathAnimation;
    public float frameRate = 0.15f;

    [Header("Loot Drop Settings")]
    public GameObject shardLootPrefab;            // Префаб осколков
    public GameObject moneyLootPrefab;            // Префаб денег
    public Vector2 moneyRange = new Vector2(5, 20);// Диапазон денег [min,max]
    public float shardDropChance = 0.5f;          // Шанс выпадения осколков
    public float dropRadius = 1f;                // Радиус разброса
    public float fallDuration = 0.7f;            // Длительность падения
    public float bounceHeight = 0.3f;            // Высота отскока
    public LeanTweenType fallEase = LeanTweenType.easeInQuad;
    public LeanTweenType bounceEase = LeanTweenType.easeOutBounce;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private EnemyState currentState = EnemyState.Idle;
    private Coroutine animationCoroutine;
    private int currentHealth;
    private float nextFireTime;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;

        currentHealth = maxHealth;
        StartAnimation(idleAnimation);
    }

    void Update()
    {
        if (currentState == EnemyState.Shooting && player != null)
        {
            ShootAtPlayer();
            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Idle)
        {
            currentState = EnemyState.Shooting;
            StartAnimation(shootAnimation);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Shooting)
        {
            currentState = EnemyState.Idle;
            StartAnimation(idleAnimation);
        }
    }

    void ShootAtPlayer()
    {
        if (Time.time >= nextFireTime)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            var projectile = bullet.GetComponent<PlayerDamagingProjectile>();
            if (projectile != null)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                projectile.SetDirection(dir);
            }
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                rb.linearVelocity = dir * bulletSpeed;
            }
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        currentState = EnemyState.Idle;
        StopAllCoroutines();
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(PlayDeathAnimation());
    }

    IEnumerator PlayDeathAnimation()
    {
        foreach (Sprite frame in deathAnimation)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }

        // После анимации смерти спавним лут
        SpawnLoot();
        Destroy(gameObject);
    }

    // Логика выпадения и анимации лута
    private void SpawnLoot()
    {
        // Осколки
        if (shardLootPrefab != null && Random.value <= shardDropChance)
            AnimateLoot(shardLootPrefab, 1, false);

        // Деньги
        if (moneyLootPrefab != null)
        {
            int amount = Random.Range((int)moneyRange.x, (int)moneyRange.y + 1);
            AnimateLoot(moneyLootPrefab, amount, true);
        }
    }

    private void AnimateLoot(GameObject prefab, int value, bool isMoney)
    {
        Vector3 origin = transform.position;
        GameObject loot = Instantiate(prefab, origin, Quaternion.identity);

        // Передаём сумму для денег
        if (isMoney)
        {
            var mp = loot.GetComponent<MoneyPickup>();
            if (mp != null) mp.SetAmount(value);
        }

        // Случайная цель в радиусе
        Vector2 offset = Random.insideUnitCircle.normalized * dropRadius;
        Vector3 targetPos = origin + (Vector3)offset;

        // Масштаб от 0 до оригинала
        Vector3 originalScale = loot.transform.localScale;
        loot.transform.localScale = Vector3.zero;
        LeanTween.scale(loot, originalScale, fallDuration * 0.5f).setEase(LeanTweenType.easeOutBack);

        // Падение и отскок
        LTSeq seq = LeanTween.sequence();
        seq.append(() => LeanTween.move(loot, targetPos, fallDuration).setEase(fallEase));
        seq.append(() => LeanTween.moveY(loot, targetPos.y + bounceHeight, fallDuration * 0.5f).setEase(bounceEase));
        seq.append(() => LeanTween.moveY(loot, targetPos.y, fallDuration * 0.5f).setEase(LeanTweenType.easeInQuad));
    }

    void StartAnimation(Sprite[] frames)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateSprite(frames));
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
}