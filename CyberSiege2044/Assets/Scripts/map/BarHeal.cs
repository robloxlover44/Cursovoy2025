using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal; // Для Light2D
using System.Collections;

public class BarHeal : MonoBehaviour
{
    [Header("Привязка World Text")]
    public TextMeshProUGUI worldText; // TMP в World Canvas

    [Header("Текст")]
    public string healMessage = "HP restored!";
    public string hintMessage = "Press E to Heal";
    public float restoredTime = 1.5f;

    [Header("Внешний вид")]
    public Sprite usedSprite; // Сюда кидаешь нужный спрайт после использования

    private bool canHeal = false;
    private bool used = false;

    void Start()
    {
        if (worldText != null)
            worldText.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!used && other.CompareTag("Player"))
        {
            canHeal = true;
            if (worldText != null)
            {
                worldText.gameObject.SetActive(true);
                worldText.text = hintMessage;
                worldText.color = Color.white;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canHeal = false;
            if (worldText != null)
                worldText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!used && canHeal && Input.GetKeyDown(KeyCode.E))
        {
            used = true;
            PlayerDataManager.Instance.RefreshHealth();

            // --- СМЕНА СПРАЙТА ---
            var sr = GetComponent<SpriteRenderer>();
            if (sr && usedSprite) sr.sprite = usedSprite;

            // --- СМЕНА ЦВЕТА СВЕТА ---
            var light = GetComponentInChildren<Light2D>();
            if (light) light.color = Color.green;

            // --- ВЫВОД ТЕКСТА И ДЕАКТИВАЦИЯ ---
            if (worldText != null)
            {
                StopAllCoroutines();
                worldText.text = healMessage;
                StartCoroutine(RainbowAndFade(worldText, restoredTime));
            }

            // Блокируем хилку
            canHeal = false;
            var coll = GetComponent<Collider2D>();
            if (coll) coll.enabled = false;
        }
    }

    IEnumerator RainbowAndFade(TextMeshProUGUI txt, float duration)
    {
        float t = 0;
        txt.alpha = 1f;
        while (t < duration)
        {
            txt.color = Color.HSVToRGB((Time.time * 1.2f) % 1f, 0.85f, 1f);
            txt.alpha = Mathf.Lerp(1f, 0f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        txt.gameObject.SetActive(false);
    }
}
