using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBoundsController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private Camera cam;

    [Header("Thresholds (Viewport)")]
    [Range(0f, 1f)] public float leftThreshold = 0.33f;
    [Range(0f, 1f)] public float rightThreshold = 0.66f;
    [Range(0f, 1f)] public float bottomThreshold = 0.33f;
    [Range(0f, 1f)] public float topThreshold = 0.66f;

    [Header("Lerp Settings")]
    [Tooltip("Velocidad de desplazamiento de cámara")]
    public float moveSpeed = 5f;

    private float halfWidth;
    private float halfHeight;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("CameraBoundsController está diseñado para cámara ortográfica.");

        halfHeight = cam.orthographicSize;
        halfWidth = cam.orthographicSize * cam.aspect;
        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (player == null || isMoving) return;

        // Calcula la posición en viewport (0..1)
        Vector3 vp = cam.WorldToViewportPoint(player.position);

        bool needMove = false;
        Vector3 newCamPos = transform.position;

        // HORIZONTAL
        if (vp.x > rightThreshold)
        {
            // Llevamos al jugador al tercio izquierdo (leftThreshold)
            float desiredCamX = player.position.x - (leftThreshold - 0.5f) * 2f * halfWidth;
            newCamPos.x = desiredCamX;
            needMove = true;
        }
        else if (vp.x < leftThreshold)
        {
            // Llevamos al jugador al tercio derecho (rightThreshold)
            float desiredCamX = player.position.x - (rightThreshold - 0.5f) * 2f * halfWidth;
            newCamPos.x = desiredCamX;
            needMove = true;
        }

        // VERTICAL
        if (vp.y > topThreshold)
        {
            // Llevamos al jugador al tercio inferior (bottomThreshold)
            float desiredCamY = player.position.y - (bottomThreshold - 0.5f) * 2f * halfHeight;
            newCamPos.y = desiredCamY;
            needMove = true;
        }
        else if (vp.y < bottomThreshold)
        {
            // Llevamos al jugador al tercio superior (topThreshold)
            float desiredCamY = player.position.y - (topThreshold - 0.5f) * 2f * halfHeight;
            newCamPos.y = desiredCamY;
            needMove = true;
        }

        if (needMove)
        {
            targetPosition = newCamPos;
            StartCoroutine(MoveCamera());
        }
    }

    private IEnumerator MoveCamera()
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPosition);
        float duration = distance / (moveSpeed * Mathf.Max(halfWidth, halfHeight));
        duration = Mathf.Max(0.05f, duration);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
