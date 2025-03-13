using UnityEngine;
using UnityEngine.UI; // Для работы с UI

public class ResourceButton : MonoBehaviour
{
    public enum ResourceType { Money, Shards }
    public ResourceType resourceType;
    public int amount = 1; // Количество ресурсов
    private bool hasBeenClicked = false; // Флаг, чтобы нажималось только один раз

    private void Start()
    {
        Button button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("[ResourceButton] Ошибка: Скрипт висит не на кнопке!");
            return;
        }

        button.onClick.AddListener(GiveResource);
    }

    private void GiveResource()
    {
        if (hasBeenClicked) return; // Если уже нажимали – выходим

        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("[ResourceButton] Ошибка: PlayerDataManager.Instance == null!");
            return;
        }

        switch (resourceType)
        {
            case ResourceType.Money:
                PlayerDataManager.Instance.AddMoney(amount);
                break;
            case ResourceType.Shards:
                PlayerDataManager.Instance.AddShards(amount);
                break;
        }

        Debug.Log($"[ResourceButton] {amount} {resourceType} добавлено!");
        hasBeenClicked = true; // Больше нельзя нажать
    }
}