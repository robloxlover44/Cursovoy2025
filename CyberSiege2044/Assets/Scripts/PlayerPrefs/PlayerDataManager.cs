using UnityEngine;

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

    // Публичные методы для доступа к данным
    public int GetMoney() => playerData.money;
    public int GetShards() => playerData.shards;
    public int GetHealth() => playerData.health; // Добавляем геттер для здоровья

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

    public void AddHealth(int amount) // Добавляем метод для увеличения здоровья
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

    public bool SpendHealth(int amount) // Добавляем метод для уменьшения здоровья
    {
        bool success = playerData.SpendHealth(amount);
        if (success) SaveData();
        return success;
    }

    public void ResetData()
    {
        playerData = new PlayerDataModel();
        SaveData();
    }
}   