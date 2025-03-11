using System;
using UnityEngine;

[Serializable] // Необходим для сериализации в JSON
public class PlayerDataModel
{
    public int money;
    public int shards;

    // Конструктор с значениями по умолчанию
    public PlayerDataModel()
    {
        money = 0;
        shards = 0;
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