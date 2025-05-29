using UnityEngine;

public class BackgroundMusicSwitcher : MonoBehaviour
{
    [Header("��������� �����")]
    [Tooltip("����� �������� ��� ������� ������")]
    public AudioSource audioSource;

    [Tooltip("�������� ���������")]
    public AudioClip primaryClip;

    [Tooltip("�������������� ��������� (���������� ��� ��������� �������)")]
    public AudioClip secondaryClip;

    [Header("������ ��� ������������")]
    [Tooltip("������, ��������� �������� ����������� ������")]
    public GameObject targetObject;

    private bool isUsingSecondaryClip = false;
    private float currentPlayTime = 0f;

    void Update()
    {
        if (targetObject == null || audioSource == null || primaryClip == null || secondaryClip == null)
        {
            Debug.LogError("�� ��� ������ ��������� � ����������!");
            return;
        }

        // ��������� ��������� ������� � ����������� ������
        if (targetObject.activeSelf && !isUsingSecondaryClip)
        {
            SwitchToClip(secondaryClip);
            isUsingSecondaryClip = true;
        }
        else if (!targetObject.activeSelf && isUsingSecondaryClip)
        {
            SwitchToClip(primaryClip);
            isUsingSecondaryClip = false;
        }
    }

    private void SwitchToClip(AudioClip clip)
    {
        // ��������� ������� ����� ���������������
        currentPlayTime = audioSource.time;

        // ����������� ���������
        audioSource.clip = clip;

        // ������������� ������� ���������������
        audioSource.time = currentPlayTime;

        // ����������� ����� ����
        audioSource.Play();
    }
}
