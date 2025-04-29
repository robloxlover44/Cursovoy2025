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

    public PlayerDataModel()
    {
        money = 50;
        shards = 0;
        maxHealth = 100; // Начальное значение максимального здоровья
        currentWeaponIndex = -1; // -1 означает, что оружие не выбрано
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
}
