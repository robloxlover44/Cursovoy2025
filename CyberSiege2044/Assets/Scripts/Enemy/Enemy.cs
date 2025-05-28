using UnityEngine;
using System.Collections;
using static LeanTween; // убедись, что плагин подключён

public class ChaseEnemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chasing, Exploding }
    private int patrolDirection = 1; // 1 — вперёд, -1 — назад

    [Header("Trigger Settings")]
    public Collider2D triggerZone;

    [Header("Movement Settings")]
    public float chaseSpeed = 4f;
    public float stoppingDistance = 0.1f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;      // назначаем точки в инспекторе
    public float patrolSpeed = 2f;        // скорость ходьбы при патруле
    public float patrolFrameRate = 0.3f;  // скорость анимации при патруле

    [Header("Damage Settings")]
    public int damage = 10;
    public int maxHealth = 150;

    [Header("Animation Settings")]
    public Sprite[] runAnimation;
    public Sprite[] explodeAnimation;
    public float frameRate = 0.15f;

    [Header("Loot Drop Settings")]
    public GameObject shardLootPrefab;
    public GameObject moneyLootPrefab;
    public Vector2 moneyRange = new Vector2(5, 20);
    public float shardDropChance = 0.5f;

    [Header("Drop Animation Settings")]
    public float dropRadius = 1f;
    public float fallDuration = 0.7f;
    public float bounceHeight = 0.3f;
    public LeanTweenType fallEase = LeanTweenType.easeInQuad;
    public LeanTweenType bounceEase = LeanTweenType.easeOutBounce;

    // внутренние поля
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private EnemyState currentState = EnemyState.Patrol;
    private Coroutine animationCoroutine;
    private float currentHealth;
    private int currentPatrolIndex = 0;
    private float currentFrameRate;

    // закэшированные мировые позиции патрульных точек
    private Vector3[] patrolPositions;

    // ДОБАВИЛ ДЛЯ ФИЗИКИ:
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>(); // <-- важно!

        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;

        currentHealth = maxHealth;
        currentFrameRate = frameRate;

        // кешируем глобальные позиции точек
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolPositions = new Vector3[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
                patrolPositions[i] = patrolPoints[i].position;
        }

        EnterPatrol();
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
        }
    }

    private void Patrol()
{
    if (patrolPositions == null || patrolPositions.Length < 2)
    {
        return;
    }

    Vector2 currentPos = rb.position;
    Vector2 targetPos = patrolPositions[currentPatrolIndex];
    float dist = Vector2.Distance(currentPos, targetPos);

    const float arrivalThreshold = 0.2f;
    if (dist > arrivalThreshold)
    {
        Vector2 dir = (targetPos - currentPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.localEulerAngles = new Vector3(0f, 0f, angle);

        rb.MovePosition(Vector2.MoveTowards(
            currentPos,
            targetPos,
            patrolSpeed * Time.fixedDeltaTime
        ));
    }
    else
    {
        // ПИНГ-ПОНГ патруль!
        currentPatrolIndex += patrolDirection;
        if (currentPatrolIndex >= patrolPositions.Length)
        {
            currentPatrolIndex = patrolPositions.Length - 2;
            patrolDirection = -1;
        }
        else if (currentPatrolIndex < 0)
        {
            currentPatrolIndex = 1;
            patrolDirection = 1;
        }
    }
}


    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 currentPos = rb.position; // rb!
        Vector2 targetPos = player.position;
        float dist = Vector2.Distance(currentPos, targetPos);

        if (dist > stoppingDistance)
        {
            Vector2 dir = (targetPos - currentPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // ФИЗИЧЕСКОЕ ДВИЖЕНИЕ:
            rb.MovePosition(currentPos + dir * chaseSpeed * Time.fixedDeltaTime);
        }
    }

    // ... дальше весь твой код без изменений!
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Patrol)
            EnterChase();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Chasing)
            EnterPatrol();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentState != EnemyState.Exploding)
        {
            collision.gameObject.GetComponent<PlayerController>()?.TakeDamage(damage);
            Die();
        }
    }

    public void TakeDamage(float dmg)
    {
        if (currentState == EnemyState.Exploding) return;
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        currentState = EnemyState.Exploding;
        StopAllCoroutines();
        StartCoroutine(PlayExplosion());
    }

    IEnumerator AnimateSprite(Sprite[] frames)
    {
        int idx = 0;
        while (true)
        {
            spriteRenderer.sprite = frames[idx];
            idx = (idx + 1) % frames.Length;
            yield return new WaitForSeconds(currentFrameRate);
        }
    }

    IEnumerator PlayExplosion()
    {
        GetComponent<Collider2D>().enabled = false;
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        foreach (Sprite frame in explodeAnimation)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameRate);
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

        Vector3 origScale = loot.transform.localScale;
        loot.transform.localScale = Vector3.zero;
        if (isMoney)
        {
            var mp = loot.GetComponent<MoneyPickup>();
            if (mp != null) mp.SetAmount(value);
        }

        Vector2 offset = Random.insideUnitCircle.normalized * dropRadius;
        Vector3 targetPos = origin + (Vector3)offset;

        LeanTween.scale(loot, origScale, fallDuration * 0.5f).setEase(fallEase);
        LTSeq seq = LeanTween.sequence();
        seq.append(() => LeanTween.move(loot, targetPos, fallDuration).setEase(fallEase));
        seq.append(() => LeanTween.moveY(loot, targetPos.y + bounceHeight, fallDuration * 0.5f).setEase(bounceEase));
        seq.append(() => LeanTween.moveY(loot, targetPos.y, fallDuration * 0.5f).setEase(LeanTweenType.easeInQuad));
    }

    private void StartAnimation(Sprite[] frames, float rate)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        currentFrameRate = rate;
        animationCoroutine = StartCoroutine(AnimateSprite(frames));
    }

    private void StartAnimation(Sprite[] frames)
    {
        StartAnimation(frames, frameRate);
    }

    private void EnterPatrol()
    {
        currentState = EnemyState.Patrol;
        StartAnimation(runAnimation, patrolFrameRate);
    }

    private void EnterChase()
    {
        currentState = EnemyState.Chasing;
        StartAnimation(runAnimation, frameRate);
    }
}
