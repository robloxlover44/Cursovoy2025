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
    public float lensDistortionIncreaseSpeed = 1f; // �������� ���������� Lens Distortion
    public float lensDistortionDecreaseSpeed = 1f; // �������� ���������� Lens Distortion
    public float fadeDuration = 1f;                // ������������ ���������� ������
    public string nextSceneName;                   // ��� ��������� �����

    private LensDistortion lensDistortion;
    private CanvasGroup screenFadeCanvasGroup;

    void Start()
    {
        // ��������, ��� � ��� ���� ��������� LensDistortion
        if (postProcessingVolume.profile.TryGet<LensDistortion>(out lensDistortion))
        {
            lensDistortion.intensity.overrideState = true;
            lensDistortion.intensity.value = 0f; // ��������� ��������
        }
        else
        {
            Debug.LogError("LensDistortion effect not found in the Post-Processing Profile.");
        }

        // ����������� �������� � ������
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(() => StartCoroutine(TransitionEffect()));
        }

        // ������������� ���������� ������
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

        // ���������� Lens Distortion �� 0.7
        while (lensDistortion.intensity.value < 0.7f)
        {
            lensDistortion.intensity.value += lensDistortionIncreaseSpeed * Time.deltaTime;
            yield return null;
        }

        // ������������ ���������� Lens Distortion � ���������� ������
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // ������� ���������� Lens Distortion
            float t = elapsedTime / fadeDuration;
            lensDistortion.intensity.value = Mathf.Lerp(0.7f, -1f, t);

            // ������� ���������� ������
            screenFadeCanvasGroup.alpha = Mathf.Clamp01(t);

            yield return null;
        }

        // ��������, ��� �������� �������� ������
        lensDistortion.intensity.value = -1f;
        screenFadeCanvasGroup.alpha = 1f;

        // ������� � ��������� �����
        SceneManager.LoadScene(nextSceneName);
    }
}
