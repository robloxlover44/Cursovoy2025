using UnityEngine;
using TMPro;
using System.Collections;

public class SequentialObjectActivatorWithReset : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToActivate; // ������ �������� ��� ���������
    private int currentIndex = 0; // ������ �������� ������� � �������
    private const int chunkSize = 4; // ���������� �������� � ����� ������ (��������, 4)
    private bool isTyping = false; // ����, �����������, ���������� �� �����
    [SerializeField] private float typingSpeed = 0.05f; // �������� ��������� ������

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTyping)
        {
            ActivateNextObject();
        }
    }

    private void ActivateNextObject()
    {
        if (currentIndex < objectsToActivate.Length)
        {
            // ����������, �������� �� ����� �������� �����
            if (currentIndex % chunkSize == 0 && currentIndex > 0)
            {
                ResetPreviousChunk();
            }

            // �������� ������� ������
            GameObject currentObject = objectsToActivate[currentIndex];
            currentObject.SetActive(true);

            // �������� ������ ��������� ������
            TextMeshProUGUI textMeshPro = currentObject.GetComponentInChildren<TextMeshProUGUI>();
            AudioSource audioSource = currentObject.GetComponentInChildren<AudioSource>();
            if (textMeshPro != null)
            {
                StartCoroutine(TypeText(textMeshPro, audioSource));
            }

            currentIndex++;
        }
    }

    private void ResetPreviousChunk()
    {
        // ��������� ������ � ����� ����������� �����
        int startIndex = Mathf.Max(0, currentIndex - chunkSize);
        int endIndex = Mathf.Min(currentIndex, objectsToActivate.Length);

        // ��������� ������� ����������� �����
        for (int i = startIndex; i < endIndex; i++)
        {
            objectsToActivate[i].SetActive(false);
        }
    }

    private IEnumerator TypeText(TextMeshProUGUI textMeshPro, AudioSource audioSource)
    {
        isTyping = true; // ��������� ������

        string fullText = textMeshPro.text; // ������ �����
        textMeshPro.text = ""; // ������� ����� ����� ������� ���������

        if (audioSource != null)
        {
            audioSource.Play(); // ��������� ����
        }

        foreach (char c in fullText)
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(0.1f); // ��������� ��������, ����� ��������� ���� ���������

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop(); // ������������� ���� ����� ���������� ���������
        }

        isTyping = false; // ������������ ������ ����� ���������� ���������
    }
}
