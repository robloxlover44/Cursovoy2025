using UnityEngine;

public class RoomDoorController : MonoBehaviour
{
    [Header("Ссылки на все двери этой комнаты")]
    public GameObject[] doors;

    [Header("Где искать врагов?")]
    public Transform enemiesRoot; // В этот объект помещай врагов комнаты (например, пустышка "Enemies")

    [Header("Слой или тег врагов (если надо)")]
    public string enemyTag = "Enemy"; // если не хочешь искать по тегу — оставь пустым

    private bool roomLocked = false;
    private bool cleared = false;

    private void Start()
    {
        // Двери на старте открыты (выключены)
        foreach (var d in doors) if (d) d.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!roomLocked && other.CompareTag("Player"))
        {
            LockRoom();
        }
    }

    void LockRoom()
    {
        roomLocked = true;
        foreach (var d in doors) if (d) d.SetActive(true);
        // Проверять врагов каждые 0.5 сек, пока не все мертвы
        InvokeRepeating(nameof(CheckEnemies), 0.2f, 0.5f);
    }

    void CheckEnemies()
    {
        int count = 0;
        if (!string.IsNullOrEmpty(enemyTag))
        {
            // Поиск по тегу
            if (enemiesRoot != null)
                foreach (Transform child in enemiesRoot)
                    if (child.gameObject.activeInHierarchy && child.CompareTag(enemyTag))
                        count++;
            else
                count = GameObject.FindGameObjectsWithTag(enemyTag).Length;
        }
        else if (enemiesRoot != null)
        {
            // Считаем всех детей
            foreach (Transform child in enemiesRoot)
                if (child.gameObject.activeInHierarchy)
                    count++;
        }

        if (count == 0 && !cleared)
        {
            cleared = true;
            OpenDoors();
        }
    }

    void OpenDoors()
    {
        foreach (var d in doors) if (d) d.SetActive(false);
        CancelInvoke(nameof(CheckEnemies));
    }
}
