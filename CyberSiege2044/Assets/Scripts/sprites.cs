using UnityEngine;

public class FANS : MonoBehaviour
{
    [Header("Idle Animation Settings")]
    [SerializeField] private Sprite[] idleSprites;  // ������ �������� ��� �������� Idle
    [SerializeField] private float animationSpeed = 0.1f;  // �������� ��������

    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    private int currentFrame;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        AnimateIdle();
    }

   private void AnimateIdle()
{
    if (idleSprites == null || idleSprites.Length == 0)
        return;

    animationTimer += Time.unscaledDeltaTime; // ���������� unscaledDeltaTime
    if (animationTimer >= animationSpeed)
    {
        animationTimer = 0f;
        currentFrame = (currentFrame + 1) % idleSprites.Length;
        spriteRenderer.sprite = idleSprites[currentFrame];
    }
}
}
