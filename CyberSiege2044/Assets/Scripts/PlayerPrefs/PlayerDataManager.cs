using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    private PlayerDataModel playerData;

    private const string DATA_KEY = "PlayerData"; // Ключ для сохранения

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Сохраняем между сценами
        LoadData(); // Загружаем данные при старте
    }

    // Инициализация или загрузка данных
    private void LoadData()
    {
        if (PlayerPrefs.HasKey(DATA_KEY))
        {
            string json = PlayerPrefs.GetString(DATA_KEY);
            playerData = JsonUtility.FromJson<PlayerDataModel>(json);
        }
        else
        {
            playerData = new PlayerDataModel(); // Новые данные, если нет сохранений
            SaveData();
        }
    }

    // Сохранение данных
    private void SaveData()
    {
        string json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(DATA_KEY, json);
        PlayerPrefs.Save();
    }

    // Публичные методы для доступа к данным
    public int GetMoney() => playerData.money;
    public int GetShards() => playerData.shards;

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

    // Для сброса данных (опционально)
    public void ResetData()
    {
        playerData = new PlayerDataModel();
        SaveData();
    }
}