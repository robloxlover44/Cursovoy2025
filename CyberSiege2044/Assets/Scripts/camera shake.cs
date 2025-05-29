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
    public AudioClip shakeSfx2;  // Второй звук
    public AudioSource audioSource; // перетащи сюда аудиосорс

    [Header("Ссылка на босса")]
    public BossEnemy boss;

    private CinemachineBasicMultiChannelPerlin perlin;
    private float originalAmplitude;
    private float originalFrequency;

    private bool isShaking = false;
    private Collider2D myCollider;

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

        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            Debug.LogError("На объекте нет Collider2D!");

        if (boss == null)
            boss = FindObjectOfType<BossEnemy>();
    }

    void Update()
    {
        // Если босс погиб — больше не играем второй звук
        if (boss == null || boss.CurrentHealth <= 0f)
        {
            shakeSfx2 = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isShaking)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && myCollider != null)
        {
            myCollider.enabled = false;
        }
    }

    System.Collections.IEnumerator ShakeRoutine()
    {
        isShaking = true;

        perlin.AmplitudeGain = shakeAmplitude;
        perlin.FrequencyGain = shakeFrequency;

        if (audioSource != null)
        {
            if (shakeSfx != null)
                audioSource.PlayOneShot(shakeSfx, audioSource.volume);
            if (shakeSfx2 != null)
                audioSource.PlayOneShot(shakeSfx2, audioSource.volume);
        }

        yield return new WaitForSeconds(shakeDuration);

        perlin.AmplitudeGain = originalAmplitude;
        perlin.FrequencyGain = originalFrequency;

        isShaking = false;
    }
}