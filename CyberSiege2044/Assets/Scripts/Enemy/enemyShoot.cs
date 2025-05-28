using UnityEngine;
using System.Collections;
using static LeanTween;

public class TurretController : MonoBehaviour
{
    [Header("Refs")]
    public Transform turretBody; // пустышка Turret с pivot по центру базы
    public Transform firePoint;  // fp — на конце дула
    public Collider2D triggerZone;
    private SpriteRenderer turretRenderer;
    private SpriteRenderer platformRenderer;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 7f;
    public float fireRate = 1f;

    [Header("Health Settings")]
    public float maxHealth = 100;
    private float currentHealth;

    [Header("Death Animation Settings")]
    public Sprite[] deathAnimation;         // Массив кадров анимации смерти
    public float deathFrameRate = 0.15f;    // Скорость анимации смерти

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

    [Header("Player Detection")]
    public LayerMask playerLayer;

    [Header("Turret Settings")]
    [SerializeField] float rotationSpeed = 720f;

    // Новые массивы спрайтов для тревоги
    [Header("Alert Sprites")]
    public Sprite[] platformAlertSprites;  // Спрайты платформы для тревоги
    public Sprite[] turretAlertSprites;    // Спрайты турели для тревоги
    private Sprite[] defaultPlatformSprites; // Для восстановления оригинальных спрайтов
    private Sprite[] defaultTurretSprites;   // Для восстановления оригинальных спрайтов

    private Transform player;
    private bool playerInZone = false;
    private float nextFireTime = 0f;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;

        // Находим SpriteRenderer на платформе и турели
        turretRenderer = turretBody.GetComponentInChildren<SpriteRenderer>();
        platformRenderer = GetComponentInChildren<SpriteRenderer>();

        // Сохраняем оригинальные спрайты
        defaultTurretSprites = new Sprite[] { turretRenderer.sprite };
        defaultPlatformSprites = new Sprite[] { platformRenderer.sprite };
    }

    void Update()
    {
        if (isDead) return;
        if (playerInZone && player != null)
        {
            RotateTurretToPlayer();
            if (CanSeePlayer())
                TryShoot();
        }
    }

    void RotateTurretToPlayer()
    {
        Vector2 dir = (player.position - turretBody.position).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float currentAngle = turretBody.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        turretBody.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    bool CanSeePlayer()
    {
        Vector2 origin = firePoint.position;
        Vector2 target = player.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, target - origin, Vector2.Distance(origin, target), playerLayer | LayerMask.GetMask("Obstacle"));
        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;
        return false;
    }

    void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = firePoint.right * bulletSpeed;
        }
        nextFireTime = Time.time + 1f / fireRate;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            SwitchToAlertMode(); // Включаем режим тревоги
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            SwitchToNormalMode(); // Возвращаем обычные спрайты
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0)
            StartCoroutine(DieAndDropLootWithAnimation());
    }

    IEnumerator DieAndDropLootWithAnimation()
    {
        isDead = true;
        triggerZone.enabled = false;

        // Анимация смерти
        if (deathAnimation != null && deathAnimation.Length > 0 && turretRenderer != null)
        {
            foreach (Sprite frame in deathAnimation)
            {
                turretRenderer.sprite = frame;
                yield return new WaitForSeconds(deathFrameRate);
            }
        }

        SpawnLoot();
        yield return new WaitForSeconds(0.1f); // чуть-чуть задержки для плавности
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

    // Включаем тревогу, меняем на красные спрайты
    private void SwitchToAlertMode()
    {
        if (platformAlertSprites.Length > 0 && turretAlertSprites.Length > 0)
        {
            platformRenderer.sprite = platformAlertSprites[0]; // Установи первый спрайт тревоги платформы
            turretRenderer.sprite = turretAlertSprites[0];    // Установи первый спрайт тревоги турели
        }
    }

    // Возвращаем спрайты в нормальное состояние
    private void SwitchToNormalMode()
    {
        if (defaultPlatformSprites.Length > 0 && defaultTurretSprites.Length > 0)
        {
            platformRenderer.sprite = defaultPlatformSprites[0]; // Возвращаем оригинальный спрайт платформы
            turretRenderer.sprite = defaultTurretSprites[0];     // Возвращаем оригинальный спрайт турели
        }
    }
}
