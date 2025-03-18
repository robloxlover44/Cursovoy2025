using UnityEngine;
using System.Collections;

public class ChaseEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Exploding }

    [Header("Trigger Settings")]
    public Collider2D triggerZone;

    [Header("Movement Settings")]
    public float chaseSpeed = 4f;
    public float stoppingDistance = 0.1f;
    
    [Header("Damage Settings")]
    public int damage = 10; // Урон, наносимый игроку при столкновении

    [Header("Animation Settings")]
    public Sprite[] runAnimation;
    public Sprite[] explodeAnimation;
    public float frameRate = 0.15f;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private EnemyState currentState = EnemyState.Idle;
    private Coroutine animationCoroutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();

        triggerZone.isTrigger = true;
    }

    void Update()
    {
        if (currentState == EnemyState.Chasing && player != null)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        if (Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * chaseSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == EnemyState.Idle)
        {
            currentState = EnemyState.Chasing;
            StartAnimation(runAnimation);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentState != EnemyState.Exploding)
        {
            // При столкновении отнимаем здоровье у игрока, используя данные из PlayerData
            PlayerDataManager.Instance.SpendHealth(damage);

            currentState = EnemyState.Exploding;
            StopAllCoroutines();
            StartCoroutine(PlayExplosion());
        }
    }

    void StartAnimation(Sprite[] frames)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateSprite(frames));
    }

    IEnumerator AnimateSprite(Sprite[] frames)
    {
        int index = 0;
        while (true)
        {
            spriteRenderer.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }

    IEnumerator PlayExplosion()
    {
        GetComponent<Collider2D>().enabled = false;
        chaseSpeed = 0;

        foreach (Sprite frame in explodeAnimation)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }

        Destroy(gameObject);
    }
}
