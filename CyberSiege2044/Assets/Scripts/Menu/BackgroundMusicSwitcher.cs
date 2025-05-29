using UnityEngine;

public class BackgroundMusicSwitcher : MonoBehaviour
{
    [Header("Настройки аудио")]
    [Tooltip("Аудио источник для фоновой музыки")]
    public AudioSource audioSource;

    [Tooltip("Основной аудиоклип")]
    public AudioClip primaryClip;

    [Tooltip("Альтернативный аудиоклип (включается при активации объекта)")]
    public AudioClip secondaryClip;

    [Header("Объект для отслеживания")]
    [Tooltip("Объект, активация которого переключает музыку")]
    public GameObject targetObject;

    private bool isUsingSecondaryClip = false;
    private float currentPlayTime = 0f;

    void Update()
    {
        if (targetObject == null || audioSource == null || primaryClip == null || secondaryClip == null)
        {
            Debug.LogError("Не все ссылки настроены в инспекторе!");
            return;
        }

        // Проверяем состояние объекта и переключаем музыку
        if (targetObject.activeSelf && !isUsingSecondaryClip)
        {
            SwitchToClip(secondaryClip);
            isUsingSecondaryClip = true;
        }
        else if (!targetObject.activeSelf && isUsingSecondaryClip)
        {
            SwitchToClip(primaryClip);
            isUsingSecondaryClip = false;
        }
    }

    private void SwitchToClip(AudioClip clip)
    {
        // Сохраняем текущее время воспроизведения
        currentPlayTime = audioSource.time;

        // Переключаем аудиоклип
        audioSource.clip = clip;

        // Устанавливаем позицию воспроизведения
        audioSource.time = currentPlayTime;

        // Проигрываем новый клип
        audioSource.Play();
    }
}
