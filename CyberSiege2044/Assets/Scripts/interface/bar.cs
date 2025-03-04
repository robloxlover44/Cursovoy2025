using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bar : MonoBehaviour, IInteractable
{
    public string sceneToLoad; // �������� �����, ����������� � ����������
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
        yield return fadeController.FadeOut(); // ��������� �����
        SceneManager.LoadScene(sceneToLoad); // ��������� �����
        yield return fadeController.FadeIn(); // ��������� �����
    }
}