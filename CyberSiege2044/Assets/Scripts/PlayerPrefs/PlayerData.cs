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

    // Новое поле для пассивных навыков
    public List<string> unlockedSkills = new List<string>();

    public PlayerDataModel()
    {
        money = 110;
        shards = 0;
        maxHealth = 100;
        currentWeaponIndex = -1;
        portalEntryCount = 0;
        unlockedSkills = new List<string>();
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

    // Методы для пассивных навыков
    public void UnlockSkill(string skillID)
    {
        if (!unlockedSkills.Contains(skillID))
            unlockedSkills.Add(skillID);
    }

    public bool HasSkill(string skillID)
    {
        return unlockedSkills.Contains(skillID);
    }

    public PlayerDataModel Clone()
    {
        string json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<PlayerDataModel>(json);
    }
}