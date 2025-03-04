using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites;
    public Sprite[] runSprites;
    public float animationSpeed = 0.1f;
    public Transform cameraTransform; // Ссылка на трансформ камеры
    public Vector3 cameraOffset; // Смещение камеры относительно персонажа

    private Vector2 movement;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private float animationTimer;
    private int currentFrame;
    private bool isMoving;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // Если смещение камеры не задано, вычисляем его
        if (cameraTransform != null && cameraOffset == Vector3.zero)
        {
            cameraOffset = cameraTransform.position - transform.position;
        }
    }

    void Update()
    {
        // Получаем ввод от игрока
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        isMoving = movement.sqrMagnitude > 0;

        // Анимация
        Animate();

        // Поворот персонажа к курсору
        RotateToMouse();

        // Обновляем позицию камеры
        UpdateCameraPosition();
    }

    void FixedUpdate()
    {
        // Перемещение персонажа
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void Animate()
    {
        animationTimer += Time.deltaTime;

        if (isMoving)
        {
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % runSprites.Length;
                spriteRenderer.sprite = runSprites[currentFrame];
            }
        }
        else
        {
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % idleSprites.Length;
                spriteRenderer.sprite = idleSprites[currentFrame];
            }
        }
    }

    void RotateToMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = rb.rotation;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        rb.rotation = newAngle;
    }

    void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            cameraTransform.position = transform.position + cameraOffset;
        }
    }
}