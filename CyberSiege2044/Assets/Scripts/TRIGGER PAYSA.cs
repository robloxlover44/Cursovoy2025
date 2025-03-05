using UnityEngine;
using TMPro;
using System.Collections;
using static LeanTween;

public class PauseOnTrigger : MonoBehaviour
{
    [Header("Настройки диалога")]
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private CanvasGroup dialogCanvasGroup; // Добавлен CanvasGroup для альфа-анимации
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI secondaryText;
    [SerializeField] private string dialogMessage = "Где это я...?";
    [SerializeField] private string secondaryMessage = "Нажмите любую клавишу...";
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private KeyCode[] continueKeys;
    
    [Header("Настройки звука")]
    [SerializeField] private AudioSource audioSource;

    private bool isPaused = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Collider2D triggerCollider;
    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;

    private void Start()
    {
        if (dialogBox != null)
        {
            hiddenPosition = dialogBox.transform.position + new Vector3(-500, 0, 0); // Начальная позиция за экраном
            visiblePosition = dialogBox.transform.position; // Позиция на экране
            dialogBox.transform.position = hiddenPosition;
            dialogCanvasGroup.alpha = 0f;
        }

        if (dialogText != null) dialogText.text = "";
        if (secondaryText != null) secondaryText.text = "";

        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider == null)
        {
            Debug.LogError("Отсутствует компонент Collider2D!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPaused)
        {
            Debug.Log("Игрок вошел в зону триггера.");
            PauseGame();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок вышел из зоны триггера.");
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
                Debug.Log("Триггер отключен.");
            }
        }
    }

    private void Update()
    {
        if (isPaused && !isTyping && AnyContinueKeyPressed())
        {
            Debug.Log("Игрок нажал кнопку для продолжения.");
            ResumeGame();
        }
    }

    private bool AnyContinueKeyPressed()
    {
        foreach (KeyCode key in continueKeys)
        {
            if (Input.GetKeyDown(key)) return true;
        }
        return false;
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (dialogBox != null)
        {
            dialogBox.SetActive(true);
            LeanTween.move(dialogBox, visiblePosition, 0.5f).setEase(LeanTweenType.easeOutExpo).setIgnoreTimeScale(true);
            LeanTween.alphaCanvas(dialogCanvasGroup, 1f, 0.5f).setIgnoreTimeScale(true);
        }

        if (dialogText != null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(dialogMessage));
        }

        if (audioSource != null) audioSource.Play();
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (dialogBox != null)
        {
            LeanTween.move(dialogBox, hiddenPosition, 0.5f).setEase(LeanTweenType.easeInExpo).setIgnoreTimeScale(true);
            LeanTween.alphaCanvas(dialogCanvasGroup, 0f, 0.5f).setIgnoreTimeScale(true).setOnComplete(() => dialogBox.SetActive(false));
        }

        if (dialogText != null) dialogText.text = "";
        if (secondaryText != null) secondaryText.text = "";
        if (audioSource != null) audioSource.Stop();
    }

    private IEnumerator TypeText(string message)
    {
        isTyping = true;
        dialogText.text = "";
        int index = 0;
        string cursor = "<color=#FFFFFF>|</color>";

        while (index <= message.Length)
        {
            dialogText.text = message.Substring(0, index) + cursor;
            index++;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        dialogText.text = message;
        isTyping = false;

        if (secondaryText != null) secondaryText.text = secondaryMessage;
        if (audioSource != null) audioSource.Stop();

        StartCoroutine(BlinkCursor());
    }

    private IEnumerator BlinkCursor()
    {
        string baseText = dialogText.text;
        while (isPaused)
        {
            dialogText.text = baseText + "<color=#FFFFFF>|</color>";
            yield return new WaitForSecondsRealtime(0.5f);
            dialogText.text = baseText;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}
