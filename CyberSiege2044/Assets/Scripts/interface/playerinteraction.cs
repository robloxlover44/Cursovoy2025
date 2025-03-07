using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;
    private LineRenderer currentLineRenderer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
            Debug.Log("Можно взаимодействовать! Жми E");

            EdgeCollider2D edgeCollider = other.GetComponent<EdgeCollider2D>();
            if (edgeCollider != null)
            {
                currentLineRenderer = other.GetComponent<LineRenderer>();
                if (currentLineRenderer == null)
                {
                    currentLineRenderer = other.gameObject.AddComponent<LineRenderer>();
                }
                ConfigureLineRenderer(currentLineRenderer, edgeCollider, other.transform);
                currentLineRenderer.enabled = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == currentInteractable)
        {
            currentInteractable = null;
            Debug.Log("Выход из зоны взаимодействия");

            if (currentLineRenderer != null)
            {
                currentLineRenderer.enabled = false;
                currentLineRenderer = null;
            }
        }
    }

    private void ConfigureLineRenderer(LineRenderer lineRenderer, EdgeCollider2D edgeCollider, Transform objTransform)
    {
        Vector2[] points2D = edgeCollider.points;
        Vector3[] points3D = new Vector3[points2D.Length];
        for (int i = 0; i < points2D.Length; i++)
        {
            points3D[i] = objTransform.TransformPoint(new Vector3(points2D[i].x, points2D[i].y, 0f));
        }
        lineRenderer.positionCount = points3D.Length;
        lineRenderer.SetPositions(points3D);
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.useWorldSpace = true;
    }
}