using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites;
    public Sprite[] runSprites;
    public float animationSpeed = 0.1f;
    public Transform cameraTransform;
    public Vector3 cameraOffset;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.right;
    public Vector2 LastMovementDirection => lastDirection;
    private Rigidbody2D rb;
    private DashController dashController;
    private Camera mainCamera;
    private float animationTimer;
    private int currentFrame;
    private bool isMoving;
    

    public List<GameObject> inventoryWeaponObjects = new List<GameObject>();
    public GameObject currentWeaponObject;
    private int currentWeaponIndex = -1;

    public float shakeIntensity = 0.2f;
    public float shakeSpeed = 5f;
    private float shakeTimeOffsetX;
    private float shakeTimeOffsetY;

    [Header("Death Settings")]
    public Sprite[] deathSprites;
    public GameObject gameOverPanel;
    public float deathAnimationSpeed = 0.2f;
    public Weapon currentWeapon;

    [Header("UI Settings")]
    public TMP_Text ammoText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashController = GetComponentInChildren<DashController>();
        mainCamera = Camera.main;

        if (cameraTransform != null && cameraOffset == Vector3.zero)
        {
            cameraOffset = cameraTransform.position - transform.position;
        }

        shakeTimeOffsetX = Random.value * 10f;
        shakeTimeOffsetY = Random.value * 10f;

        List<string> weaponIDs = PlayerDataManager.Instance.GetInventoryWeapons();
        foreach (string weaponID in weaponIDs)
        {
            GameObject weaponPrefab = Resources.Load<GameObject>("Weapons/" + weaponID);
            if (weaponPrefab != null)
            {
                GameObject newWeaponObject = Instantiate(weaponPrefab, transform.position, Quaternion.identity);
                newWeaponObject.transform.SetParent(transform);
                SpriteRenderer[] weaponSprites = newWeaponObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sprite in weaponSprites)
                {
                    sprite.enabled = false;
                }
                inventoryWeaponObjects.Add(newWeaponObject);
                newWeaponObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Не удалось загрузить префаб оружия: " + weaponID);
            }
        }

        currentWeaponIndex = PlayerDataManager.Instance.GetCurrentWeaponIndex();
        if (currentWeaponIndex >= 0 && currentWeaponIndex < inventoryWeaponObjects.Count)
        {
            SwitchWeapon(currentWeaponIndex);
        }
        else if (inventoryWeaponObjects.Count > 0)
        {
            SwitchWeapon(0);
        }

        UpdateAmmoUI();
    }
    private bool isDead = false; // Флаг смерти
    void Update()
    {
        if (isDead) return;
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        isMoving = movement.sqrMagnitude > 0;
        if (movement.sqrMagnitude > 0f)
        lastDirection = movement.normalized;

        Animate();
        RotateToMouse();
        UpdateCameraPosition();

        if (Input.GetMouseButtonDown(0) && currentWeapon != null)
        {
            currentWeapon.Fire(transform.right);
            UpdateAmmoUI();
            if (currentWeapon.GetCurrentAmmo() <= 0)
            {
                StartCoroutine(ShakeAmmoText());
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentWeapon != null)
        {
            currentWeapon.Reload();
            UpdateAmmoUI();
            StartCoroutine(WaitForReload());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryWeaponObjects.Count >= 1)
        {
            SwitchWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryWeaponObjects.Count >= 2)
        {
            SwitchWeapon(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryWeaponObjects.Count >= 3)
        {
            SwitchWeapon(2);
        }

    }

    void FixedUpdate()
{
    // Если рывок в процессе — отключаем обычное движение
    if (dashController != null && dashController.IsDashing)
        return;

    rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
}


    void Animate()
    {
        animationTimer += Time.deltaTime;

        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0f;
            currentFrame = (currentFrame + 1) % (isMoving ? runSprites.Length : idleSprites.Length);
            spriteRenderer.sprite = isMoving ? runSprites[currentFrame] : idleSprites[currentFrame];
        }
    }

    public void TakeDamage(int damage)
{
    if (isDead) return; // (строка 83)

    // 1) Если щит активен — ломаем его и НЕ даём урон игроку
    var shield = GetComponentInChildren<ShieldController>();
    if (shield != null && shield.IsShieldActive)
    {
        shield.BreakShield(); // ломаем щит вместо урона
        return;
    }

    // 2) Обычный урон
    if (PlayerDataManager.Instance.SpendHealth(damage))
    {
        Debug.Log($"Игрок получил {damage} урона. Текущее здоровье: {PlayerDataManager.Instance.GetHealth()}");
    }

    if (PlayerDataManager.Instance.GetHealth() <= 0)
    {
        StartCoroutine(PlayDeathAnimation());
    }
}

    
    IEnumerator PlayDeathAnimation()
    {
        isDead = true; // Устанавливаем флаг смерти
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        foreach (Sprite frame in deathSprites)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(deathAnimationSpeed);
        }

        ShowGameOverPanel();
        gameObject.SetActive(false); // Деактивируем игрока после анимации
        // Или можно использовать Destroy(gameObject), если игрок не нужен до перезапуска сцены
    }

    void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.localScale = Vector3.zero;
            LeanTween.scale(gameOverPanel, Vector3.one, 0.5f).setEaseOutBounce();
        }
    }

    void RotateToMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.deltaTime);
    }

    void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            Vector3 shakeOffset = ShakeCamera();
            cameraTransform.position = transform.position + cameraOffset + shakeOffset;
        }
    }

    Vector3 ShakeCamera()
    {
        float xShake = (Mathf.PerlinNoise(Time.time * shakeSpeed, shakeTimeOffsetX) - 0.5f) * shakeIntensity;
        float yShake = (Mathf.PerlinNoise(Time.time * shakeSpeed, shakeTimeOffsetY) - 0.5f) * shakeIntensity;
        return new Vector3(xShake, yShake, 0);
    }

    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= inventoryWeaponObjects.Count)
            return;

        if (currentWeaponObject != null)
            currentWeaponObject.SetActive(false);

        currentWeaponObject = inventoryWeaponObjects[index];
        currentWeaponObject.SetActive(true);
        currentWeapon = currentWeaponObject.GetComponent<Weapon>();
        currentWeaponIndex = index;

        PlayerDataManager.Instance.SetCurrentWeaponIndex(index);

        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.HighlightActiveWeapon(index);
        }

        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null || currentWeapon == null)
            return;

        if (currentWeapon.IsReloading())
        {
            ammoText.text = "//reloading.exe";
        }
        else
        {
            ammoText.text = $"{currentWeapon.GetCurrentAmmo()} / {currentWeapon.GetTotalAmmo()}";
        }
    }

    IEnumerator ShakeAmmoText()
    {
        if (ammoText == null)
            yield break;

        Vector3 originalPos = ammoText.transform.localPosition;
        Color originalColor = ammoText.color;
        ammoText.color = Color.red;

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float offsetX = Random.Range(-5f, 5f);
            float offsetY = Random.Range(-5f, 5f);
            ammoText.transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ammoText.transform.localPosition = originalPos;
        ammoText.color = originalColor;
    }

    IEnumerator WaitForReload()
    {
        if (currentWeapon == null)
            yield break;

        while (currentWeapon.IsReloading())
        {
            yield return null;
        }

        UpdateAmmoUI();
    }
}