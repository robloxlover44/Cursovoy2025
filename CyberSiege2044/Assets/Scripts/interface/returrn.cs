using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class ReturnToFirstScene : MonoBehaviour
{
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

            Camera[] allCameras = Camera.allCameras;
            foreach (Camera cam in allCameras)
            {
                if (cam.gameObject.scene == lastScene)
                {
                    cam.gameObject.SetActive(false);
                    Debug.Log("Disabled camera in second scene: " + cam.name);
                }
            }

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

            Scene firstScene = SceneManager.GetSceneAt(0);
            SceneManager.SetActiveScene(firstScene);

            Camera firstSceneCamera = null;
            GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (cameraObj != null)
            {
                firstSceneCamera = cameraObj.GetComponent<Camera>();
            }

            if (firstSceneCamera == null)
            {
                firstSceneCamera = firstScene.GetRootGameObjects()
                    .SelectMany(go => go.GetComponentsInChildren<Camera>(true))
                    .FirstOrDefault();
                Debug.Log("Fallback search for camera in first scene: " + (firstSceneCamera != null ? firstSceneCamera.name : "Not found"));
            }

            if (firstSceneCamera != null)
            {
                firstSceneCamera.gameObject.SetActive(true);
                Debug.Log("First scene camera enabled: " + firstSceneCamera.name);
            }
            else
            {
                Debug.LogError("No camera found in first scene even after exhaustive search!");
            }

            Destroy(gameObject);

            yield return null;
        }
        else
        {
            Debug.LogError("No additive scene to unload!");
        }
    }
}