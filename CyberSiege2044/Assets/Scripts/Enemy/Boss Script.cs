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

    [Header("Shooting")]
    public float fireRate = 1f;         // ������� �������� (��� � �������� �����)
    public float bulletSpeed = 7f;
    private float nextFireTime = 0f;

    [Header("Wave (Ult)")]
    public int waveBulletCount = 16;
    public float waveCooldown = 5f;

    [Header("Dash Attack")]
    public float dashCooldown = 4f;
    public float dashSpeed = 14f;
    public float dashDuration = 0.25f;
    public float dashWarningTime = 0.7f;
    public GameObject dashIndicatorPrefab;

    [Header("Other")]
    public Collider2D triggerZone;

    private Transform player;
    private bool playerInZone;
    private Rigidbody2D rb;
    private bool isDashing;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();
        triggerZone.isTrigger = true;
    }

    void Update()
    {
        // ��� � �������� �����: ������ �������������� � ������, ���� �� � ����
        if (playerInZone && player != null && !isDashing)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // �������� ��� � �������� �����
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
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
            proj.SetDirection(dir);

        // ���� � ���� ���� Rigidbody2D � �� ������ ������ ����� ������ velocity
        var rb2d = bullet.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.linearVelocity = dir * bulletSpeed;
    }

    // --- ����� ���� (�����) ---
    IEnumerator WaveAttackRoutine()
    {
        yield return new WaitForSeconds(1f); // ��������� �������� ����� �����������
        while (playerInZone && currentHealth > 0)
        {
            yield return new WaitForSeconds(waveCooldown);
            FireBulletWave();
        }
    }
    void FireBulletWave()
    {
        float step = 360f / waveBulletCount;
        for (int i = 0; i < waveBulletCount; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
            var proj = bullet.GetComponent<Projectile>();
            if (proj != null) proj.SetDirection(dir);
            var rb2d = bullet.GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.linearVelocity = dir * bulletSpeed;
        }
    }

    // --- ����� ---
    IEnumerator DashRoutine()
    {
        while (playerInZone && currentHealth > 0)
        {
            yield return new WaitForSeconds(dashCooldown);

            GameObject dashIndicator = null;
            Vector2 dashDir = (player.position - transform.position).normalized;

            if (dashIndicatorPrefab)
            {
                dashIndicator = Instantiate(dashIndicatorPrefab);
                var indicatorScript = dashIndicator.GetComponent<DashIndicator>();
                if (indicatorScript != null)
                    indicatorScript.Init(transform, player);
            }

            yield return new WaitForSeconds(dashWarningTime);

            if (dashIndicator)
                Destroy(dashIndicator);

            yield return StartCoroutine(DoDash(dashDir));
        }
    }
    IEnumerator DoDash(Vector2 dashDir)
    {
        isDashing = true;
        float elapsed = 0f;
        Vector2 start = rb.position;
        Vector2 end = start + dashDir * dashSpeed * dashDuration;
        while (elapsed < dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(start, end, elapsed / dashDuration));
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isDashing = false;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= (int)dmg;
        if (currentHealth <= 0)
            Destroy(gameObject);
    }
}