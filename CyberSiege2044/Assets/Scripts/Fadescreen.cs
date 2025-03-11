using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ScreenFader инициализирован и сохранён между сценами.");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Дубликат ScreenFader уничтожен.");
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (fadeImage == null)
        {
            Debug.LogError("fadeImage не задан в инспекторе!");
        }
        else
        {
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void FadeAndLoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Название сцены пустое!");
            return;
        }
        StartCoroutine(FadeAndLoadSceneCoroutine(sceneName));
    }

    private IEnumerator FadeAndLoadSceneCoroutine(string sceneName)
    {
        yield return StartCoroutine(FadeIn());
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        fadeImage.gameObject.SetActive(true);
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}