using System.Collections;
using UnityEngine;

public class DashController : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("ID пассивного навыка для разблокировки")] public string skillID = "Dash";

    [Header("Debug")]
    [Tooltip("Принудительно разблокировать Dash при старте (для тестов)")] public bool forceUnlock = false;

    [Header("Dash Parameters")]
    [Tooltip("Дистанция рывка в юнитах Unity")] public float dashDistance = 5f;
    [Tooltip("Длительность рывка (сек)")] public float dashDuration = 0.1f;
    [Tooltip("Кулдаун перед следующим рывком (сек)")] public float cooldown = 3f;
    [Tooltip("Клавиша активации рывка")] public KeyCode dashKey = KeyCode.Space;

    [Header("Particles")]
    [Tooltip("GameObject с ParticleSystem для эффекта при рывке")] public GameObject dashParticlesObject;

    private Rigidbody2D _rb;
    private PlayerController _playerCtrl;
    private float _nextAvailableTime = 0f;
    private bool _isDashing = false;

    public bool IsDashing => _isDashing;
    public float CooldownRemaining => Mathf.Max(0f, _nextAvailableTime - Time.time);
    public float CooldownDuration => cooldown;

    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        _playerCtrl = GetComponentInParent<PlayerController>();

        if (dashParticlesObject != null)
            dashParticlesObject.SetActive(false);
    }

    private void Update()
    {
        if (!Application.isPlaying || _isDashing)
            return;
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(dashKey))
        {
            if (forceUnlock)
                PlayerDataManager.Instance.UnlockSkill(skillID);

            if (Time.time >= _nextAvailableTime && PlayerDataManager.Instance.IsSkillUnlocked(skillID))
            {
                _nextAvailableTime = Time.time + cooldown;
                StartCoroutine(PerformDash());
            }
            else if (Time.time < _nextAvailableTime)
            {
                Debug.Log($"[DashController] Dash cooldown: {(_nextAvailableTime - Time.time):F1}s remaining");
            }
        }
    }

    private IEnumerator PerformDash()
{
    _isDashing = true;

    Vector2 dashDir = Vector2.right;
    if (_playerCtrl != null && _playerCtrl.LastMovementDirection.sqrMagnitude > 0f)
        dashDir = _playerCtrl.LastMovementDirection.normalized;

    Vector2 startPos = _rb.position;
    Vector2 intendedTarget = startPos + dashDir * dashDistance;

    // Главное отличие: используем Raycast с маленьким сдвигом старта
    Vector2 rayStart = startPos + dashDir * 0.01f; // чуть вперед, чтобы не застревать в стене
    float dashLen = Vector2.Distance(rayStart, intendedTarget);

    int wallMask = LayerMask.GetMask("Wall"); // Имя слоя стен!
    RaycastHit2D hit = Physics2D.Raycast(rayStart, dashDir, dashLen, wallMask);

    Vector2 dashTarget = intendedTarget;
    if (hit.collider != null)
    {
        dashTarget = hit.point;
        // Debug.Log($"Игрок dash остановлен у стены: {hit.collider.name} {hit.point}");
    }

    if (dashParticlesObject != null)
        dashParticlesObject.SetActive(true);

    float elapsed = 0f;
    while (elapsed < dashDuration)
    {
        float t = elapsed / dashDuration;
        _rb.MovePosition(Vector2.Lerp(startPos, dashTarget, t));
        elapsed += Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();
    }
    _rb.MovePosition(dashTarget);

    if (dashParticlesObject != null)
        dashParticlesObject.SetActive(false);

    _isDashing = false;
}

}
