using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnToFirstScene : MonoBehaviour
{
    private void Awake()
    {
         DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(UnloadLastScene());
        }
    }

    private IEnumerator UnloadLastScene()
    {
        // Проверяем, есть ли что выгружать
        if (SceneManager.sceneCount > 1)
        {
            Scene lastScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(lastScene);
            while (!asyncUnload.isDone)
            {
                yield return null;
            }
            Debug.Log($"Scene unloaded: {lastScene.name}");
        }

        // Просто включаем камеру, если она есть
        if (Bar.firstSceneCamera != null)
        {
            Bar.firstSceneCamera.gameObject.SetActive(true);
            Debug.Log($"First scene camera enabled: {Bar.firstSceneCamera.name}");
        }
        else
        {
            Debug.LogError("First scene camera is null! Check Bar.cs initialization.");
        }

        // Уничтожаем объект
        Destroy(gameObject);
    }
}