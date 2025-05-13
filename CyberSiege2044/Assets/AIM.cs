using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimLineRenderer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Длина прицельной линии")]
    public float aimLength = 5f;
    [Tooltip("Показывать только если навык разблокирован")] 
    public bool onlyWhenUnlocked = true;
    [Tooltip("ID пассивного навыка")] 
    public string skillID = "Aim";
    [Tooltip("Слой(ы) для обнаружения препятствий при прицеливании")]
    public LayerMask obstacleMask = Physics2D.DefaultRaycastLayers;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        // Всегда две точки
        lr.positionCount = 2;
        // Используем мировые координаты
        lr.useWorldSpace = true;
        // Ширина линии
        lr.widthMultiplier = 0.01f;
        // Устанавливаем материал по умолчанию, если его нет
        if (lr.material == null)
            lr.material = new Material(Shader.Find("Sprites/Default"));
        // Сортировка поверх спрайтов
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 100;
    }

    void Update()
    {
        // Если навык не разблокирован, скрываем линию
        if (onlyWhenUnlocked && !PlayerDataManager.Instance.IsSkillUnlocked(skillID))
        {
            lr.enabled = false;
            return;
        }

        lr.enabled = true;

        // Определяем начало и направление луча
        Vector2 origin = transform.position;
        Vector2 direction = transform.right;

        // Выполняем Raycast в 2D
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, aimLength, obstacleMask);

        Vector3 startPoint = origin;
        Vector3 endPoint;

        if (hit.collider != null)
        {
            // Останавливаемся на точке столкновения
            endPoint = hit.point;
        }
        else
        {
            // Максимальная длина
            endPoint = origin + direction * aimLength;
        }

        // Устанавливаем позиции линии
        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, endPoint);
    }
}