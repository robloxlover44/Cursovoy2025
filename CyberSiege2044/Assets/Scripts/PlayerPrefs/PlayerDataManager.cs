using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Список уровней по порядку")]
    public List<string> levelSceneNames = new List<string>();

    [Header("Scenes to save checkpoint on")]
    public List<string> checkpointScenes = new List<string>();

    private PlayerDataModel playerData;
    private int health;
    private PlayerDataModel checkpointData;
    private const string DATA_KEY = "PlayerData";
    private int portalEntryCount = 0; // Счётчик заходов в портал

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
        health = playerData.maxHealth;
        LoadCheckpointState();
        OnHealthChanged?.Invoke();

        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("PlayerDataManager инициализирован");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (checkpointScenes.Contains(scene.name))
        {
            SaveCheckpointState();
            Debug.Log("Чекпоинт сохранён на сцене: " + scene.name);
        }
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey(DATA_KEY))
        {
            string json = PlayerPrefs.GetString(DATA_KEY);
            playerData = JsonUtility.FromJson<PlayerDataModel>(json);
            portalEntryCount = playerData.portalEntryCount; // Загружаем счётчик
            Debug.Log($"Загружен portalEntryCount: {portalEntryCount}");
        }
        else
        {
            playerData = new PlayerDataModel();
            SaveData();
        }
    }

    private void SaveData()
    {
        playerData.portalEntryCount = portalEntryCount; // Сохраняем счётчик
        string json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(DATA_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"Сохранён portalEntryCount: {portalEntryCount}");
    }

    public void SaveCheckpointState()
    {
        checkpointData = new PlayerDataModel
        {
            money = playerData.money,
            shards = playerData.shards,
            maxHealth = playerData.maxHealth,
            inventoryWeapons = new List<string>(playerData.inventoryWeapons),
            currentWeaponIndex = playerData.currentWeaponIndex,
            portalEntryCount = portalEntryCount
        };
        checkpointData.SetHealth(health);
    }

    public void LoadCheckpointState()
    {
        if (checkpointData != null)
        {
            Debug.Log("Загружаем данные чекпоинта:");
            Debug.Log($"HP: {checkpointData.GetHealth()}, Money: {checkpointData.money}, Shards: {checkpointData.shards}, Weapons: {string.Join(",", checkpointData.inventoryWeapons)}, PortalEntryCount: {checkpointData.portalEntryCount}");

            health = checkpointData.GetHealth();
            playerData.money = checkpointData.money;
            playerData.shards = checkpointData.shards;
            playerData.maxHealth = checkpointData.maxHealth;
            playerData.inventoryWeapons = new List<string>(checkpointData.inventoryWeapons);
            playerData.currentWeaponIndex = checkpointData.currentWeaponIndex;
            portalEntryCount = checkpointData.portalEntryCount;
            SaveData();
            OnHealthChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("Чекпоинт не найден, загрузка невозможна!");
        }
    }

    public void LoadNextLevel()
    {
        portalEntryCount++; // Увеличиваем счётчик заходов
        Debug.Log($"portalEntryCount увеличен до: {portalEntryCount}");

        int sceneIndex = portalEntryCount - 1; // Индекс сцены (первый заход = индекс 0)
        if (sceneIndex >= 0 && sceneIndex < levelSceneNames.Count)
        {
            string nextScene = levelSceneNames[sceneIndex];
            Debug.Log($"Загружаем сцену: {nextScene} (индекс: {sceneIndex})");
            SceneManager.LoadScene(nextScene);
            SaveData(); // Сохраняем счётчик
        }
        else
        {
            Debug.LogWarning($"Сцена с индексом {sceneIndex} не найдена! Количество сцен: {levelSceneNames.Count}");
        }
    }

    public int GetPortalEntryCount() => portalEntryCount;

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
        OnHealthChanged?.Invoke();
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
            OnHealthChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void UpgradeMaxHealth(int newMaxHealth)
    {
        playerData.maxHealth = newMaxHealth;
        health = playerData.maxHealth;
        SaveData();
        OnHealthChanged?.Invoke();
    }

    public void RefreshHealth()
    {
        health = playerData.maxHealth;
        OnHealthChanged?.Invoke();
    }

    public void AddWeaponToInventory(string weaponID)
    {
        if (string.IsNullOrEmpty(weaponID)) return;
        if (!playerData.inventoryWeapons.Contains(weaponID))
        {
            playerData.inventoryWeapons.Add(weaponID);
            Debug.Log($"Added {weaponID} to inventory");
            SaveData();
        }
    }

    public List<string> GetInventoryWeapons() => playerData.inventoryWeapons;

    public void SetCurrentWeaponIndex(int index)
    {
        playerData.currentWeaponIndex = index;
        SaveData();
    }

    public int GetCurrentWeaponIndex() => playerData.currentWeaponIndex;

    public void ResetData()
    {
        playerData = new PlayerDataModel();
        health = playerData.maxHealth;
        portalEntryCount = 0; // Сбрасываем счётчик
        SaveData();
        OnHealthChanged?.Invoke();
    }
}

// Расширение модели под здоровье чекпоинта
public static class PlayerDataModelExtensions
{
    private static int checkpointHealth;
    public static void SetHealth(this PlayerDataModel data, int hp) => checkpointHealth = hp;
    public static int GetHealth(this PlayerDataModel data) => checkpointHealth;
}