using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHealthBar2 : MonoBehaviour
{
    [Header("Ссылка на босса")]
    public BossEnemy boss;

    [Header("Слайдер HP бара")]
    public Slider healthSlider;

    [Header("BG и Fill")]
    public RectTransform backgroundRect;
    public RectTransform fillRect;

    private TextMeshProUGUI hpText;
    private bool appeared = false;
    private string originalText = "";

    void Start()
    {
        if (boss == null)
            boss = FindObjectOfType<BossEnemy>();
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();
        if (backgroundRect == null)
            backgroundRect = transform.Find("Background").GetComponent<RectTransform>();
        if (fillRect == null)
            fillRect = transform.Find("Fill Area/Fill").GetComponent<RectTransform>();
        hpText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (boss != null && healthSlider != null)
        {
            healthSlider.maxValue = boss.maxHealth;
            healthSlider.value = boss.maxHealth;
        }

        // PIVOT обязательно слева!
        if (backgroundRect) backgroundRect.pivot = new Vector2(0, 0.5f);
        if (fillRect) fillRect.pivot = new Vector2(0, 0.5f);

        if (hpText)
        {
            originalText = hpText.text;
            hpText.text = "";
        }

        if (backgroundRect) backgroundRect.localScale = new Vector3(0, 1, 1);
        if (fillRect) fillRect.localScale = new Vector3(0, 1, 1);
        appeared = false;
    }

    void Update()
    {
        if (boss == null || healthSlider == null) return;

        if (!appeared)
        {
            appeared = true;
            StartCoroutine(ShowBar());
        }

        float hp = Mathf.Clamp(boss.CurrentHealth, 0, boss.maxHealth);
        healthSlider.value = hp;
    }

    IEnumerator ShowBar()
    {
        // 1. Бэкграунд появляется слева
        if (backgroundRect)
        {
            backgroundRect.localScale = new Vector3(0, 1, 1);
            LeanTween.scaleX(backgroundRect.gameObject, 1, 0.4f).setEaseOutQuad();
            yield return new WaitForSeconds(1.0f); // Пауза перед fill
        }

        // 2. Fill появляется слева
        if (fillRect)
        {
            fillRect.localScale = new Vector3(0, 1, 1);
            LeanTween.scaleX(fillRect.gameObject, 1, 0.25f).setEaseOutBack();
            yield return new WaitForSeconds(0.25f);
        }

        // 3. Анимировать появление текста по символу (оригинальный текст)
        if (hpText && !string.IsNullOrEmpty(originalText))
        {
            hpText.text = "";
            for (int i = 0; i < originalText.Length; i++)
            {
                hpText.text += originalText[i];
                yield return new WaitForSeconds(0.04f);
            }
        }
    }

    // --- Анимация исчезновения хп-бара ---
    public void HideBarWithAnim()
    {
        StartCoroutine(HideBarCoroutine());
    }

    IEnumerator HideBarCoroutine()
    {
        // 1. Fill исчезает слева
        if (fillRect)
        {
            LeanTween.scaleX(fillRect.gameObject, 0, 0.25f).setEaseInBack();
            yield return new WaitForSeconds(0.25f);
        }

        // 2. Через 0.4 сек после fill уходит BG
        if (backgroundRect)
        {
            LeanTween.scaleX(backgroundRect.gameObject, 0, 0.4f).setEaseInQuad();
            yield return new WaitForSeconds(0.4f);
        }

        // 3. Анимация исчезновения текста по символу (с конца к началу)
        if (hpText && !string.IsNullOrEmpty(hpText.text))
        {
            string text = hpText.text;
            for (int i = text.Length; i > 0; i--)
            {
                hpText.text = text.Substring(0, i - 1);
                yield return new WaitForSeconds(0.04f);
            }
        }

        // 4. Отключить объект
        gameObject.SetActive(false);
    }
}
