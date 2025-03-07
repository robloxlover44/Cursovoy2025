using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnToFirstScene : MonoBehaviour
{
    private Camera firstSceneCamera;
    private Canvas firstSceneCanvas;

    private void Start()
    {
        firstSceneCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        firstSceneCanvas = GameObject.FindObjectOfType<Canvas>();
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
        if (SceneManager.sceneCount > 1)
        {
            Scene lastScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            Debug.Log("Unloading scene: " + lastScene.name);

            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(lastScene);
            if (asyncUnload == null)
            {
                Debug.LogError("Failed to start unloading scene: " + lastScene.name);
                yield break;
            }

            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            Debug.Log("Scene unloaded successfully: " + lastScene.name);

            // Устанавливаем первую сцену как активную
            Scene firstScene = SceneManager.GetSceneAt(0);
            SceneManager.SetActiveScene(firstScene);

            if (firstSceneCamera != null)
            {
                firstSceneCamera.gameObject.SetActive(true);
                firstSceneCamera.Render(); // Принудительно рендерим
            }
            if (firstSceneCanvas != null)
                firstSceneCanvas.gameObject.SetActive(true);

            // Уничтожаем этот объект, чтобы он не остался в DontDestroyOnLoad
            Destroy(gameObject);

            yield return null; // Ждем кадр
        }
        else
        {
            Debug.LogError("No additive scene to unload! Only one scene is loaded.");
        }
    }
}