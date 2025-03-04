using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bar : MonoBehaviour, IInteractable
{
    public string sceneToLoad; // Название сцены, указывается в инспекторе
    private FadeController fadeController;

    private void Start()
    {
        fadeController = FindObjectOfType<FadeController>();
    }

    public void Interact()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneWithFade());
        }
    }

    private IEnumerator LoadSceneWithFade()
    {
        yield return fadeController.FadeOut(); // Затемняем экран
        SceneManager.LoadScene(sceneToLoad); // Загружаем сцену
        yield return fadeController.FadeIn(); // Осветляем экран
    }
}