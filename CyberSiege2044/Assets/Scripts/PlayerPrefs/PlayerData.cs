using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class PlayerDataModel
{
    public int money;
    public int shards;
    public int maxHealth; // Максимальное здоровье, которое сохраняется
    public List<string> inventoryWeapons = new List<string>();
    public int currentWeaponIndex;
    public int portalEntryCount; // Счётчик заходов в портал

    public PlayerDataModel()
    {
        money = 60;
        shards = 0;
        maxHealth = 100; // Начальное значение максимального здоровья
        currentWeaponIndex = -1; // -1 означает, что оружие не выбрано
        portalEntryCount = 0; // Начинаем с 0
    }

    // Методы для работы с валютами
    public void AddMoney(int amount) => money += amount;
    public void AddShards(int amount) => shards += amount;

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }
        return false;
    }

    public bool SpendShards(int amount)
    {
        if (shards >= amount)
        {
            shards -= amount;
            return true;
        }
        return false;
    }

    public PlayerDataModel Clone()
    {
        // Глубокая копия через сериализацию в JSON и обратно
        string json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<PlayerDataModel>(json);
    }
}