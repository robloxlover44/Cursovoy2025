using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Включить автопереход при входе")]
    public bool autoLoad = false;

    [Header("Кнопка для ручного перехода")]
    public KeyCode interactKey = KeyCode.E;

    private bool playerIsNear = false;

    private void Update()
    {
        if (playerIsNear && !autoLoad && Input.GetKeyDown(interactKey))
        {
            Debug.Log("Игрок нажал клавишу E, пытаемся загрузить следующий уровень");
            TryLoadNextLevel();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Игрок вошел в триггер портала");
        playerIsNear = true;

        if (autoLoad)
        {
            Debug.Log("autoLoad включен, загружаем следующий уровень");
            TryLoadNextLevel();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Игрок покинул триггер портала");
        playerIsNear = false;
    }

    private void TryLoadNextLevel()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager.Instance is null!");
            return;
        }

        PlayerDataManager.Instance.LoadNextLevel();
    }
}