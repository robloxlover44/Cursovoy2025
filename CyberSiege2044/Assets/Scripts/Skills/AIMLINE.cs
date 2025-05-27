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
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.widthMultiplier = 0.01f;
        if (lr.material == null)
            lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 100;
    }

    void Update()
    {
        if (onlyWhenUnlocked && !PlayerDataManager.Instance.IsSkillUnlocked(skillID))
        {
            lr.enabled = false;
            return;
        }

        lr.enabled = true;

        Vector2 origin = transform.position;
        Vector2 direction = transform.right;

        // Новый Raycast, который ищет только НЕ-триггерные коллайдеры!
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, aimLength, obstacleMask);

        Vector3 startPoint = origin;
        Vector3 endPoint = origin + direction * aimLength;

        // Проверяем, что коллайдер найден и НЕ является триггером
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            endPoint = hit.point;
        }
        else if (hit.collider != null && hit.collider.isTrigger)
        {
            // Если первый коллайдер — триггер, ищем дальше по лучу…
            // CastAll и ищем первый НЕ-триггерный хит
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, aimLength, obstacleMask);
            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.isTrigger)
                {
                    endPoint = h.point;
                    break;
                }
            }
        }

        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, endPoint);
    }
}
