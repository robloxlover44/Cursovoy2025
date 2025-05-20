using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour {
    [Tooltip("Твой Player GameObject")]
    public Transform target;
    void LateUpdate() {
        if (target == null) return;
        // Фиксируем Z (глубину), обновляем X и Y по игроку
        Vector3 pos = transform.position;
        pos.x = target.position.x;
        pos.y = target.position.y;
        transform.position = pos;
    }
}
