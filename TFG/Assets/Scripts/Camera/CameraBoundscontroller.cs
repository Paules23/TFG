using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBoundsController : MonoBehaviour
{
    [Header("Thresholds (Viewport)")]
    [Range(0f, 1f)] public float leftThreshold = 0.33f;
    [Range(0f, 1f)] public float rightThreshold = 0.66f;
    [Range(0f, 1f)] public float bottomThreshold = 0.33f;
    [Range(0f, 1f)] public float topThreshold = 0.66f;

    [Header("Lerp Settings")]
    public float moveSpeed = 5f;

    private Camera cam;
    private Transform player;
    private float halfWidth, halfHeight;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("CameraBoundsController diseñado para cámara ortográfica.");

        // Buscar al Player por tag
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;
        else Debug.LogError("CameraBoundsController: tag 'Player' no encontrado.");

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (player == null || isMoving) return;

        Vector3 vp = cam.WorldToViewportPoint(player.position);
        bool needMove = false;
        Vector3 newCam = transform.position;

        // Horizontal
        if (vp.x > rightThreshold)
        {
            newCam.x = transform.position.x + 2f * halfWidth;
            needMove = true;
        }
        else if (vp.x < leftThreshold)
        {
            newCam.x = transform.position.x - 2f * halfWidth;
            needMove = true;
        }

        // Vertical
        if (vp.y > topThreshold)
        {
            newCam.y = transform.position.y + 2f * halfHeight;
            needMove = true;
        }
        else if (vp.y < bottomThreshold)
        {
            newCam.y = transform.position.y - 2f * halfHeight;
            needMove = true;
        }

        if (needMove)
        {
            targetPosition = newCam;
            StartCoroutine(MoveCamera());
        }
    }

    private IEnumerator MoveCamera()
    {
        isMoving = true;
        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, targetPosition);
        float dur = Mathf.Max(0.05f, dist / (moveSpeed * Mathf.Max(halfWidth, halfHeight)));

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / dur);
            transform.position = Vector3.Lerp(start, targetPosition, f);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
