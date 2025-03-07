using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bar : MonoBehaviour, IInteractable
{
    public string sceneToLoad = "bar Store";
    private FadeController fadeController;
    private Camera firstSceneCamera;

    private void Start()
    {
        fadeController = FindObjectOfType<FadeController>();
        firstSceneCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        if (firstSceneCamera == null)
        {
            Debug.LogError("No MainCamera found at start in Bar!");
        }
        else
        {
            Debug.Log($"Scene to load: {sceneToLoad}, First camera: {firstSceneCamera.name}");
        }
    }

    public void Interact()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAdditiveWithFade());
        }
    }

    private IEnumerator LoadSceneAdditiveWithFade()
    {
        Debug.Log("Starting scene load: " + sceneToLoad);
        if (firstSceneCamera != null)
        {
            firstSceneCamera.gameObject.SetActive(false);
            Debug.Log("First scene camera disabled");
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        if (asyncLoad == null)
        {
            Debug.LogError("Failed to start loading scene: " + sceneToLoad);
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("Scene loaded: " + sceneToLoad);
    }
}