using UnityEngine;
using TMPro;
using System.Collections;
using static LeanTween;

public class DialogueSystemFP : MonoBehaviour
{
    [Header("Настройки диалога")]
    [SerializeField] private GameObject dialogBox1;           // Первое диалоговое окно
    [SerializeField] private GameObject dialogBox2;           // Второе диалоговое окно
    [SerializeField] private CanvasGroup dialogCanvasGroup1;  // CanvasGroup для анимации первого окна
    [SerializeField] private CanvasGroup dialogCanvasGroup2;  // CanvasGroup для анимации второго окна
    [SerializeField] private TextMeshProUGUI dialogText1;     // Текст первого окна
    [SerializeField] private TextMeshProUGUI dialogText2;     // Текст второго окна
    [SerializeField] private Transform speakerIcon1;          // Иконка первого говорящего
    [SerializeField] private Transform speakerIcon2;          // Иконка второго говорящего
    [SerializeField] private float typingSpeed = 0.05f;       // Скорость набора текста
    [SerializeField] private KeyCode nextKey = KeyCode.Space; // Клавиша для перехода к следующей реплике

    [Header("Данные диалога")]
    [SerializeField] private string[] dialogue;               // Массив реплик
    [SerializeField] private bool[] isSpeaker1;               // Кто говорит: true — первый, false — второй

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;         // Источник звука для диалога

    [Header("Задержка перед диалогом")]
    [SerializeField] private float delayBeforeDialogue = 0.5f; // Задержка перед стартом (настраивается в инспекторе)

    private int currentLine = 0;         // Текущая реплика
    private bool isPaused = false;       // Флаг паузы игры
    private bool isTyping = false;       // Флаг набора текста
    private Coroutine typingCoroutine;   // Корутина для набора текста

    private void Start()
    {
        // Изначально скрываем диалоговые окна
        dialogBox1.SetActive(false);
        dialogBox2.SetActive(false);

        // Запускаем диалог с задержкой
        StartCoroutine(StartDialogueAfterDelay());
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        // Ждем указанное время
        yield return new WaitForSeconds(delayBeforeDialogue);

        // Запускаем диалог
        StartDialogue();
    }

    private void Update()
    {
        // Переход к следующей реплике по нажатию клавиши
        if (isPaused && Input.GetKeyDown(nextKey))
        {
            ShowNextLine();
        }
    }

    private void StartDialogue()
    {
        isPaused = true;
        Time.timeScale = 0f; // Останавливаем время в игре
        currentLine = 0;

        // Показываем окна и очищаем текст
        dialogBox1.SetActive(true);
        dialogBox2.SetActive(true);
        dialogText1.text = "";
        dialogText2.text = "";

        // Начинаем с первой реплики
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (currentLine >= dialogue.Length)
        {
            EndDialogue();
            return;
        }

        // Очищаем текст для новой пары реплик
        if (currentLine % 2 == 0)
        {
            dialogText1.text = "";
            dialogText2.text = "";
        }

        bool speaker1Active = isSpeaker1[currentLine];
        string message = dialogue[currentLine];
        currentLine++;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (speaker1Active)
        {
            AnimateDialogueBox(dialogBox1, dialogCanvasGroup1, true);
            AnimateDialogueBox(dialogBox2, dialogCanvasGroup2, false);
            AnimateSpeaker(speakerIcon1, speakerIcon2);
            typingCoroutine = StartCoroutine(TypeText(dialogText1, message));
        }
        else
        {
            AnimateDialogueBox(dialogBox2, dialogCanvasGroup2, true);
            AnimateDialogueBox(dialogBox1, dialogCanvasGroup1, false);
            AnimateSpeaker(speakerIcon2, speakerIcon1);
            typingCoroutine = StartCoroutine(TypeText(dialogText2, message));
        }

        if (audioSource != null)
            audioSource.Play();
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent, string message)
    {
        isTyping = true;
        string baseText = textComponent.text;
        int index = 0;
        while (index <= message.Length)
        {
            textComponent.text = baseText + message.Substring(0, index);
            index++;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        isTyping = false;
    }

    private void AnimateSpeaker(Transform active, Transform inactive)
    {
        LeanTween.scale(active.gameObject, Vector3.one * 1.3f, 0.3f)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true);
        LeanTween.scale(inactive.gameObject, Vector3.one * 1f, 0.3f)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(active.GetComponent<CanvasGroup>(), 1f, 0.3f)
            .setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(inactive.GetComponent<CanvasGroup>(), 0.5f, 0.3f)
            .setIgnoreTimeScale(true);
    }

    private void AnimateDialogueBox(GameObject box, CanvasGroup group, bool active)
    {
        if (active)
        {
            LeanTween.scale(box, Vector3.one * 1.05f, 0.3f)
                .setEase(LeanTweenType.easeOutBack)
                .setIgnoreTimeScale(true);
            LeanTween.alphaCanvas(group, 1f, 0.3f)
                .setIgnoreTimeScale(true);
        }
        else
        {
            LeanTween.scale(box, Vector3.one, 0.3f)
                .setEase(LeanTweenType.easeOutBack)
                .setIgnoreTimeScale(true);
            LeanTween.alphaCanvas(group, 0.8f, 0.3f)
                .setIgnoreTimeScale(true);
        }
    }

    private void EndDialogue()
    {
        isPaused = false;
        Time.timeScale = 1f; // Возвращаем время
        LeanTween.alphaCanvas(dialogCanvasGroup1, 0f, 0.5f)
            .setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(dialogCanvasGroup2, 0f, 0.5f)
            .setIgnoreTimeScale(true);
    }
}