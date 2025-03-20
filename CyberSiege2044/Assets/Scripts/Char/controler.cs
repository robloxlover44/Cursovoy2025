using UnityEngine;
using UnityEngine.UI; // Если решишь использовать UI для отображения патронов

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
    private Rigidbody2D rb;
    private Camera mainCamera;
    private float animationTimer;
    private int currentFrame;
    private bool isMoving;

    // Параметры для покачивания камеры
    public float shakeIntensity = 0.2f; 
    public float shakeSpeed = 5f;
    private float shakeTimeOffsetX;
    private float shakeTimeOffsetY;

    // Ссылка на оружие
    public Weapon currentWeapon;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (cameraTransform != null && cameraOffset == Vector3.zero)
        {
            cameraOffset = cameraTransform.position - transform.position;
        }

        shakeTimeOffsetX = Random.value * 10f;
        shakeTimeOffsetY = Random.value * 10f;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        isMoving = movement.sqrMagnitude > 0;

        Animate();
        RotateToMouse();
        UpdateCameraPosition();

        // Стрельба теперь управляется через Weapon.
        // Если хочешь стрелять по направлению взгляда игрока (например, transform.right), можешь раскомментировать:
         if (Input.GetMouseButtonDown(0) && currentWeapon != null)
         {
             currentWeapon.Fire(transform.right);
         }
        
        

        // Перезарядка по нажатию R
        if (Input.GetKeyDown(KeyCode.R) && currentWeapon != null)
        {
            currentWeapon.Reload();
        }
    }

    void FixedUpdate()
    {
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
}
