using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar2: MonoBehaviour
{
    [Header("Ссылка на босса")]
    public BossEnemy boss; // Перетащи сюда босса или найди его в Start

    [Header("Слайдер HP бара")]
    public Slider healthSlider;

    void Start()
    {
        // Можно автоматически найти босса и слайдер, если не задано в инспекторе
        if (boss == null)
            boss = FindObjectOfType<BossEnemy>();
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (boss != null && healthSlider != null)
        {
            healthSlider.maxValue = boss.maxHealth;
            healthSlider.value = boss.maxHealth;
        }
    }

    void Update()
    {
        if (boss != null && healthSlider != null)
        {
            healthSlider.value = Mathf.Clamp(boss.CurrentHealth, 0, boss.maxHealth);
        }
    }
}
