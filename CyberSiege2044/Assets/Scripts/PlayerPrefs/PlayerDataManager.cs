using UnityEngine;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    private PlayerDataModel playerData;
    private int health; // Текущее здоровье, не сохраняется

    private const string DATA_KEY = "PlayerData";

    // Событие, которое будет вызываться при изменении здоровья
    public event System.Action OnHealthChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
        health = playerData.maxHealth; // Устанавливаем текущее здоровье равным максимальному
        OnHealthChanged?.Invoke(); // Уведомляем подписчиков о начальном состоянии
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey(DATA_KEY))
        {
            string json = PlayerPrefs.GetString(DATA_KEY);
            playerData = JsonUtility.FromJson<PlayerDataModel>(json);
        }
        else
        {
            playerData = new PlayerDataModel();
            SaveData();
        }
    }

    private void SaveData()
    {
        string json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(DATA_KEY, json);
        PlayerPrefs.Save();
    }

    public int GetMoney() => playerData.money;
    public int GetShards() => playerData.shards;
    public int GetHealth() => health;
    public int GetMaxHealth() => playerData.maxHealth;

    public void AddMoney(int amount)
    {
        playerData.AddMoney(amount);
        SaveData();
    }

    public void AddShards(int amount)
    {
        playerData.AddShards(amount);
        SaveData();
    }

    public void AddHealth(int amount)
    {
        health += amount;
        if (health > playerData.maxHealth) health = playerData.maxHealth;
        OnHealthChanged?.Invoke(); // Уведомляем о изменении здоровья
    }

    public bool SpendMoney(int amount)
    {
        bool success = playerData.SpendMoney(amount);
        if (success) SaveData();
        return success;
    }

    public bool SpendShards(int amount)
    {
        bool success = playerData.SpendShards(amount);
        if (success) SaveData();
        return success;
    }

    public bool SpendHealth(int amount)
    {
        if (health >= amount)
        {
            health -= amount;
            OnHealthChanged?.Invoke(); // Уведомляем о изменении здоровья
            return true;
        }
        return false;
    }

    public void UpgradeMaxHealth(int newMaxHealth)
    {
        playerData.maxHealth = newMaxHealth;
        health = playerData.maxHealth;
        SaveData();
        OnHealthChanged?.Invoke(); // Уведомляем о изменении здоровья
    }
    public void RefreshHealth()
        {
            health = playerData.maxHealth; // Устанавливаем здоровье на максимум
            OnHealthChanged?.Invoke(); // Уведомляем подписчиков
        }
    public void AddWeaponToInventory(string weaponID)
    {
        if (string.IsNullOrEmpty(weaponID))
        {
            Debug.LogError("WeaponID is null or empty!");
            return;
        }
        playerData.inventoryWeapons.Add(weaponID);
        Debug.Log($"Added {weaponID} to inventory");
        SaveData();
    }

    public List<string> GetInventoryWeapons()
    {
        return playerData.inventoryWeapons;
    }

    public void SetCurrentWeaponIndex(int index)
    {
        playerData.currentWeaponIndex = index;
        SaveData();
    }

    public int GetCurrentWeaponIndex()
    {
        return playerData.currentWeaponIndex;
    }

    public void ResetData()
    {
        playerData = new PlayerDataModel();
        health = playerData.maxHealth;
        SaveData();
        OnHealthChanged?.Invoke(); // Уведомляем о сбросе
    }
}