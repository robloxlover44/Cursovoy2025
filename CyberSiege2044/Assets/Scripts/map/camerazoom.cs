using UnityEngine;
using Unity.Cinemachine;

public class CinemachineOrthoZoomTrigger : MonoBehaviour
{
    public CinemachineCamera cineCamera;
    public float newOrthoSize = 10f;
    public float lerpSpeed = 2f;

    void Start()
    {
        if (cineCamera == null)
        {
            Debug.LogError("Cinemachine Camera не назначена!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(ZoomTo(newOrthoSize));
        }
    }

    // OnTriggerExit2D можно не добавлять — обзор не возвращается!

    System.Collections.IEnumerator ZoomTo(float targetSize)
    {
        float startSize = cineCamera.Lens.OrthographicSize;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * lerpSpeed;
            cineCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
        cineCamera.Lens.OrthographicSize = targetSize;
    }
}
