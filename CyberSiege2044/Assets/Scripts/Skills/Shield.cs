using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ShieldController : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("ID пассивного навыка")]
    public string skillID = "Shield";

    [Header("Debug")]
    [Tooltip("Принудительно разблокировать щит в инспекторе")]
    public bool forceUnlock = false;

    [Header("Animation Frames")]
    [Tooltip("Спрайты открытия щита")]
    public Sprite[] activationSprites;
    [Tooltip("Спрайты закрытия щита")]
    public Sprite[] deactivationSprites;

    [Header("Timing")]
    [Tooltip("Время отображения каждого кадра (сек)")]
    public float frameDuration = 0.1f;
    [Tooltip("Общая продолжительность удержания щита (сек)")]
    public float sustainDuration = 2f;

    [Header("Input & Cooldown")]
    [Tooltip("Кнопка активации щита")]
    public KeyCode activationKey = KeyCode.F;
    [Tooltip("Кулдаун перед следующей активацией (сек)")]
    public float cooldown = 5f;

    [Header("Hit Settings")]
    [Tooltip("Сколько попаданий пуль выдерживает щит")]
    public int maxBulletHits = 3;
    [Tooltip("Тег врага для мгновенного разрушения")]
    public string enemyTag = "Enemy";
    [Tooltip("Тег вражеской пули")]
    public string bulletTag = "e-bullet";

    private SpriteRenderer _sr;
    private Collider2D _col;
    private float _nextAvailableTime = 0f;
    private Coroutine _shieldRoutine;
    private bool _isBreaking;
    private int _bulletHits;

    // Свойство для проверки внешними скриптами, активен ли щит
    public bool IsShieldActive => _sr.enabled && !_isBreaking;
    // Для UI: оставшееся время кулдауна (0 если готов)
    public float CooldownRemaining => Mathf.Max(0f, _nextAvailableTime - Time.time);
    // Для UI: длительность кулдауна
    public float CooldownDuration => cooldown;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        // Скрываем визуал и коллайдер, но держим объект активным для Update()
        _sr.enabled = false;
        _col.enabled = false;
    }

    private void Update()
    {
        if (!Application.isPlaying || _isBreaking)
            return;

        if (Input.GetKeyDown(activationKey))
        {
            Debug.Log("[ShieldController] Key pressed: " + activationKey);
            if (Time.time >= _nextAvailableTime)
            {
                ActivateShield();
                _nextAvailableTime = Time.time + cooldown;
            }
            else
            {
                Debug.Log($"[ShieldController] Shield cooldown: {(_nextAvailableTime - Time.time):F1}s remaining");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_col.enabled || _isBreaking)
            return;

        // Разрушение при столкновении с EdgeCollider2D врага
        if (other.CompareTag(enemyTag) && other is EdgeCollider2D)
        {
            Debug.Log("[ShieldController] Hit by Enemy EdgeCollider, breaking shield immediately");
            BreakShield();
        }
        // Обработка попаданий пуль
        else if (other.CompareTag(bulletTag))
        {
            _bulletHits++;
            Debug.Log($"[ShieldController] Bullet hit #{_bulletHits}");
            Destroy(other.gameObject);
            if (_bulletHits >= maxBulletHits)
            {
                Debug.Log("[ShieldController] Max bullet hits reached, breaking shield");
                BreakShield();
            }
        }
    }

    /// <summary>
    /// Активирует щит, если навык куплен или принудительно разблокирован.
    /// </summary>
    public void ActivateShield()
    {
        bool isUnlocked = PlayerDataManager.Instance.IsSkillUnlocked(skillID);
        if (!forceUnlock && !isUnlocked)
        {
            Debug.Log($"[ShieldController] Skill {skillID} not unlocked");
            return;
        }

        Debug.Log("[ShieldController] Activating shield");
        // Сброс состояния
        _bulletHits = 0;
        _isBreaking = false;

        if (_shieldRoutine != null)
            StopCoroutine(_shieldRoutine);

        _sr.enabled = true;
        _col.enabled = true;
        _shieldRoutine = StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        // 1) Анимация открытия
        foreach (var frame in activationSprites)
        {
            _sr.sprite = frame;
            yield return new WaitForSeconds(frameDuration);
        }

        // 2) Удержание с повтором последних кадров или досрочный разрыв
        float elapsed = 0f;
        int last = activationSprites.Length - 1;
        int prev = Mathf.Max(activationSprites.Length - 2, 0);
        while (elapsed < sustainDuration && !_isBreaking)
        {
            _sr.sprite = activationSprites[prev];
            yield return new WaitForSeconds(frameDuration);
            _sr.sprite = activationSprites[last];
            yield return new WaitForSeconds(frameDuration);
            elapsed += frameDuration * 2f;
        }

        // 3) Закрытие, если не было досрочного разрушения
        if (!_isBreaking)
            yield return PlayDeactivationSequence();
    }

    /// <summary>
    /// Разрушает щит досрочно и запускает анимацию закрытия.
    /// </summary>
    public void BreakShield()
    {
        if (_isBreaking)
            return;
        _isBreaking = true;
        if (_shieldRoutine != null)
            StopCoroutine(_shieldRoutine);
        StartCoroutine(PlayDeactivationSequence());
    }

    private IEnumerator PlayDeactivationSequence()
    {
        // Анимация закрытия
        foreach (var frame in deactivationSprites)
        {
            _sr.sprite = frame;
            yield return new WaitForSeconds(frameDuration);
        }

        // Отключаем щит
        _col.enabled = false;
        _sr.enabled = false;
        _shieldRoutine = null;
        _isBreaking = false;
    }
}
