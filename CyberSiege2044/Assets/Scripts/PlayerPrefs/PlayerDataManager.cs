using UnityEngine;
using System.Collections.Generic;
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    private PlayerDataModel playerData;

    private const string DATA_KEY = "PlayerData";

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
    public int GetHealth() => playerData.health;

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
        playerData.AddHealth(amount);
        SaveData();
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
        bool success = playerData.SpendHealth(amount);
        if (success) SaveData();
        return success;
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
        SaveData();
    }
}