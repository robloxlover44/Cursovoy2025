using UnityEngine;

[ExecuteAlways]
public class AimGizmoDrawer : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [Tooltip("Длина прицельной линии в единицах Unity")] 
    public float aimLength = 5f;
    [Tooltip("Цвет линии гизма")] 
    public Color gizmoColor = Color.green;
    [Tooltip("Показывать только если скилл разблокирован (применяется только в режиме Play)")]
    public bool onlyWhenUnlocked = true;
    [Tooltip("ID пассивного скилла, отвечающего за прицел")]
    public string skillID = "Aim";

    private void OnDrawGizmos()
    {
        // Проверяем условие показа
        if (onlyWhenUnlocked && Application.isPlaying)
        {
            if (!PlayerDataManager.Instance.IsSkillUnlocked(skillID))
                return;
        }

        Gizmos.color = gizmoColor;
        Vector3 start = transform.position;
        Vector3 end = start + transform.right * aimLength;

        // Рисуем основную линию
        Gizmos.DrawLine(start, end);

        // Дополнительная стрелочка на конце
        Vector3 dir = (end - start).normalized;
        float arrowHeadAngle = 20f;
        float arrowHeadLength = 0.3f;

        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * dir;
        Vector3 left  = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * dir;

        Gizmos.DrawLine(end, end + right * arrowHeadLength);
        Gizmos.DrawLine(end, end + left  * arrowHeadLength);
    }
}