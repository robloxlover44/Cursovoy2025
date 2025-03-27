using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject weaponPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && weaponPrefab != null)
            {
                string weaponID = weaponPrefab.GetComponent<Weapon>().weaponID;
                int newWeaponIndex = -1; // Объявляем переменную на уровне метода

                if (!PlayerDataManager.Instance.GetInventoryWeapons().Contains(weaponID))
                {
                    GameObject newWeaponObject = Instantiate(weaponPrefab, player.transform.position, Quaternion.identity);
                    newWeaponObject.transform.SetParent(player.transform);

                    SpriteRenderer[] weaponSprites = newWeaponObject.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer sprite in weaponSprites)
                    {
                        sprite.enabled = false;
                    }

                    player.inventoryWeaponObjects.Add(newWeaponObject);
                    newWeaponObject.SetActive(false);

                    PlayerDataManager.Instance.AddWeaponToInventory(weaponID);

                    newWeaponIndex = player.inventoryWeaponObjects.Count - 1;
                    player.SwitchWeapon(newWeaponIndex);

                    InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
                    if (inventoryUI != null)
                    {
                        inventoryUI.RefreshInventory();
                        inventoryUI.HighlightActiveWeapon(newWeaponIndex); // Теперь newWeaponIndex доступен
                    }
                }
                else
                {
                    Debug.Log("Оружие уже в инвентаре");
                }

                SpriteRenderer[] pickupSprites = GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sprite in pickupSprites)
                {
                    sprite.enabled = false;
                }

                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }

                Destroy(gameObject);
            }
        }
    }
}