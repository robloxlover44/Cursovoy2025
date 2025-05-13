using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Weapon Shop Slots")]
    [SerializeField]
    private List<ShopItemSlot> shopItemSlots = new List<ShopItemSlot>();

    [Header("Weapon Items")]
    [SerializeField]
    private List<ShopItem> shopItems = new List<ShopItem>();

    [System.Serializable]
    public class ShopItem
    {
        public string itemID;
        public int cost;
        public Sprite icon;
    }

    [Header("Skill Shop Slots")]
    [SerializeField]
    private List<ShopItemSlot> skillItemSlots = new List<ShopItemSlot>();

    [Header("Skill Items")]
    [SerializeField]
    private List<SkillItem> skillItems = new List<SkillItem>();

    [System.Serializable]
    public class SkillItem
    {
        public string skillID;
        public int cost;
        public Sprite icon;
    }

    private void Start()
    {
        Debug.Log("[ShopUI] Start() called");
        Debug.Log($"[ShopUI] Weapon slots: {shopItemSlots.Count}, Weapon items: {shopItems.Count}");
        Debug.Log($"[ShopUI] Skill slots: {skillItemSlots.Count}, Skill items: {skillItems.Count}");

        SetupWeaponShop();
        SetupSkillShop();
    }

    private void SetupWeaponShop()
    {
        Debug.Log("[ShopUI] SetupWeaponShop() start");
        for (int i = 0; i < shopItemSlots.Count; i++)
        {
            Debug.Log($"[ShopUI] Weapon slot index {i}");
            if (i < shopItems.Count)
            {
                var item = shopItems[i];
                bool isOwned = PlayerDataManager.Instance.GetInventoryWeapons().Contains(item.itemID);
                var slot = shopItemSlots[i];

                Debug.Log($"[ShopUI] Weapon itemID={item.itemID}, cost={item.cost}, isOwned={isOwned}");
                slot.Setup(item.itemID, item.icon, item.cost, isOwned);

                slot.button.interactable = !isOwned;
                Debug.Log($"[ShopUI] Weapon slot.button.interactable set to {!isOwned}");

                Debug.Log("[ShopUI] Weapon slot.button.RemoveAllListeners()");
                slot.button.onClick.RemoveAllListeners();
                if (!isOwned)
                {
                    int index = i;
                    Debug.Log("[ShopUI] Adding Weapon listener");
                    slot.button.onClick.AddListener(() =>
                    {
                        Debug.Log($"[ShopUI] Weapon buy clicked: {shopItems[index].itemID}");
                        TryBuyWeapon(shopItems[index].itemID, shopItems[index].cost, shopItemSlots[index]);
                    });
                }
            }
            else
            {
                Debug.Log($"[ShopUI] Weapon slot {i} has no corresponding item, deactivating slot");
                shopItemSlots[i].slotroot.SetActive(false);
            }
        }
        Debug.Log("[ShopUI] SetupWeaponShop() end");
    }

    private void TryBuyWeapon(string weaponID, int cost, ShopItemSlot slot)
    {
        Debug.Log($"[ShopUI] TryBuyWeapon called for {weaponID} with cost {cost}");
        LeanTween.scale(slot.slotroot, Vector3.one * 0.9f, 0.05f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => LeanTween.scale(slot.slotroot, Vector3.one, 0.05f).setEase(LeanTweenType.easeOutQuad));

        if (PlayerDataManager.Instance.SpendMoney(cost))
        {
            if (!PlayerDataManager.Instance.GetInventoryWeapons().Contains(weaponID))
            {
                PlayerDataManager.Instance.AddWeaponToInventory(weaponID);
                Debug.Log($"[ShopUI] Weapon purchased: {weaponID}");
                slot.MarkAsOwned();
                slot.button.interactable = false;
            }
            else
            {
                Debug.Log("[ShopUI] Weapon already owned!");
            }
        }
        else
        {
            Debug.Log("[ShopUI] Not enough money for weapon!");
            slot.FlashRed(weaponID);
        }
    }

    private void SetupSkillShop()
    {
        Debug.Log("[ShopUI] SetupSkillShop() start");
        for (int i = 0; i < skillItemSlots.Count; i++)
        {
            Debug.Log($"[ShopUI] Skill slot index {i}");
            if (i < skillItems.Count)
            {
                var item = skillItems[i];
                bool isUnlocked = PlayerDataManager.Instance.IsSkillUnlocked(item.skillID);
                var slot = skillItemSlots[i];

                Debug.Log($"[ShopUI] Skill skillID={item.skillID}, cost={item.cost}, isUnlocked={isUnlocked}");
                slot.Setup(item.skillID, item.icon, item.cost, isUnlocked);

                slot.button.interactable = !isUnlocked;
                Debug.Log($"[ShopUI] Skill slot.button.interactable set to {!isUnlocked}");

                Debug.Log("[ShopUI] Skill slot.button.RemoveAllListeners()");
                slot.button.onClick.RemoveAllListeners();
                if (!isUnlocked)
                {
                    int index = i;
                    Debug.Log("[ShopUI] Adding Skill listener");
                    slot.button.onClick.AddListener(() =>
                    {
                        Debug.Log($"[ShopUI] Skill buy clicked: {skillItems[index].skillID}");
                        TryBuySkill(skillItems[index].skillID, skillItems[index].cost, skillItemSlots[index]);
                    });
                }
            }
            else
            {
                Debug.Log($"[ShopUI] Skill slot {i} has no corresponding item, deactivating slot");
                skillItemSlots[i].slotroot.SetActive(false);
            }
        }
        Debug.Log("[ShopUI] SetupSkillShop() end");
    }

    private void TryBuySkill(string skillID, int cost, ShopItemSlot slot)
    {
        Debug.Log($"[ShopUI] TryBuySkill called for {skillID} with cost {cost}");
        LeanTween.scale(slot.slotroot, Vector3.one * 0.9f, 0.05f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => LeanTween.scale(slot.slotroot, Vector3.one, 0.05f).setEase(LeanTweenType.easeOutQuad));

        if (PlayerDataManager.Instance.SpendMoney(cost))
        {
            if (!PlayerDataManager.Instance.IsSkillUnlocked(skillID))
            {
                PlayerDataManager.Instance.UnlockSkill(skillID);
                Debug.Log($"[ShopUI] Skill unlocked: {skillID}");
                slot.MarkAsOwned();
                slot.button.interactable = false;
            }
            else
            {
                Debug.Log("[ShopUI] Skill already unlocked!");
            }
        }
        else
        {
            Debug.Log("[ShopUI] Not enough money for skill!");
            slot.FlashRed(skillID);
        }
    }
}

[System.Serializable]
public class ShopItemSlot
{
    public GameObject slotroot;
    public Button button;
    public Image icon;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI weaponNameText;

    public void Setup(string itemID, Sprite iconSprite, int cost, bool isOwned)
    {
        Debug.Log($"[ShopItemSlot] Setup called for {itemID}, isOwned={isOwned}");
        icon.sprite = iconSprite;
        if (isOwned)
        {
            MarkAsOwned();
        }
        else
        {
            slotroot.SetActive(true);
            button.interactable = true;
            costText.text = cost.ToString();
            weaponNameText.text = itemID;
            SetImageAlpha(icon, 1f);
            var btnImage = button.GetComponent<Image>();
            if (btnImage != null) SetImageAlpha(btnImage, 1f);
        }
    }

    public void MarkAsOwned()
    {
        Debug.Log("[ShopItemSlot] MarkAsOwned called");
        LeanTween.value(icon.gameObject, icon.color.a, 0.4f, 0.4f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float alpha) => SetImageAlpha(icon, alpha));

        var btnImage = button.GetComponent<Image>();
        if (btnImage != null)
        {
            LeanTween.value(btnImage.gameObject, btnImage.color.a, 0.2f, 0.3f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnUpdate((float alpha) => SetImageAlpha(btnImage, alpha));
        }

        costText.gameObject.SetActive(false);
        weaponNameText.text = "Owned";
        weaponNameText.fontSize *= 2;
        LeanTween.move(weaponNameText.GetComponent<RectTransform>(), new Vector3(0, 5, 0), 0.3f).setEase(LeanTweenType.easeOutQuad);
    }

    private void SetImageAlpha(Graphic graphic, float alpha)
    {
        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    public void FlashRed(string originalText)
    {
        Debug.Log($"[ShopItemSlot] FlashRed called for {originalText}");
        var originalIconColor = icon.color;
        var btnImage = button.GetComponent<Image>();
        var originalBtnColor = btnImage != null ? btnImage.color : Color.white;
        var originalCostTextColor = costText.color;
        var originalNameTextColor = weaponNameText.color;

        weaponNameText.text = "Error..";

        LeanTween.value(icon.gameObject, 0f, 1f, 0.2f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1)
            .setOnUpdate((float t) => icon.color = Color.Lerp(originalIconColor, Color.red, t));

        if (btnImage != null)
        {
            LeanTween.value(btnImage.gameObject, 0f, 1f, 0.2f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setLoopPingPong(1)
                .setOnUpdate((float t) => btnImage.color = Color.Lerp(originalBtnColor, Color.red, t));
        }

        LeanTween.value(costText.gameObject, 0f, 1f, 0.2f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1)
            .setOnUpdate((float t) =>
            {
                costText.color = Color.Lerp(originalCostTextColor, Color.red, t);
                weaponNameText.color = Color.Lerp(originalNameTextColor, Color.red, t);
            })
            .setOnComplete(() => weaponNameText.text = originalText);
    }
}
