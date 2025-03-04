using UnityEngine;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.5f; // ����� ���������� � ����������

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // ����� �� ����������� ��� ����� ����
        fadePanel.alpha = 0; // �������� � ������������
    }

    public IEnumerator FadeOut()
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 1;
    }

    public IEnumerator FadeIn()
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 0;
    }
}