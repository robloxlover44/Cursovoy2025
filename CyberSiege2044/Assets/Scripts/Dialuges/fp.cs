using UnityEngine;
using TMPro;
using System.Collections;
using static LeanTween;

public class DialogueSystemFP : MonoBehaviour
{
    [Header("��������� �������")]
    [SerializeField] private GameObject bg;           

    [SerializeField] private GameObject dialogBox1;           // ������ ���������� ����
    [SerializeField] private GameObject dialogBox2;           // ������ ���������� ����
    [SerializeField] private CanvasGroup dialogCanvasGroup1;  // CanvasGroup ��� �������� ������� ����
    [SerializeField] private CanvasGroup dialogCanvasGroup2;  // CanvasGroup ��� �������� ������� ����
    [SerializeField] private TextMeshProUGUI dialogText1;     // ����� ������� ����
    [SerializeField] private TextMeshProUGUI dialogText2;     // ����� ������� ����
    [SerializeField] private Transform speakerIcon1;          // ������ ������� ����������
    [SerializeField] private Transform speakerIcon2;          // ������ ������� ����������
    [SerializeField] private float typingSpeed = 0.05f;       // �������� ������ ������
    [SerializeField] private KeyCode nextKey = KeyCode.Space; // ������� ��� �������� � ��������� �������

    [Header("������ �������")]
    [SerializeField] private string[] dialogue;               // ������ ������
    [SerializeField] private bool[] isSpeaker1;               // ��� �������: true � ������, false � ������

    [Header("����")]
    [SerializeField] private AudioSource audioSource;         // �������� ����� ��� �������

    [Header("�������� ����� ��������")]
    [SerializeField] private float delayBeforeDialogue = 0.5f; // �������� ����� ������� (������������� � ����������)

    private int currentLine = 0;         // ������� �������
    private bool isPaused = false;       // ���� ����� ����
    private bool isTyping = false;       // ���� ������ ������
    private Coroutine typingCoroutine;   // �������� ��� ������ ������

    private void Start()
    {
        // ���������� �������� ���������� ����
        dialogBox1.SetActive(false);
        dialogBox2.SetActive(false);
        bg.SetActive(false);

        // ��������� ������ � ���������
        StartCoroutine(StartDialogueAfterDelay());
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        // ���� ��������� �����
        yield return new WaitForSeconds(delayBeforeDialogue);

        // ��������� ������
        StartDialogue();
    }

    private void Update()
    {
        // ������� � ��������� ������� �� ������� �������
        if (isPaused && Input.GetKeyDown(nextKey))
        {
            ShowNextLine();
        }
    }

    private void StartDialogue()
    {
        isPaused = true;
        Time.timeScale = 0f; // ������������� ����� � ����
        currentLine = 0;

        // ���������� ���� � ������� �����
        dialogBox1.SetActive(true);
        dialogBox2.SetActive(true);
        bg.SetActive(true);
        dialogText1.text = "";
        dialogText2.text = "";

        // �������� � ������ �������
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (currentLine >= dialogue.Length)
        {
            EndDialogue();
            return;
        }

        // ������� ����� ��� ����� ���� ������
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
        Time.timeScale = 1f; // ���������� �����
        LeanTween.alphaCanvas(dialogCanvasGroup1, 0f, 0.5f)
            .setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(dialogCanvasGroup2, 0f, 0.5f)
            .setIgnoreTimeScale(true);
        bg.SetActive(false);
    }
}

