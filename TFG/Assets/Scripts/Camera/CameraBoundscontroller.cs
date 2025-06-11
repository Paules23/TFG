using UnityEngine;
using System.Collections;

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
    private Transform player;   // Se asignará en Start, no desde el Inspector.
    private float halfWidth, halfHeight;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("CameraBoundsController está diseñado para cámaras ortográficas.");

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        targetPosition = transform.position;
    }

    void Start()
    {
        // Buscamos el Player por su tag en Start para asegurar que ya se haya instanciado.
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
            player = go.transform;
        else
            Debug.LogError("CameraBoundsController: No se encontró ningún GameObject con tag 'Player'.");
    }

    void LateUpdate()
    {
        if (player == null || isMoving)
            return;

        Vector3 vp = cam.WorldToViewportPoint(player.position);
        bool needMove = false;
        Vector3 newCamPos = transform.position;

        // Horizontal
        if (vp.x > rightThreshold)
        {
            newCamPos.x = transform.position.x + 2f * halfWidth;
            needMove = true;
        }
        else if (vp.x < leftThreshold)
        {
            newCamPos.x = transform.position.x - 2f * halfWidth;
            needMove = true;
        }

        // Vertical
        if (vp.y > topThreshold)
        {
            newCamPos.y = transform.position.y + 2f * halfHeight;
            needMove = true;
        }
        else if (vp.y < bottomThreshold)
        {
            newCamPos.y = transform.position.y - 2f * halfHeight;
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
        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, targetPosition);
        float duration = Mathf.Max(0.05f, dist / (moveSpeed * Mathf.Max(halfWidth, halfHeight)));

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / duration);
            transform.position = Vector3.Lerp(start, targetPosition, f);
            yield return null;
        }
        transform.position = targetPosition;
        isMoving = false;
    }
}
