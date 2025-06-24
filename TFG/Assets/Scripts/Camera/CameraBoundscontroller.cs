// CameraBoundsController.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraBoundsController : MonoBehaviour
{
    [Header("Initial Position")]
    public bool useInitialPosition = false;
    public Vector3 initialPosition = Vector3.zero;

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
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        targetPosition = transform.position;
    }

    void Start()
    {
        if (useInitialPosition)
        {
            transform.position = initialPosition;
            targetPosition = initialPosition;
        }

        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;
        else Debug.LogError("CameraBoundsController: no se encontró tag 'Player'.");
    }

    void LateUpdate()
    {
        if (player == null || isMoving) return;

        Vector3 vp = cam.WorldToViewportPoint(player.position);
        bool needMove = false;
        Vector3 newCamP = transform.position;

        if (vp.x > rightThreshold)
        {
            newCamP.x += 2f * halfWidth;
            needMove = true;
        }
        else if (vp.x < leftThreshold)
        {
            newCamP.x -= 2f * halfWidth;
            needMove = true;
        }

        if (vp.y > topThreshold)
        {
            newCamP.y += 2f * halfHeight;
            needMove = true;
        }
        else if (vp.y < bottomThreshold)
        {
            newCamP.y -= 2f * halfHeight;
            needMove = true;
        }

        if (needMove)
        {
            // Antes de mover, detener shake si hubiera
            if (CameraShake.Instance != null)
                CameraShake.Instance.StopShake();

            targetPosition = newCamP;
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
