using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBoundsController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private Camera cam;

    [Header("Thresholds (as fraction of screen)")]
    [Range(0f, 1f)] public float leftThreshold = 0.33f;
    [Range(0f, 1f)] public float rightThreshold = 0.66f;
    [Range(0f, 1f)] public float bottomThreshold = 0.33f;
    [Range(0f, 1f)] public float topThreshold = 0.66f;

    [Header("Lerp Settings")]
    [Tooltip("Velocidad de desplazamiento de cámara")]
    public float moveSpeed = 5f;

    private float halfWidth;
    private float halfHeight;
    private float screenWidth;   // = 2 * halfWidth
    private float screenHeight;  // = 2 * halfHeight

    private Vector3 originPos;   // posición inicial fija de la cámara
    private int camXIndex = 0;   // índice horizontal de “pantalla”
    private int camYIndex = 0;   // índice vertical de “pantalla”

    private bool isMoving = false;
    private Vector3 targetPosition;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("CameraBoundsController está diseñado para cámara ortográfica.");

        halfHeight = cam.orthographicSize;
        halfWidth = cam.orthographicSize * cam.aspect;
        screenWidth = halfWidth * 2f;
        screenHeight = halfHeight * 2f;

        originPos = transform.position;   // fijamos la posición “0,0” de la grilla
        camXIndex = 0;
        camYIndex = 0;
        targetPosition = originPos;
    }

    void LateUpdate()
    {
        if (player == null || isMoving) return;

        // Calcula coordenadas relativas del jugador respecto al origen fijo
        Vector3 playerWorld = player.position;
        float relX = playerWorld.x - originPos.x;
        float relY = playerWorld.y - originPos.y;

        bool moved = false;
        int newXIndex = camXIndex;
        int newYIndex = camYIndex;

        // Si cruza el umbral derecho:
        float rightWorldBorder = (camXIndex + rightThreshold - 0.5f) * screenWidth;
        float leftWorldBorder = (camXIndex + leftThreshold - 0.5f) * screenWidth;
        if (relX > rightWorldBorder)
        {
            newXIndex = camXIndex + 1;
            moved = true;
        }
        else if (relX < leftWorldBorder)
        {
            newXIndex = camXIndex - 1;
            moved = true;
        }

        // Si cruza el umbral superior:
        float topWorldBorder = (camYIndex + topThreshold - 0.5f) * screenHeight;
        float bottomWorldBorder = (camYIndex + bottomThreshold - 0.5f) * screenHeight;
        if (relY > topWorldBorder)
        {
            newYIndex = camYIndex + 1;
            moved = true;
        }
        else if (relY < bottomWorldBorder)
        {
            newYIndex = camYIndex - 1;
            moved = true;
        }

        if (moved)
        {
            camXIndex = newXIndex;
            camYIndex = newYIndex;
            // Nueva posición = origin + (índices * tamaño de pantalla)
            targetPosition = new Vector3(
                originPos.x + camXIndex * screenWidth,
                originPos.y + camYIndex * screenHeight,
                originPos.z
            );
            StartCoroutine(MoveCamera());
        }
    }

    private IEnumerator MoveCamera()
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPosition);
        // Normalizamos duración con referencia a screenWidth/screenHeight
        float refSize = Mathf.Max(screenWidth, screenHeight);
        float duration = distance / (moveSpeed * refSize);
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
