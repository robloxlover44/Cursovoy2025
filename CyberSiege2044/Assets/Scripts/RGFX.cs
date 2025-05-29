using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Картинки для отключения во время глитча")]
    public Image imageToDisable1;
    public Image imageToDisable2;

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

            // Отключаем изображения
            if (imageToDisable1 != null)
                imageToDisable1.gameObject.SetActive(false);
            if (imageToDisable2 != null)
                imageToDisable2.gameObject.SetActive(false);

            // Держим эффект включенным в течение glitchDuration
            yield return new WaitForSeconds(glitchDuration);

            // Откатываем обратно
            globalVolume.SetActive(false);

            if (imageToDisable1 != null)
                imageToDisable1.gameObject.SetActive(true);
            if (imageToDisable2 != null)
                imageToDisable2.gameObject.SetActive(true);
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
