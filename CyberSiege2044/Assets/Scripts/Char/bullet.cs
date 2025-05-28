using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed = 10f; // Скорость пули
    public float damage = 0.1f; // Урон, наносимый пулей, настраивается в инспекторе
    private Vector2 direction;

    // Метод для задания направления движения пули
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized; // Нормализуем вектор направления
    }

    void Update()
    {
        // Движение пули в заданном направлении
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // --- Обычные враги (ChaseEnemy, TurretController) ---
        if (other.CompareTag("Enemy") && other is BoxCollider2D)
        {
            // ChaseEnemy
            ChaseEnemy chaseEnemy = other.GetComponent<ChaseEnemy>();
            if (chaseEnemy != null)
            {
                chaseEnemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            // ShooterEnemy
            TurretController turret = other.GetComponent<TurretController>();
            if (turret != null)
            {
                turret.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            // --- БОСС ---
            BossEnemy boss = other.GetComponent<BossEnemy>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Уничтожаем при столкновении со стеной или препятствием
        }
    }
}
