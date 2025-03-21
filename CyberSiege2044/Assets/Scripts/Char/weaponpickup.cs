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
            // Отключаем рендерер и коллайдер пикапа
            GetComponentInChildren<SpriteRenderer>().enabled = false;
            GetComponentInChildren<Collider2D>().enabled = false;

            // Отключаем все дочерние объекты (например, частицы)
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            // Создаём оружие и передаём управление игроку
            GameObject newWeapon = Instantiate(weaponPrefab, player.transform.position, Quaternion.identity);
            newWeapon.transform.SetParent(player.transform);
            player.currentWeapon = newWeapon.GetComponent<Weapon>();

            // Добавляем в инвентарь
            Weapon weaponData = weaponPrefab.GetComponent<Weapon>();
            if (weaponData != null)
            {
                PlayerDataManager.Instance.AddWeaponToInventory(weaponData.weaponID);
            }

            // Обновляем UI
            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.RefreshInventory();
            }

            // Уничтожаем пикап через 0.1 секунды (для гарантии)
            Destroy(gameObject, 0.1f);
        }
    }
}

}
