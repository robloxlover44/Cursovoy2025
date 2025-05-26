using UnityEngine;

public class Chest : MonoBehaviour
{
    public GameObject wirePuzzlePanel; // Панель мини-игры (WirePuzzlePanel, на ней скрипт WirePuzzle)
    public GameObject lootPrefab;      // Префаб лута (файл/монетка)
    public Transform lootSpawnPoint;   // Где спавнить лут (можно сам сундук)
    public int minLoot = 1;
    public int maxLoot = 3;

    private bool canOpen = false;
    private bool isOpened = false;

    private void Update()
    {
        if (canOpen && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            if (wirePuzzlePanel != null)
            {
                wirePuzzlePanel.SetActive(true);   // Включаем панель мини-игры
                Time.timeScale = 0f;               // Ставим игру на паузу
                // Передаём ссылку на этот сундук в WirePuzzle:
                WirePuzzle puzzle = wirePuzzlePanel.GetComponent<WirePuzzle>();
                if (puzzle != null)
                    puzzle.targetChest = this;
            }
        }
    }

    // Этот метод вызывается WirePuzzle после победы в мини-игре!
    public void UnlockChest()
    {
        if (isOpened) return;

        isOpened = true;

        // Дроп лута
        int lootCount = Random.Range(minLoot, maxLoot + 1);
        for (int i = 0; i < lootCount; i++)
        {
            Instantiate(lootPrefab, lootSpawnPoint.position, Quaternion.identity);
        }

        // Тут можно запустить анимацию открытия
        // GetComponent<Animator>()?.SetTrigger("Open");

        // Скрываем мини-игру (на всякий)
        if (wirePuzzlePanel != null)
            wirePuzzlePanel.SetActive(false);

        // Возвращаем время в игре
        Time.timeScale = 1f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpen = true;
            // Включи UI подсказку "Press E to hack chest" если надо
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpen = false;
            // Выключи UI подсказку если надо
        }
    }
}