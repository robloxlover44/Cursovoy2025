using UnityEngine;
using System.Collections;

public class ShooterEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Shooting }

    [Header("Trigger Settings")]
    public Collider2D triggerZone;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab; // Префаб пули
    public Transform firePoint; // Точка, откуда стреляет враг
    public float bulletSpeed = 5f; // Скорость пули
    public float fireRate = 1f; // Частота стрельбы (выстрелы в секунду)

    [Header("Health Settings")]
    public int maxHealth = 100; // Максимальное здоровье врага

    [Header("Animation Settings")]
    public Sprite[] idleAnimation; // Анимация ожидания
    public Sprite[] shootAnimation; // Анимация стрельбы
    public Sprite[] deathAnimation; // Анимация смерти
    public float frameRate = 0.15f; // Скорость анимации

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private EnemyState currentState = EnemyState.Idle;
    private Coroutine animationCoroutine;
    private int currentHealth;
    private float nextFireTime; // Время до следующего выстрела

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();

        triggerZone.isTrigger = true;
        currentHealth = maxHealth;
        StartAnimation(idleAnimation); // Запускаем анимацию ожидания
    }

    void Update()
    {
        if (currentState == EnemyState.Shooting && player != null)
        {
            ShootAtPlayer();
            // Поворачиваем врага лицом к игроку
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
            StartAnimation(shootAnimation); // Переключаем на анимацию стрельбы
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Shooting)
        {
            currentState = EnemyState.Idle;
            StartAnimation(idleAnimation); // Возвращаемся к анимации ожидания
        }
    }

    void ShootAtPlayer()
    {
        if (Time.time >= nextFireTime)
        {
            // Создаём пулю
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            PlayerDamagingProjectile projectile = bullet.GetComponent<PlayerDamagingProjectile>();
            if (projectile != null)
            {
                Vector2 direction = (player.position - firePoint.position).normalized;
                projectile.SetDirection(direction); // Задаём направление для пули
            }
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (player.position - firePoint.position).normalized;
                rb.linearVelocity = direction * bulletSpeed;
            }

            // Устанавливаем время следующего выстрела
            nextFireTime = Time.time + 1f / fireRate;
        }
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentState = EnemyState.Idle; // Останавливаем стрельбу
        StopAllCoroutines(); // Останавливаем текущие анимации
        GetComponent<Collider2D>().enabled = false; // Отключаем коллайдер
        StartCoroutine(PlayDeathAnimation()); // Запускаем анимацию смерти
    }

    IEnumerator PlayDeathAnimation()
    {
        foreach (Sprite frame in deathAnimation)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }
        Destroy(gameObject); // Уничтожаем врага после анимации
    }
}