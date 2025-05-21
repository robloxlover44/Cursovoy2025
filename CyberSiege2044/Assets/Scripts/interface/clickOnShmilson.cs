using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // для работы со сценами
using System.Collections.Generic;

public class ResourceButton : MonoBehaviour
{
    public string mainSceneName = "MainScene"; // Название твоей основной сцены
    private bool hasBeenClicked = false;       // чтобы не срабатывать дважды

    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("[ResourceButton] Нет Button компонента!");
            return;
        }
        btn.onClick.AddListener(DisableRedObjects);
    }

    private void DisableRedObjects()
    {
        if (hasBeenClicked) return;
        hasBeenClicked = true;

        // пытаемся найти основную сцену по имени
        Scene main = SceneManager.GetSceneByName(mainSceneName);
        if (!main.IsValid() || !main.isLoaded)
        {
            Debug.LogError($"[ResourceButton] Сцена \"{mainSceneName}\" не загружена или не найдена!");
            return;
        }

        // Проходим по корневым объектам в основной сцене
        var roots = main.GetRootGameObjects();
        var turnedOff = new List<GameObject>();
        foreach (var root in roots)
        {
            // ищем ВСЕ объекты с тегом RED в иерархии этого корня
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.CompareTag("RED"))
                {
                    child.gameObject.SetActive(false);
                    turnedOff.Add(child.gameObject);
                }
            }
        }

        Debug.Log($"[ResourceButton] Отключено объектов RED: {turnedOff.Count}");
    }
}
