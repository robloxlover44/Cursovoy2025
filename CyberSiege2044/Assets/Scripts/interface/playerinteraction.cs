using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;
    public GameObject interactPrompt; // ����������� ������ "E" � ����������
    public Vector3 promptOffset = new Vector3(0, 1f, 0); // �������� ��� ����������

    void Start()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false); // ������ ������ � ������
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void LateUpdate()
    {
        // ���� ������ �������, ��������� ��� ������� ��� ������� � ������� �������
        if (interactPrompt != null && interactPrompt.activeSelf)
        {
            interactPrompt.transform.position = transform.position + promptOffset;
            interactPrompt.transform.rotation = Quaternion.identity; // ���������� �������
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
            Debug.Log("����� �����������������! ����� E");

            if (interactPrompt != null)
                interactPrompt.SetActive(true); // ���������� ������ "E"
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == currentInteractable)
        {
            currentInteractable = null;
            Debug.Log("������ ��� ���� ��������������");

            if (interactPrompt != null)
                interactPrompt.SetActive(false); // �������� ������ "E"
        }
    }
}