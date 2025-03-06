using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;
    public GameObject interactPrompt; // Привязываем спрайт "E" в инспекторе
    public Vector3 promptOffset = new Vector3(0, 1f, 0); // Смещение над персонажем

    void Start()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false); // Прячем спрайт в начале
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
        // Если спрайт активен, фиксируем его позицию над игроком и убираем поворот
        if (interactPrompt != null && interactPrompt.activeSelf)
        {
            interactPrompt.transform.position = transform.position + promptOffset;
            interactPrompt.transform.rotation = Quaternion.identity; // Сбрасываем поворот
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
            Debug.Log("Можно взаимодействовать! Нажми E");

            if (interactPrompt != null)
                interactPrompt.SetActive(true); // Показываем спрайт "E"
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == currentInteractable)
        {
            currentInteractable = null;
            Debug.Log("Объект вне зоны взаимодействия");

            if (interactPrompt != null)
                interactPrompt.SetActive(false); // Скрываем спрайт "E"
        }
    }
}