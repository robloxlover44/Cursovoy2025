using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenEffectController : MonoBehaviour
{
    [Header("UI Components")]
    public Button actionButton;

    [Header("Post-Processing Volume")]
    public Volume postProcessingVolume;

    [Header("Transition Settings")]
    public float lensDistortionIncreaseSpeed = 1f; // Скорость увеличения Lens Distortion
    public float lensDistortionDecreaseSpeed = 1f; // Скорость уменьшения Lens Distortion
    public float fadeDuration = 1f;                // Длительность затемнения экрана
    public string nextSceneName;                   // Имя следующей сцены

    private LensDistortion lensDistortion;
    private CanvasGroup screenFadeCanvasGroup;

    void Start()
    {
        // Убедимся, что у нас есть компонент LensDistortion
        if (postProcessingVolume.profile.TryGet<LensDistortion>(out lensDistortion))
        {
            lensDistortion.intensity.overrideState = true;
            lensDistortion.intensity.value = 0f; // Начальное значение
        }
        else
        {
            Debug.LogError("LensDistortion effect not found in the Post-Processing Profile.");
        }

        // Привязываем действие к кнопке
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(() => StartCoroutine(TransitionEffect()));
        }

        // Инициализация затемнения экрана
        screenFadeCanvasGroup = new GameObject("ScreenFadeOverlay").AddComponent<CanvasGroup>();
        var canvas = screenFadeCanvasGroup.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        screenFadeCanvasGroup.alpha = 0;

        var image = screenFadeCanvasGroup.gameObject.AddComponent<Image>();
        image.color = Color.black;
    }

    private IEnumerator TransitionEffect()
    {
        float elapsedTime = 0f;

        // Увеличение Lens Distortion до 0.7
        while (lensDistortion.intensity.value < 0.7f)
        {
            lensDistortion.intensity.value += lensDistortionIncreaseSpeed * Time.deltaTime;
            yield return null;
        }

        // Параллельное уменьшение Lens Distortion и затемнение экрана
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Плавное уменьшение Lens Distortion
            float t = elapsedTime / fadeDuration;
            lensDistortion.intensity.value = Mathf.Lerp(0.7f, -1f, t);

            // Плавное затемнение экрана
            screenFadeCanvasGroup.alpha = Mathf.Clamp01(t);

            yield return null;
        }

        // Убедимся, что значения достигли финала
        lensDistortion.intensity.value = -1f;
        screenFadeCanvasGroup.alpha = 1f;

        // Переход к следующей сцене
        SceneManager.LoadScene(nextSceneName);
    }
}
