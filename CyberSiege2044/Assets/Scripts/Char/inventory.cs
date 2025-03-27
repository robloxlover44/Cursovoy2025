using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public List<Image> inventorySlots;
    private List<Vector3> originalPositions = new List<Vector3>(); // Храним исходные позиции слотов
    private List<Vector3> originalScales = new List<Vector3>(); // Храним исходные масштабы слотов

    void Start()
    {
        // Сохраняем исходные позиции и масштабы слотов
        foreach (Image slot in inventorySlots)
        {
            if (slot != null)
            {
                originalPositions.Add(slot.transform.parent.position);
                originalScales.Add(slot.transform.parent.localScale);
            }
        }

        RefreshInventory();
    }

    public void RefreshInventory()
    {
        List<string> weapons = PlayerDataManager.Instance.GetInventoryWeapons();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < weapons.Count)
            {
                inventorySlots[i].sprite = GetWeaponSprite(weapons[i]);
                inventorySlots[i].enabled = true;
            }
            else
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].enabled = false;
            }
        }

        // Подсвечиваем активное оружие
        HighlightActiveWeapon(PlayerDataManager.Instance.GetCurrentWeaponIndex());
    }

    private Sprite GetWeaponSprite(string weaponID)
    {
        Sprite sprite = Resources.Load<Sprite>("Weapons/" + weaponID);
        if (sprite == null)
        {
            Debug.LogError($"Спрайт для {weaponID} не найден!");
            return Resources.Load<Sprite>("Weapons/44");
        }
        return sprite;
    }

    public void HighlightActiveWeapon(int activeIndex)
    {
        // Сбрасываем все слоты к исходному состоянию
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].transform.parent != null)
            {
                Transform slotParent = inventorySlots[i].transform.parent;
                LeanTween.cancel(slotParent.gameObject); // Отменяем предыдущие анимации

                // Возвращаем исходные позицию и масштаб
                LeanTween.move(slotParent.gameObject, originalPositions[i], 0.3f).setEase(LeanTweenType.easeOutQuad);
                LeanTween.scale(slotParent.gameObject, originalScales[i], 0.3f).setEase(LeanTweenType.easeOutQuad);
            }
        }

        // Подсвечиваем активный слот
        if (activeIndex >= 0 && activeIndex < inventorySlots.Count && inventorySlots[activeIndex] != null)
        {
            Transform activeSlotParent = inventorySlots[activeIndex].transform.parent;
            if (activeSlotParent != null)
            {
                // Увеличиваем масштаб (например, до 1.2)
                LeanTween.scale(activeSlotParent.gameObject, originalScales[activeIndex] * 1.2f, 0.3f).setEase(LeanTweenType.easeOutQuad);

                // Сдвигаем вправо на 10 пикселей
                Vector3 targetPosition = originalPositions[activeIndex] + new Vector3(1f, 0, 0);
                LeanTween.move(activeSlotParent.gameObject, targetPosition, 0.3f).setEase(LeanTweenType.easeOutQuad);
            }
        }
    }
}