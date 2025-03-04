using System.Collections;
using UnityEngine;

public class RGFX : MonoBehaviour
{
    [Header("Настройки времени")]
    [Tooltip("Минимальное время между включением эффекта")]
    public float minInterval = 5f;

    [Tooltip("Максимальное время между включением эффекта")]
    public float maxInterval = 15f;

    [Tooltip("Продолжительность эффекта")]
    public float glitchDuration = 3f;

    [Header("Объект с эффектом")]
    [Tooltip("Ссылка на Global Volume объект")]
    public GameObject globalVolume;

    private Coroutine glitchCoroutine;

    void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Не назначен объект Global Volume!");
            return;
        }

        // Убедитесь, что эффект отключен в начале
        globalVolume.SetActive(false);

        // Запускаем цикл случайного включения эффекта
        glitchCoroutine = StartCoroutine(GlitchCycle());
    }

    private IEnumerator GlitchCycle()
    {
        while (true)
        {
            // Ждем случайное время перед включением эффекта
            float randomInterval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(randomInterval);

            // Включаем эффект
            globalVolume.SetActive(true);

            // Держим эффект включенным в течение glitchDuration
            yield return new WaitForSeconds(glitchDuration);

            // Выключаем эффект
            globalVolume.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }
    }
}
