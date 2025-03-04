using UnityEngine;
using TMPro;
using System.Collections;

public class SequentialObjectActivatorWithReset : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToActivate; // Массив объектов для включения
    private int currentIndex = 0; // Индекс текущего объекта в массиве
    private const int chunkSize = 4; // Количество объектов в одной группе (например, 4)
    private bool isTyping = false; // Флаг, указывающий, печатается ли текст
    [SerializeField] private float typingSpeed = 0.05f; // Скорость печатания текста

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
            // Определяем, достигли ли конца текущего блока
            if (currentIndex % chunkSize == 0 && currentIndex > 0)
            {
                ResetPreviousChunk();
            }

            // Включаем текущий объект
            GameObject currentObject = objectsToActivate[currentIndex];
            currentObject.SetActive(true);

            // Начинаем эффект печатания текста
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
        // Вычисляем начало и конец предыдущего блока
        int startIndex = Mathf.Max(0, currentIndex - chunkSize);
        int endIndex = Mathf.Min(currentIndex, objectsToActivate.Length);

        // Выключаем объекты предыдущего блока
        for (int i = startIndex; i < endIndex; i++)
        {
            objectsToActivate[i].SetActive(false);
        }
    }

    private IEnumerator TypeText(TextMeshProUGUI textMeshPro, AudioSource audioSource)
    {
        isTyping = true; // Блокируем пробел

        string fullText = textMeshPro.text; // Полный текст
        textMeshPro.text = ""; // Очищаем текст перед началом печатания

        if (audioSource != null)
        {
            audioSource.Play(); // Запускаем звук
        }

        foreach (char c in fullText)
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(0.1f); // Небольшая задержка, чтобы завершить звук корректно

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop(); // Останавливаем звук после завершения печатания
        }

        isTyping = false; // Разблокируем пробел после завершения печатания
    }
}
