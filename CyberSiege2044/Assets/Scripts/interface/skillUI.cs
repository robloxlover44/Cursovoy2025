using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SkillType { Aim, Shield }

public class SkillIconUI : MonoBehaviour
{
    [Header("Skill Identification")]
    [Tooltip("ID пассивного навыка, совпадающий с PlayerDataModel.unlockedSkills")] 
    public string skillID;
    [Tooltip("Тип скилла для обработки кулдауна")] 
    public SkillType skillType;
    [Tooltip("Ссылка на контроллер щита (для SkillType.Shield)")]
    public ShieldController shieldController;

    [Header("UI Components")]
    public Image frameImage;          // рамка вокруг иконки
    public Image iconImage;           // сама иконка навыка
    public Image overlayImage;        // тёмный оверлей для кулдауна
    public TextMeshProUGUI statusText; // текст Act//, Deact// или CD//

    [Header("Appearance")]
    [Tooltip("Цвет рамки при активном скилле")] public Color activeFrameColor = Color.green;
    [Tooltip("Цвет рамки при неактивном скилле")] public Color inactiveFrameColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [Tooltip("Цвет иконки при активном скилле")] public Color activeIconColor = Color.white;
    [Tooltip("Цвет иконки при неактивном скилле")] public Color inactiveIconColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [Tooltip("Цвет оверлея кулдауна (сверху вниз)")] public Color cooldownOverlayColor = new Color(0f, 0f, 0f, 0.7f);

    private void Start()
    {
        // Настройка оверлея для кулдауна (заполнение сверху вниз)
        if (overlayImage != null)
        {
            overlayImage.type = Image.Type.Filled;
            overlayImage.fillMethod = Image.FillMethod.Vertical;
            overlayImage.fillOrigin = (int)Image.OriginVertical.Top;
            overlayImage.color = cooldownOverlayColor;
        }
    }

    private void Update()
    {
        UpdateIconState();
    }

    private void UpdateIconState()
    {
        bool unlocked = PlayerDataManager.Instance.IsSkillUnlocked(skillID);
        if (!unlocked)
        {
            SetInactive();
            return;
        }

        if (skillType == SkillType.Shield)
        {
            // ShieldController должен быть присвоен в инспекторе
            if (shieldController == null)
            {
                Debug.LogError($"[SkillIconUI] ShieldController not assigned for skillID={skillID}");
                SetActive();
                return;
            }

            float remaining = shieldController.CooldownRemaining;
            float duration  = shieldController.CooldownDuration;

            if (remaining > 0f)
                SetCooldown(remaining, duration);
            else
                SetActive();
        }
        else // Aim или другие пассивки без кулдауна
        {
            SetActive();
        }
    }

    private void SetInactive()
    {
        frameImage.color  = inactiveFrameColor;
        iconImage.color   = inactiveIconColor;
        if (overlayImage != null)
            overlayImage.enabled = false;
        statusText.text   = "Deact//";
    }

    private void SetActive()
    {
        frameImage.color  = activeFrameColor;
        iconImage.color   = activeIconColor;
        if (overlayImage != null)
            overlayImage.enabled = false;
        statusText.text   = "Act//";
    }

    private void SetCooldown(float remaining, float duration)
    {
        frameImage.color  = inactiveFrameColor;
        iconImage.color   = activeIconColor;
        if (overlayImage != null)
        {
            overlayImage.enabled    = true;
            overlayImage.fillAmount = remaining / duration;
        }
        statusText.text   = $"CD//{Mathf.CeilToInt(remaining)}";
    }
}
