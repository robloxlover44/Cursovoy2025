// SceneReloader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    public void ReloadScene()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.LoadCheckpointState(); // Возвращаемся к последнему чекпоинту
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
