using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private List<ShopItemSlot> shopItemSlots = new List<ShopItemSlot>(); // Список слотов магазина в сцене

    // Описание предмета магазина
    [System.Serializable]
    public class ShopItem
    {
        public string weaponID; // ID оружия (например, "Pistol", "Rifle")
        public int cost; // Стоимость в деньгах
        public Sprite icon; // Иконка оружия
    }
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>(); // Список доступных предметов

    void Start()
    {
        // Настраиваем слоты магазина
        SetupShop();
    }

    private void SetupShop()
    {
        // Проходим по слотам и настраиваем их
        for (int i = 0; i < shopItemSlots.Count; i++)
        {
            if (i < shopItems.Count)
            {
                bool isOwned = PlayerDataManager.Instance.GetInventoryWeapons().Contains(shopItems[i].weaponID);
                // Настраиваем слот с данными предмета
                shopItemSlots[i].Setup(shopItems[i].weaponID, shopItems[i].icon, shopItems[i].cost, isOwned);
                // Добавляем обработчик нажатия
                int index = i; // Сохраняем индекс для замыкания
                shopItemSlots[i].button.onClick.RemoveAllListeners();
                if (!isOwned)
                {
                    shopItemSlots[i].button.onClick.AddListener(() => TryBuyWeapon(shopItems[index].weaponID, shopItems[index].cost, shopItemSlots[index]));
                }
            }
            else
            {
                // Отключаем пустые слоты
                shopItemSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void TryBuyWeapon(string weaponID, int cost, ShopItemSlot slot)
    {
        // Эффект нажатия: уменьшение и возврат масштаба
        LeanTween.scale(slot.gameObject, Vector3.one * 0.9f, 0.05f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
            LeanTween.scale(slot.gameObject, Vector3.one, 0.05f).setEase(LeanTweenType.easeOutQuad);
        });

        // Проверяем, хватает ли денег
        if (PlayerDataManager.Instance.SpendMoney(cost))
        {
            // Проверяем, нет ли уже этого оружия в инвентаре
            if (!PlayerDataManager.Instance.GetInventoryWeapons().Contains(weaponID))
            {
                // Добавляем оружие в инвентарь
                PlayerDataManager.Instance.AddWeaponToInventory(weaponID);
                Debug.Log($"Куплено оружие: {weaponID}");

                // Анимация покупки
                slot.MarkAsOwned();
            }
            else
            {
                Debug.Log("Оружие уже есть в инвентаре!");
            }
        }
        else
        {
            Debug.Log("Недостаточно денег!");
            slot.FlashRed(shopItems[shopItemSlots.IndexOf(slot)].weaponID);
        }
    }
}

// Компонент для слота магазина
[System.Serializable]
public class ShopItemSlot
{
    public GameObject gameObject; // Ссылка на объект слота
    public Button button; // Кнопка слота
    public Image icon; // Иконка оружия
    public TextMeshProUGUI costText; // Текст стоимости
    public TextMeshProUGUI weaponNameText; // Текст названия оружия

    public void Setup(string weaponID, Sprite iconSprite, int cost, bool isOwned)
    {
        icon.sprite = iconSprite;
        if (isOwned)
        {
            MarkAsOwned();
        }
        else
        {
            costText.text = cost.ToString();
            weaponNameText.text = weaponID;
            // Сбрасываем альфу для иконки и фона кнопки
            SetImageAlpha(icon, 1f);
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null) SetImageAlpha(buttonImage, 1f);
            gameObject.SetActive(true);
        }
    }

    public void MarkAsOwned()
    {
        // Затемнение иконки и фона кнопки на 80% (альфа = 0.2)
        LeanTween.value(icon.gameObject, icon.color.a, 0.4f, 0.4f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float alpha) => SetImageAlpha(icon, alpha));
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            LeanTween.value(buttonImage.gameObject, buttonImage.color.a, 0.2f, 0.3f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnUpdate((float alpha) => SetImageAlpha(buttonImage, alpha));
        }

        // Исчезновение текста стоимости
        costText.gameObject.SetActive(false);

        // Смена текста на "Owned" и центрирование
        weaponNameText.text = "Owned";
        weaponNameText.fontSize *= 2;
        RectTransform nameRect = weaponNameText.GetComponent<RectTransform>();
        LeanTween.move(nameRect, new Vector3(0, 5, 0), 0.3f).setEase(LeanTweenType.easeOutQuad);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    public void FlashRed(string originalText)
    {
    // Сохраняем исходные цвета
    Color originalIconColor = icon.color;
    Color originalButtonColor = button.GetComponent<Image>()?.color ?? Color.white;
    Color originalCostTextColor = costText.color;
    Color originalNameTextColor = weaponNameText.color;

    // Меняем текст на "Not Enough Money"
    weaponNameText.text = "Error\\..";

    // Мигание красным для иконки
    LeanTween.value(icon.gameObject, 0f, 1f, 0.2f)
        .setEase(LeanTweenType.easeInOutQuad)
        .setLoopPingPong(1)
        .setOnUpdate((float t) =>
        {
            icon.color = Color.Lerp(originalIconColor, new Color(1f, 0f, 0f, originalIconColor.a), t);
        });

    // Мигание красным для фона кнопки (если есть)
    Image buttonImage = button.GetComponent<Image>();
    if (buttonImage != null)
    {
        LeanTween.value(buttonImage.gameObject, 0f, 1f, 0.2f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1)
            .setOnUpdate((float t) =>
            {
                buttonImage.color = Color.Lerp(originalButtonColor, new Color(1f, 0f, 0f, originalButtonColor.a), t);
            });
    }

    // Мигание красным для текстов
    LeanTween.value(costText.gameObject, 0f, 1f, 0.2f)
        .setEase(LeanTweenType.easeInOutQuad)
        .setLoopPingPong(1)
        .setOnUpdate((float t) =>
        {
            costText.color = Color.Lerp(originalCostTextColor, Color.red, t);
            weaponNameText.color = Color.Lerp(originalNameTextColor, Color.red, t);
        })
        .setOnComplete(() =>
        {
            // Восстанавливаем текст
            weaponNameText.text = originalText;
        });
}
}