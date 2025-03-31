using UnityEngine;

public class PlayerDamagingProjectile : MonoBehaviour
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
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>(); // Получаем компонент игрока
            if (player != null)
            {
                player.TakeDamage(damage); // Наносим урон игроку
            }
            Destroy(gameObject); // Уничтожаем пулю после попадания
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Уничтожаем при столкновении со стеной или препятствием
        }
    }
}