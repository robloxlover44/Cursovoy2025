using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void OnEnable()
    {
        // Скрыть курсор и заблокировать его в центре экрана
        Cursor.visible = false;
    }

    void OnDisable()
    {
        // Показать курсор и разблокировать его
        Cursor.visible = true;
    }
}
