using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinimapPlayerIcon : MonoBehaviour {
    [Header("Ссылки")]
    public Camera minimapCam;         // твоя MinimapCam
    public RectTransform mapRect;     // RectTransform зелёного квадрата (MinimapMask)
    public RectTransform iconRect;    // RectTransform MinimapPlayerIcon
    public Transform player;          // твой Player.transform

    [Header("Мигание")]
    public float blinkInterval = 0.5f;

    private Image iconImage;

    void Start() {
        iconImage = iconRect.GetComponent<Image>();
        StartCoroutine(Blink());
    }

    void Update() {
        if (player == null || minimapCam == null) return;

        // переводим мир->вьюпорт (0–1,0–1)
        Vector3 vp = minimapCam.WorldToViewportPoint(player.position);

        // если игрок за границами миникарты — скрываем точку
        bool onMap = vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1 && vp.z > 0;
        iconRect.gameObject.SetActive(onMap);
        if (!onMap) return;

        // смещаем точку в локальные координаты mapRect
        float x = (vp.x - 0.5f) * mapRect.rect.width;
        float y = (vp.y - 0.5f) * mapRect.rect.height;
        iconRect.anchoredPosition = new Vector2(x, y);
    }

    IEnumerator Blink() {
        while (true) {
            iconImage.enabled = !iconImage.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
