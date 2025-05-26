using UnityEngine;
using TMPro;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public int requiredShards = 1;
    public TMP_Text shardText;
    public GameObject wirePuzzlePanel; // Сюда закинь свой Canvas с WirePuzzle

    public float shakeAmount = 16f;
    public float shakeDuration = 0.23f;
    public Color failColor = Color.red;

    [HideInInspector] public bool isPlayerNear = false;

    private void Start()
    {
        UpdateShardText();
    }

    private void Update()
    {
        if (!isPlayerNear)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int playerShards = PlayerDataManager.Instance.GetShards();
            if (playerShards >= requiredShards)
            {
                // Запуск мини-игры (аналогично сундуку)
                if (wirePuzzlePanel != null)
                {
                    wirePuzzlePanel.SetActive(true);
                    Time.timeScale = 0f;

                    WirePuzzle puzzle = wirePuzzlePanel.GetComponent<WirePuzzle>();
                    if (puzzle != null)
                    {
                        puzzle.targetChest = null;   // Это не сундук!
                        puzzle.doorToOpen = this;    // Передаём себя мини-игре (для открытия)
                    }
                }
            }
            else
            {
                StartCoroutine(ShakeAndColorText());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
            UpdateShardText();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    public void OpenDoor()
    {
        // Тратим шарды
        PlayerDataManager.Instance.SpendShards(requiredShards);

        // Можно добавить анимацию открытия/исчезновения двери!
        Destroy(gameObject);
    }

    private void UpdateShardText()
    {
        if (shardText == null)
            return;

        int playerShards = PlayerDataManager.Instance.GetShards();
        shardText.text = $"{playerShards} / {requiredShards}";
        shardText.color = playerShards >= requiredShards ? Color.white : Color.red;
    }

    private IEnumerator ShakeAndColorText()
    {
        Vector3 origPos = shardText.rectTransform.localPosition;
        Color origColor = shardText.color;
        shardText.color = failColor;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            shardText.rectTransform.localPosition = origPos + new Vector3(x, y, 0);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        shardText.rectTransform.localPosition = origPos;
        shardText.color = origColor;
    }
}