using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bar : MonoBehaviour, IInteractable
{
    public string sceneToLoad;
    private FadeController fadeController;
    private Camera firstSceneCamera;
    private Canvas firstSceneCanvas;

    private void Start()
    {
        fadeController = FindObjectOfType<FadeController>();
        firstSceneCamera = Camera.main;
        firstSceneCanvas = FindObjectOfType<Canvas>();
        Debug.Log($"Scene to load: {sceneToLoad}");
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
        //yield return fadeController.FadeOut(); // Закомментировано
        Debug.Log("Starting scene load: " + sceneToLoad);
        if (firstSceneCamera != null)
            firstSceneCamera.gameObject.SetActive(false);
        if (firstSceneCanvas != null)
            firstSceneCanvas.gameObject.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        if (asyncLoad == null)
        {
            Debug.LogError("Failed to start loading scene: " + sceneToLoad);
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            Debug.Log("Loading progress: " + asyncLoad.progress);
            yield return null;
        }

        Debug.Log("Scene loaded: " + sceneToLoad);
        //yield return fadeController.FadeIn(); // Закомментировано
    }
}