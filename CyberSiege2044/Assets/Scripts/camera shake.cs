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
    public AudioClip shakeSfx2;  // Музыка для боссфайта
    public AudioSource audioSource; // Обычный sfx

    [Header("Ссылка на босса")]
    public BossEnemy boss;

    private CinemachineBasicMultiChannelPerlin perlin;
    private float originalAmplitude;
    private float originalFrequency;

    private bool isShaking = false;
    private Collider2D myCollider;

    // Специальный источник для музыки босса
    private AudioSource bossMusicSource;

    void Start()
    {
        if (cineCamera == null)
        {
            Debug.LogError("Cinemachine Camera не назначена!");
            return;
        }
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

        // Создаём отдельный источник для музыки босса
        if (bossMusicSource == null)
        {
            GameObject musicObj = new GameObject("BossMusicSource");
            bossMusicSource = musicObj.AddComponent<AudioSource>();
            bossMusicSource.transform.SetParent(this.transform);
            bossMusicSource.playOnAwake = false;
            bossMusicSource.loop = false; // true если музыка на повторе
            bossMusicSource.volume = audioSource != null ? audioSource.volume : 1f;
            musicObj.hideFlags = HideFlags.HideInHierarchy; // Чтобы не мешал в иерархии
        }
    }

    void Update()
    {
        // Если босс погиб — стоп музыка для босса мгновенно
        if ((boss == null || boss.CurrentHealth <= 0f) && bossMusicSource != null && bossMusicSource.isPlaying)
        {
            bossMusicSource.Stop();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isShaking && boss != null && boss.CurrentHealth > 0f)
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

        if (audioSource != null && shakeSfx != null)
            audioSource.PlayOneShot(shakeSfx, audioSource.volume);

        if (shakeSfx2 != null && bossMusicSource != null)
        {
            bossMusicSource.clip = shakeSfx2;
            bossMusicSource.volume = audioSource != null ? audioSource.volume : 1f;
            bossMusicSource.Play();
        }

        yield return new WaitForSeconds(shakeDuration);

        perlin.AmplitudeGain = originalAmplitude;
        perlin.FrequencyGain = originalFrequency;

        isShaking = false;
    }
}