using UnityEngine;
using TMPro;
using System.Collections;
using static LeanTween;

public class DialogueSystem : MonoBehaviour
{
    [Header("Настройки диалога")]
    [SerializeField] private GameObject dialogBox1;
    [SerializeField] private GameObject dialogBox2;
    [SerializeField] private CanvasGroup dialogCanvasGroup1;
    [SerializeField] private CanvasGroup dialogCanvasGroup2;
    [SerializeField] private TextMeshProUGUI dialogText1;
    [SerializeField] private TextMeshProUGUI dialogText2;
    [SerializeField] private Transform speakerIcon1;
    [SerializeField] private Transform speakerIcon2;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private KeyCode nextKey = KeyCode.Space;
    
    [Header("Настройки диалога")]
    [SerializeField] private string[] dialogue;
    [SerializeField] private bool[] isSpeaker1;
    
    [Header("Настройки звука")]
    [SerializeField] private AudioSource audioSource;
    
    private int currentLine = 0;
    private bool isPaused = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Collider2D triggerCollider;
    
    private void Start()
    {
        // В начале оба окна не видны
        dialogBox1.SetActive(false);
        dialogBox2.SetActive(false);
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
            StartDialogue();
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
        if (isPaused && Input.GetKeyDown(nextKey))
        {
            ShowNextLine();
        }
    }
    
    private void StartDialogue()
    {
        isPaused = true;
        Time.timeScale = 0f;
        currentLine = 0;
        
        // Активируем оба окна и очищаем их текст, чтобы диалог накапливался с нуля
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
        
        bool speaker1Active = isSpeaker1[currentLine];
        string message = dialogue[currentLine];
        currentLine++;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        if (speaker1Active)
        {
            // Анимация диалоговых окон
            AnimateDialogueBox(dialogBox1, dialogCanvasGroup1, true);
            AnimateDialogueBox(dialogBox2, dialogCanvasGroup2, false);
            // Анимация иконок
            AnimateSpeaker(speakerIcon1, speakerIcon2);
            // Запускаем анимацию набора текста, дописывая в конец уже существующего текста
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
        // Если уже есть текст, добавляем перенос строки перед новой репликой
        string prefix = baseText.Length > 0 ? "\n" : "";
        int index = 0;
        while (index <= message.Length)
        {
            textComponent.text = baseText + prefix + message.Substring(0, index);
            index++;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        isTyping = false;
    }
    
    // Анимация для иконок говорящих с игнорированием Time.timeScale
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
    
    // Анимация для диалоговых окон (подсвечиваем активное окно и затемняем неактивное)
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
        Time.timeScale = 1f;
        // Плавно исчезаем оба окна
        LeanTween.alphaCanvas(dialogCanvasGroup1, 0f, 0.5f)
            .setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(dialogCanvasGroup2, 0f, 0.5f)
            .setIgnoreTimeScale(true);
    }
}
