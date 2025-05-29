using UnityEngine;
using System.Collections;

public class BossEnemy : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public Transform ultiFirePoint;
    public GameObject bulletPrefab;
    public GameObject ultiBulletPrefab;  // <-- Новый: для ульты!

    

    [Header("Boss Settings")]
    public float maxHealth = 1000f;
    private float currentHealth;
    public float CurrentHealth { get { return currentHealth; } }

    [Header("Shooting")]
    public float fireRate = 0.5f;
    public float bulletSpeed = 7f;
    private float nextFireTime = 0f;

    [Header("Wave (Ult)")]
    public int waveBulletCount = 16;
    public float waveCooldown = 5f;

    [Header("Dash Attack")]
    public float dashCooldown = 4f;
    //public float dashDuration = 0.25f;
    public float dashWarningTime = 0.7f;
    public float dashSpeed = 15f; // Новое поле: скорость рывка

    public GameObject dashIndicatorPrefab;

    [Header("Sound FX")]
    public AudioClip shootSfx;
    public AudioClip ultSfx;
    public AudioClip dashSfx;
    public AudioSource audioSource;

    [Header("Other")]
    public Collider2D triggerZone;

    [Header("Animation Sprites")]
    public SpriteRenderer bossSpriteRenderer;
    public Sprite idleSprite;
    public Sprite[] shootSprites;
    public Sprite[] dashSprites;

    [Header("Death Animation")]
    public Sprite[] deathSprites;
    public float deathAnimDuration = 2f;
    public float shakeIntensity = 0.1f;
    public float blinkInterval = 0.08f;

    private Transform player;
    private bool playerInZone;
    private Rigidbody2D rb;
    private bool isDashing;
    private Coroutine dashAnimCoroutine;

    // Флаги для анимации, смерти, рывка
    private bool isShootingState = false;
    private float shootStateTimer = 0f;
    private bool dashRoutineRunning = false;
    private Vector2 lastPlayerPos;
    private bool isDying = false;

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

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.isKinematic = false;

        if (bossSpriteRenderer != null && idleSprite != null)
            bossSpriteRenderer.sprite = idleSprite;
    }

    void Update()
    {
        if (!isDashing && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }

        if (isDying) return;

        if (playerInZone && player != null && !isDashing)
        {
            Vector2 lookDir = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Vector2 shootDir = (player.position - firePoint.position).normalized;
            if (Time.time >= nextFireTime)
            {
                Shoot(shootDir);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }

        if (!isDashing)
        {
            if (shootStateTimer > 0f)
            {
                shootStateTimer -= Time.deltaTime;
                if (shootStateTimer <= 0f)
                {
                    isShootingState = !isShootingState;
                    shootStateTimer = 1f;
                }
            }

            if (isShootingState)
            {
                if (bossSpriteRenderer != null && shootSprites != null && shootSprites.Length > 0)
                    bossSpriteRenderer.sprite = shootSprites[0];
            }
            else
            {
                if (bossSpriteRenderer != null && idleSprite != null)
                    bossSpriteRenderer.sprite = idleSprite;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playerInZone)
        {
            playerInZone = true;
            StartCoroutine(WaveAttackRoutine());
            if (!dashRoutineRunning)
                StartCoroutine(DashRoutine());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (!isDashing && bossSpriteRenderer != null && idleSprite != null)
                bossSpriteRenderer.sprite = idleSprite;
        }
    }

    void Shoot(Vector2 dir)
    {
        isShootingState = true;
        shootStateTimer = 1f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        var proj = bullet.GetComponent<bullet>();
        if (proj != null)
            proj.SetDirection(dir);

        var rb2d = bullet.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.linearVelocity = dir * bulletSpeed;

        if (shootSfx != null && audioSource != null)
            audioSource.PlayOneShot(shootSfx, audioSource.volume);
    }

    IEnumerator PlayDashAnimation(float duration)
    {
        if (dashSprites == null || dashSprites.Length == 0 || bossSpriteRenderer == null)
            yield break;

        float frameTime = duration / dashSprites.Length;
        int i = 0;
        while (isDashing)
        {
            bossSpriteRenderer.sprite = dashSprites[i % dashSprites.Length];
            yield return new WaitForSeconds(frameTime);
            i++;
        }
        if (isShootingState && shootSprites != null && shootSprites.Length > 0)
            bossSpriteRenderer.sprite = shootSprites[0];
        else if (idleSprite != null)
            bossSpriteRenderer.sprite = idleSprite;
    }

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
        if (ultSfx != null && audioSource != null)
            audioSource.PlayOneShot(ultSfx, audioSource.volume);

        if (ultiFirePoint == null)
        {
            Debug.LogWarning("ultiFirePoint не назначен! Используется обычный firePoint для ульты.");
            ultiFirePoint = firePoint;
        }

        float step = 360f / waveBulletCount;
        for (int i = 0; i < waveBulletCount; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject bullet = Instantiate(ultiBulletPrefab != null ? ultiBulletPrefab : bulletPrefab, ultiFirePoint.position, Quaternion.Euler(0, 0, angle));
            var proj = bullet.GetComponent<bullet>();
            if (proj != null) proj.SetDirection(dir);
            var rb2d = bullet.GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.linearVelocity = dir * bulletSpeed;
        }
    }

    IEnumerator DashRoutine()
    {
        dashRoutineRunning = true;
        while (playerInZone && currentHealth > 0)
        {
            yield return new WaitForSeconds(dashCooldown);

            GameObject dashIndicator = null;
            DashIndicator indicatorScript = null;

            if (dashIndicatorPrefab)
            {
                dashIndicator = Instantiate(dashIndicatorPrefab);
                indicatorScript = dashIndicator.GetComponent<DashIndicator>();
                if (indicatorScript != null)
                    indicatorScript.Init(transform, player);
            }

            float timer = 0f;
            while (timer < dashWarningTime)
            {
                if (player != null)
                    lastPlayerPos = player.position;
                timer += Time.deltaTime;
                yield return null;
            }

            if (dashIndicator)
                Destroy(dashIndicator);

            if (dashSfx != null && audioSource != null)
                audioSource.PlayOneShot(dashSfx, audioSource.volume);

            yield return StartCoroutine(DoDash());
        }
        dashRoutineRunning = false;
    }

    IEnumerator DoDash()
{
    isDashing = true;

    if (dashAnimCoroutine != null)
        StopCoroutine(dashAnimCoroutine);

    Vector2 start = rb.position;
    Vector2 end = lastPlayerPos;

    Vector2 dashDir = (end - start).normalized;
    float dashLen = Vector2.Distance(start, end);

    Vector2 rayStart = start + dashDir * 0.01f;
    int wallMask = LayerMask.GetMask("Wall");
    RaycastHit2D hit = Physics2D.Raycast(rayStart, dashDir, dashLen, wallMask);

    Vector2 finalTarget = end;
    if (hit.collider != null)
        finalTarget = hit.point;

    float totalDistance = Vector2.Distance(start, finalTarget);
    float duration = totalDistance / dashSpeed;

    // Включаем dash-анимацию на всё время движения
    dashAnimCoroutine = StartCoroutine(PlayDashAnimation(duration));

    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0;
    rb.isKinematic = true;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        float t = elapsed / duration;
        rb.MovePosition(Vector2.Lerp(start, finalTarget, t));
        elapsed += Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();
    }
    rb.MovePosition(finalTarget);

    rb.isKinematic = false;
    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0;
    isDashing = false;
}


    public void TakeDamage(float dmg)
    {
        if (isDying) return;
        currentHealth -= dmg;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            StartCoroutine(DeathSequence());
        }
    }

    // Урон по игроку во время рывка (именно на коллизии!)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(50);
            }
        }
    }

    IEnumerator DeathSequence()
    {
        isDying = true;
        float timer = 0f;

        Vector3 originalPos = transform.position;
        Color originalColor = bossSpriteRenderer.color;

        // Фаза тряски и мигания
        while (timer < deathAnimDuration)
        {
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity);
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = originalPos + new Vector3(shakeX, shakeY, 0);

            if (Mathf.FloorToInt(timer / blinkInterval) % 2 == 0)
                bossSpriteRenderer.color = Color.red;
            else
                bossSpriteRenderer.color = originalColor;

            timer += Time.deltaTime;
            yield return null;
        }

        bossSpriteRenderer.color = originalColor;
        transform.position = originalPos;

        // Спрайтовая анимация смерти
        if (deathSprites != null && deathSprites.Length > 0)
        {
            float frameTime = 1f / deathSprites.Length;
            for (int i = 0; i < deathSprites.Length; i++)
            {
                bossSpriteRenderer.sprite = deathSprites[i];
                yield return new WaitForSeconds(frameTime);
            }
        }
        BossHealthBar2 bar = FindObjectOfType<BossHealthBar2>();
if (bar != null)
    bar.HideBarWithAnim();
    
    foreach (var indicator in FindObjectsOfType<DashIndicator>())
        {
            Destroy(indicator.gameObject);
        }



        Destroy(gameObject);
    }
}
