using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Скорость пули
    public int damage = 50; // Урон, наносимый пулей, настраивается в инспекторе**
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
        if (other.CompareTag("Enemy") && other is EdgeCollider2D) // Убираем проверку на EdgeCollider2D для универсальности**
        {
            ChaseEnemy enemy = other.GetComponent<ChaseEnemy>(); // Получаем компонент врага**
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Наносим урон врагу**
            }
            Destroy(gameObject); // Уничтожаем пулю после попадания
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}