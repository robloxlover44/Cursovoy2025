using System.Collections;
using UnityEngine;

public class RGFX : MonoBehaviour
{
    [Header("��������� �������")]
    [Tooltip("����������� ����� ����� ���������� �������")]
    public float minInterval = 5f;

    [Tooltip("������������ ����� ����� ���������� �������")]
    public float maxInterval = 15f;

    [Tooltip("����������������� �������")]
    public float glitchDuration = 3f;

    [Header("������ � ��������")]
    [Tooltip("������ �� Global Volume ������")]
    public GameObject globalVolume;

    private Coroutine glitchCoroutine;

    void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("�� �������� ������ Global Volume!");
            return;
        }

        // ���������, ��� ������ �������� � ������
        globalVolume.SetActive(false);

        // ��������� ���� ���������� ��������� �������
        glitchCoroutine = StartCoroutine(GlitchCycle());
    }

    private IEnumerator GlitchCycle()
    {
        while (true)
        {
            // ���� ��������� ����� ����� ���������� �������
            float randomInterval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(randomInterval);

            // �������� ������
            globalVolume.SetActive(true);

            // ������ ������ ���������� � ������� glitchDuration
            yield return new WaitForSeconds(glitchDuration);

            // ��������� ������
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
