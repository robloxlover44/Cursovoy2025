using UnityEngine;
using System.Collections;
using static LeanTween; // Для анимации выпадения лута

public class ShooterEnemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Shooting }

    [Header("Trigger Settings")]
    public Collider2D triggerZone;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;        // Настраиваемые точки патруля
    public float patrolSpeed = 2f;          // Скорость передвижения при патруле

    [Header("Patrol Animation Settings")]
    public Sprite[] patrolAnimation;        // Спрайты «ходьбы» при патруле
    public float patrolFrameRate = 0.5f;    // Частота смены кадров при патруле

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 5f;
    public float fireRate = 1f;

    [Header("Shooting Animation Settings")]
    public Sprite[] shootAnimation;
    public float shootFrameRate = 0.15f;

    [Header("Health Settings")]
    public int maxHealth = 100;             // Максимальное здоровье

    [Header("Death Animation Settings")]
    public Sprite[] deathAnimation;
    public float deathFrameRate = 0.15f;

    [Header("Loot Drop Settings")]
    public GameObject shardLootPrefab;
    public GameObject moneyLootPrefab;
    public Vector2 moneyRange = new Vector2(5, 20);
    public float shardDropChance = 0.5f;
    public float dropRadius = 1f;
    public float fallDuration = 0.7f;
    public float bounceHeight = 0.3f;
    public LeanTweenType fallEase = LeanTweenType.easeInQuad;
    public LeanTweenType bounceEase = LeanTweenType.easeOutBounce;

    // --- внутренние поля ---
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private EnemyState currentState = EnemyState.Patrol;
    private Coroutine animationCoroutine;
    private int currentHealth;
    private float nextFireTime;

    // Патруль
    private Vector3[] patrolPositions;
    private int currentPatrolIndex = 0;
    private float currentFrameRate;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;

        currentHealth = maxHealth;

        CachePatrolPositions();
        EnterPatrol();
    }

    void Update()
    {
        if (currentState == EnemyState.Patrol)
        {
            Patrol();
        }
        else // Shooting
        {
            ShootAtPlayer();
            RotateTowardsPlayer();
        }
    }

    private void CachePatrolPositions()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolPositions = new Vector3[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
                patrolPositions[i] = patrolPoints[i].position;
        }
    }

    private void Patrol()
    {
        if (patrolPositions == null || patrolPositions.Length < 2)
    {
        // Не крутимся
        transform.localEulerAngles = Vector3.zero;
        return;
    }


        Vector2 currentPos = transform.position;
        Vector2 targetPos = patrolPositions[currentPatrolIndex];
        float dist = Vector2.Distance(currentPos, targetPos);
        const float arrivalThreshold = 0.1f;

        if (dist > arrivalThreshold)
        {
            Vector2 dir = (targetPos - currentPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0f, 0f, angle);

            transform.position = Vector2.MoveTowards(
                currentPos,
                targetPos,
                patrolSpeed * Time.deltaTime
            );
        }
        else
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPositions.Length;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Patrol)
            EnterShooting();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Shooting)
            EnterPatrol();
    }

    private void ShootAtPlayer()
    {
        if (Time.time < nextFireTime) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (bullet.TryGetComponent<PlayerDamagingProjectile>(out var projectile))
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            projectile.SetDirection(dir);
        }
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            rb.linearVelocity = dir * bulletSpeed;
        }

        nextFireTime = Time.time + 1f / fireRate;
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        StopAllCoroutines();
        triggerZone.enabled = false;
        StartCoroutine(PlayDeathAnimation());
    }

    IEnumerator PlayDeathAnimation()
    {
        // Используем deathFrameRate
        foreach (Sprite frame in deathAnimation)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(deathFrameRate);
        }

        SpawnLoot();
        Destroy(gameObject);
    }

    private void SpawnLoot()
    {
        if (shardLootPrefab != null && Random.value <= shardDropChance)
            AnimateLoot(shardLootPrefab, 1, false);

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

        if (isMoney && loot.TryGetComponent<MoneyPickup>(out var mp))
            mp.SetAmount(value);

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

    private void EnterPatrol()
    {
        currentState = EnemyState.Patrol;
        currentFrameRate = patrolFrameRate;
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateSprite(patrolAnimation, patrolFrameRate));
    }

    private void EnterShooting()
    {
        currentState = EnemyState.Shooting;
        currentFrameRate = shootFrameRate;
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateSprite(shootAnimation, shootFrameRate));
    }

    private IEnumerator AnimateSprite(Sprite[] frames, float rate)
    {
        int idx = 0;
        while (true)
        {
            spriteRenderer.sprite = frames[idx];
            idx = (idx + 1) % frames.Length;
            yield return new WaitForSeconds(rate);
        }
    }
}
