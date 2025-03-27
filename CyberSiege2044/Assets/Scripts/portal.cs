using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerSceneChange : MonoBehaviour
{
    [SerializeField] private int sceneIndex; // Номер сцены задается в инспекторе

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, является ли объект, вошедший в триггер, игроком
        if (collision.CompareTag("Player"))
        {
            // Загружаем сцену с указанным индексом
            SceneManager.LoadScene(sceneIndex);
        }
    }
}