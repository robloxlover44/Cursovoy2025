using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class PlayerDataModel
{
    public int money;
    public int shards;
    public int health; // Добавляем здоровье
    public List<string> inventoryWeapons = new List<string>();

    public PlayerDataModel()
    {
        money = 0;
        shards = 0;
        health = 100; // Задаем начальное значение здоровья, например 100
    }

    // Методы для работы с валютами и здоровьем
    public void AddMoney(int amount) => money += amount;
    public void AddShards(int amount) => shards += amount;
    public void AddHealth(int amount) => health += amount; // Добавляем метод для прибавления здоровья

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

    public bool SpendHealth(int amount) // Добавляем метод для траты здоровья
    {
        if (health >= amount)
        {
            health -= amount;
            return true;
        }
        return false;
    }
}