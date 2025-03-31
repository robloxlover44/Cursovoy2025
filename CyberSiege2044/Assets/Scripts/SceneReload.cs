using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    public void ReloadScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Обновляем здоровье через публичный метод
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.RefreshHealth(); // Устанавливаем здоровье и уведомляем
        }

        SceneManager.LoadScene(currentSceneName);
    }
}