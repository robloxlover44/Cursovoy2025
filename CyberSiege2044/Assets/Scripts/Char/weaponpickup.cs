using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject weaponPrefab; // Префаб оружия, которое получит игрок

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && weaponPrefab != null)
            {
                // Если у игрока уже есть оружие, можно его удалить перед заменой
                if (player.currentWeapon != null)
                {
                    Destroy(player.currentWeapon.gameObject);
                }

                // Создаём оружие у игрока
                GameObject newWeapon = Instantiate(weaponPrefab, player.transform.position, Quaternion.identity);
                newWeapon.transform.SetParent(player.transform); // Делаем его дочерним объектом игрока

                // Передаём это оружие в PlayerController
                player.currentWeapon = newWeapon.GetComponent<Weapon>();

                Debug.Log("Оружие подобрано!");

                // Удаляем объект пикапа
                Destroy(gameObject);
            }
        }
    }
}
