using UnityEngine;
using System.Collections;

public class BossEnemy : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Boss Settings")]
    public int maxHealth = 1000;
    private int currentHealth;

    // --- Публичное свойство для HP бара ---
    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    [Header("Shooting")]
    public float fireRate = 1f;
    public float bulletSpeed = 7f;
    private float nextFireTime = 0f;

    [Header("Wave (Ult)")]
    public int waveBulletCount = 16;
    public float waveCooldown = 5f;

    [Header("Dash Attack")]
    public float dashCooldown = 4f;
    public float dashDuration = 0.25f;
    public float dashWarningTime = 0.7f;
    public GameObject dashIndicatorPrefab;

    [Header("Sound FX")]
    public AudioClip shootSfx;
    public AudioClip ultSfx;
    public AudioClip dashSfx;
    public AudioSource audioSource; // назначь сюда AudioSource в инспекторе

    [Header("Other")]
    public Collider2D triggerZone;

    private Transform player;
    private bool playerInZone;
    private Rigidbody2D rb;
    private bool isDashing;

    private Vector2 lastPlayerPos;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Поворот к игроку и стрельба (обычный режим)
        if (playerInZone && player != null && !isDashing)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (Time.time >= nextFireTime)
            {
                Shoot(dir);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playerInZone)
        {
            playerInZone = true;    
            StartCoroutine(WaveAttackRoutine());
            StartCoroutine(DashRoutine());
        }
    }

    void Shoot(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        var proj = bullet.GetComponent<bullet>();
        if (proj != null)
            proj.SetDirection(dir);

        var rb2d = bullet.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.linearVelocity = dir * bulletSpeed;

        // Звук выстрела
        if (shootSfx != null && audioSource != null)
            audioSource.PlayOneShot(shootSfx);
    }

    // --- Волна пуль (ультимейт) ---
    IEnumerator WaveAttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (playerInZone && currentHealth > 0)
        {
            yield return new WaitForSeconds(waveCooldown);
            FireBulletWave();
        }
    }
    void FireBulletWave()
    {
        // Звук ульты
        if (ultSfx != null && audioSource != null)
            audioSource.PlayOneShot(ultSfx);

        float step = 360f / waveBulletCount;
        for (int i = 0; i < waveBulletCount; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            var proj = bullet.GetComponent<bullet>();
            if (proj != null) proj.SetDirection(dir);
            var rb2d = bullet.GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.linearVelocity = dir * bulletSpeed;
        }
    }

    // --- Рывок ---
    IEnumerator DashRoutine()
    {
        while (playerInZone && currentHealth > 0)
        {
            yield return new WaitForSeconds(dashCooldown);

            GameObject dashIndicator = null;
            DashIndicator indicatorScript = null;

            // Создаём индикатор один раз до зарядки
            if (dashIndicatorPrefab)
            {
                dashIndicator = Instantiate(dashIndicatorPrefab);
                indicatorScript = dashIndicator.GetComponent<DashIndicator>();
                if (indicatorScript != null)
                    indicatorScript.Init(transform, player);
            }

            // Зарядка: обновляем позицию игрока и линию каждый кадр
            float timer = 0f;
            while (timer < dashWarningTime)
            {
                if (player != null)
                    lastPlayerPos = player.position; // Сохраняем последнюю позицию

                if (indicatorScript != null)
                    indicatorScript.Init(transform, player);

                timer += Time.deltaTime;
                yield return null;
            }

            if (dashIndicator)
                Destroy(dashIndicator);

            // Звук рывка
            if (dashSfx != null && audioSource != null)
                audioSource.PlayOneShot(dashSfx);

            // Рывок строго до lastPlayerPos!
            yield return StartCoroutine(DoDash());
        }
    }
    IEnumerator DoDash()
    {
        isDashing = true;
        float elapsed = 0f;
        Vector2 start = rb.position;
        Vector2 end = lastPlayerPos;

        while (elapsed < dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(start, end, elapsed / dashDuration));
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(end);
        isDashing = false;
    }

public void TakeDamage(float dmg)
{
    currentHealth -= (int)dmg;
    if (currentHealth <= 0)
    {
        currentHealth = 0;
        StartCoroutine(DestroyAfterDelay());
    }
}

IEnumerator DestroyAfterDelay()
{
    yield return null; // ждём один кадр, чтобы HP-бар успел обновиться
    Destroy(gameObject);
}
}
