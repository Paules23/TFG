// CameraBoundsController.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Mueve la c�mara en saltos discretos cuando el jugador toca los l�mites definidos,
/// dejando al jugador en el primer tercio o �ltimo tercio de la pantalla, y mantiene la c�mara fija hasta el siguiente l�mite.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraBoundsController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private Camera cam;

    [Header("Bounds Settings")]
    [Tooltip("Viewport X threshold to trigger right movement (0 to 1)")]
    [Range(0f, 1f)] public float rightThreshold = 0.66f;
    [Tooltip("Viewport X threshold to trigger left movement (0 to 1)")]
    [Range(0f, 1f)] public float leftThreshold = 0.33f;
    [Tooltip("Speed for camera Lerp movement between positions")]
    public float moveSpeed = 5f;

    private float halfWidth;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("CameraBoundsController est� dise�ado para c�mara ortogr�fica.");
        halfWidth = cam.orthographicSize * cam.aspect;
        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (player == null || isMoving) return;

        // Calcula posici�n del jugador en viewport
        Vector3 viewportPos = cam.WorldToViewportPoint(player.position);

        if (viewportPos.x > rightThreshold)
        {
            // Saltar c�mara a la derecha: jugador quedar� en leftThreshold
            float worldOffset = (viewportPos.x - leftThreshold) * 2f * halfWidth;
            targetPosition = transform.position + new Vector3(worldOffset, 0f, 0f);
            StartCoroutine(MoveCamera());
        }
        else if (viewportPos.x < leftThreshold)
        {
            // Saltar c�mara a la izquierda: jugador quedar� en rightThreshold
            float worldOffset = (viewportPos.x - rightThreshold) * 2f * halfWidth;
            targetPosition = transform.position + new Vector3(worldOffset, 0f, 0f);
            StartCoroutine(MoveCamera());
        }
    }

    private IEnumerator MoveCamera()
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        float duration = Mathf.Abs(targetPosition.x - startPos.x) / (moveSpeed * halfWidth * 2f);
        duration = Mathf.Max(0.1f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPosition, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
