using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    // Список слотов инвентаря (например, Image или Button компоненты)
    public List<Image> inventorySlots;

    void Start()
{
    // Явно обновляем инвентарь при старте
    RefreshInventory();
}


    // Метод для обновления инвентаря
    public void RefreshInventory()
    {
        List<string> weapons = PlayerDataManager.Instance.GetInventoryWeapons();
        
        // Пример: проходимся по слотам и заполняем их данными
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < weapons.Count)
            {
                // Здесь можно загружать спрайт по ID оружия и присваивать его слоту
                inventorySlots[i].sprite = GetWeaponSprite(weapons[i]);
                inventorySlots[i].enabled = true;
            }
            else
            {
                // Если оружия меньше, чем слотов, очищаем слот
                inventorySlots[i].sprite = null;
                inventorySlots[i].enabled = false;
            }
        }
    }

    // Метод для получения спрайта оружия по его ID
    private Sprite GetWeaponSprite(string weaponID)
{
    Sprite sprite = Resources.Load<Sprite>("Weapons/" + weaponID);
    if (sprite == null)
    {
        Debug.LogError($"Спрайт для {weaponID} не найден!");
        return Resources.Load<Sprite>("Weapons/44"); // Заглушка
    }
    return sprite;
}


}
