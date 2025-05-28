using UnityEngine;
using UnityEngine.UI;

public class BossHPBarActivator : MonoBehaviour
{
    [Header("Ссылка на объект HP-бара (Panel или CanvasGroup)")]
    public GameObject hpBarObject;
    [Header("CanvasGroup для fade-in (по желанию)")]
    public CanvasGroup hpBarCanvasGroup; // добавь CanvasGroup на панель хп
    [Header("Слайдер HP-бара")]
    public RectTransform hpBarRect;      // сам RectTransform слайдера
    public float showDuration = 0.5f;    // длительность анимации

    private Vector2 initialSize;
    private Vector2 fullSize;

    void Start()
    {
        if (hpBarObject != null)
            hpBarObject.SetActive(false);

        if (hpBarRect != null)
        {
            fullSize = hpBarRect.sizeDelta;
            initialSize = new Vector2(0, fullSize.y);
            hpBarRect.sizeDelta = initialSize;
        }

        if (hpBarCanvasGroup != null)
            hpBarCanvasGroup.alpha = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && hpBarObject != null)
        {
            hpBarObject.SetActive(true);

            if (hpBarRect != null)
            {
                // Бар стартует с шириной 0 и расширяется из центра
                hpBarRect.pivot = new Vector2(0.5f, hpBarRect.pivot.y);
                hpBarRect.sizeDelta = initialSize;
                LeanTween.size(hpBarRect, fullSize, showDuration).setEase(LeanTweenType.easeOutCubic);
            }

            if (hpBarCanvasGroup != null)
            {
                hpBarCanvasGroup.alpha = 0f;
                LeanTween.alphaCanvas(hpBarCanvasGroup, 1f, showDuration * 0.9f).setEase(LeanTweenType.easeInCubic);
            }
        }
    }
}