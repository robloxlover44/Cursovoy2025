using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeTrigger : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    public CinemachineCamera cineCamera;

    [Header("Длительность тряски (сек)")]
    public float shakeDuration = 2f;

    [Header("Сила тряски")]
    public float shakeAmplitude = 10f;
    public float shakeFrequency = 1f;

    [Header("Аудиоэффект")]
    public AudioClip shakeSfx;
    public AudioSource audioSource; // перетащи сюда аудиосорс

    private CinemachineBasicMultiChannelPerlin perlin;
    private float originalAmplitude;
    private float originalFrequency;

    void Start()
    {
        if (cineCamera == null)
        {
            Debug.LogError("Cinemachine Camera не назначена!");
            return;
        }
        // Ищем noise-компонент на камере
        perlin = cineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin == null)
        {
            Debug.LogError("На Cinemachine Camera нет Basic Multi Channel Perlin!");
            return;
        }
        originalAmplitude = perlin.AmplitudeGain;
        originalFrequency = perlin.FrequencyGain;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine());
        }
    }

    System.Collections.IEnumerator ShakeRoutine()
    {
        // Включаем тряску
        perlin.AmplitudeGain = shakeAmplitude;
        perlin.FrequencyGain = shakeFrequency;

        // Воспроизводим звук
        if (audioSource != null && shakeSfx != null)
            audioSource.PlayOneShot(shakeSfx);

        // Ждём заданное время
        yield return new WaitForSeconds(shakeDuration);

        // Возвращаем значения обратно
        perlin.AmplitudeGain = originalAmplitude;
        perlin.FrequencyGain = originalFrequency;
    }
}