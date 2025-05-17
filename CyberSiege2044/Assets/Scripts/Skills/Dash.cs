using System.Collections;
using UnityEngine;

// Скрипт отвечает за механику рывка (Dash) с простым включением/отключением GameObject частиц
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

    /// <summary>Флаг, показывающий, что сейчас идёт рывок</summary>
    public bool IsDashing => _isDashing;
    /// <summary>Оставшееся время кулдауна (0 если готов)</summary>
    public float CooldownRemaining => Mathf.Max(0f, _nextAvailableTime - Time.time);
    /// <summary>Полная длительность кулдауна</summary>
    public float CooldownDuration => cooldown;

    private void Awake()
    {
        // Находим Rigidbody2D и PlayerController у родителя (игрока)
        _rb = GetComponentInParent<Rigidbody2D>();
        _playerCtrl = GetComponentInParent<PlayerController>();

        // Отключаем объект частиц при старте
        if (dashParticlesObject != null)
            dashParticlesObject.SetActive(false);
    }

    private void Update()
    {
        if (!Application.isPlaying || _isDashing)
            return;

        if (Input.GetKeyDown(dashKey))
        {
            // Принудительная разблокировка навыка (для тестирования)
            if (forceUnlock)
                PlayerDataManager.Instance.UnlockSkill(skillID);

            // Проверяем доступность рывка
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

        // Определяем направление рывка: последнее движение или вправо по умолчанию
        Vector2 dashDir = Vector2.right;
        if (_playerCtrl != null && _playerCtrl.LastMovementDirection.sqrMagnitude > 0f)
            dashDir = _playerCtrl.LastMovementDirection.normalized;

        Vector2 startPos = _rb.position;
        Vector2 targetPos = startPos + dashDir * dashDistance;

        // Включаем GameObject частиц
        if (dashParticlesObject != null)
            dashParticlesObject.SetActive(true);

        // Плавное перемещение за dashDuration
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            _rb.MovePosition(Vector2.Lerp(startPos, targetPos, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        _rb.MovePosition(targetPos);

        // Отключаем GameObject частиц
        if (dashParticlesObject != null)
            dashParticlesObject.SetActive(false);

        _isDashing = false;
    }
}