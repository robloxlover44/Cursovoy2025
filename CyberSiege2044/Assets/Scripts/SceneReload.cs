using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Для работы с UI

public class SceneReload : MonoBehaviour
{
    [SerializeField] private Button reloadButton; // Ссылка на кнопку в инспекторе

    void Start()
    {
        // Добавляем слушатель к кнопке
        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(ReloadScene);
        }
    }

    void ReloadScene()
    {
        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Убираем слушатель при уничтожении объекта (хорошая практика)
    void OnDestroy()
    {
        if (reloadButton != null)
        {
            reloadButton.onClick.RemoveListener(ReloadScene);
        }
    }
}