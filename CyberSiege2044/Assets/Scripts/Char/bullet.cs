using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Скорость пули
    public int damage = 50; // Урон, наносимый пулей, настраивается в инспекторе
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
        if (other.CompareTag("Enemy") && other is BoxCollider2D)
        {
            // Проверяем ChaseEnemy
            ChaseEnemy chaseEnemy = other.GetComponent<ChaseEnemy>();
            if (chaseEnemy != null)
            {
                chaseEnemy.TakeDamage(damage); // Наносим урон ChaseEnemy
                Destroy(gameObject); // Уничтожаем пулю после попадания
                return; // Выходим, чтобы не проверять дальше
            }

            // Проверяем ShooterEnemy
            TurretController turret = other.GetComponent<TurretController>();
            if (turret != null)
            {
                turret.TakeDamage(damage);
                Destroy(gameObject);
            }

        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Уничтожаем при столкновении со стеной или препятствием
        }
    }
}