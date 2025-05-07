using UnityEngine;
using UnityEngine.SceneManagement;

public class barportal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Загружаем следующий уровень через менеджер данных
            int next = PlayerDataManager.Instance.GetNextLevelIndex();
            SceneManager.LoadScene(next);
        }
    }
}