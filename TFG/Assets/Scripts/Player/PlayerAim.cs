using System.Collections;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Eye Settings")]
    [Tooltip("Transform del ojo (GameObject hijo)")]
    public Transform eyeTransform;
    [Tooltip("SpriteRenderer del ojo para verificar que existe")]
    public SpriteRenderer eyeSprite;

    [Header("Circle Bounds")]
    [Tooltip("Radio del círculo (zona roja) en unidades por donde se mueve el ojo")]
    public float circleRadius = 0.4f;
    [Tooltip("Desplazamiento del centro del círculo en unidades locales (ej. para subir el círculo)")]
    public Vector2 circleCenterOffset = new Vector2(0f, 0.2f);

    [Header("Lower Limit")]
    [Tooltip("Límite inferior (en Y) que el ojo no puede bajar")]
    public float eyeMinY = 0f;

    [Header("Flip Settings")]
    [Tooltip("Velocidad a la que el jugador se da la vuelta (más alto = más rápido)")]
    [Range(0.01f, 10f)]
    public float flipSpeed = 5f;

    [Header("Cursor Settings")]
    [Tooltip("Sprite que se usará como cursor")]
    public Texture2D cursorTexture;
    [Tooltip("Hotspot del cursor, en píxeles (0,0 = esquina superior izquierda)")]
    public Vector2 cursorHotspot = Vector2.zero;

    // Internos
    private Camera mainCamera;
    private bool facingRight = true;

    void Awake()
    {
        mainCamera = Camera.main;

        if (eyeTransform == null)
            Debug.LogError("PlayerAim: Debes asignar el eyeTransform en el Inspector.");
        if (eyeSprite == null)
            Debug.LogError("PlayerAim: Debes asignar el eyeSprite en el Inspector.");

        if (cursorTexture != null)
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    void Update()
    {
        UpdateEyePosition();
        UpdateFlip();
    }

    /// <summary>
    /// Actualiza la posición del ojo para que siga al ratón dentro de un círculo cuyo centro está desplazado.
    /// </summary>
    private void UpdateEyePosition()
    {
        // 1) Obtener la posición del ratón en mundo
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // 2) Convertir la posición del ratón a coordenadas locales del jugador
        Vector3 localMousePos = transform.InverseTransformPoint(mouseWorld);

        // 3) Ajustar el centro del círculo en función del flip
        float flipSign = Mathf.Sign(transform.lossyScale.x);
        Vector2 adjustedCenter = new Vector2(circleCenterOffset.x * flipSign, circleCenterOffset.y);

        // 4) Calcular la posición relativa respecto al centro del círculo
        Vector2 relativePos = (Vector2)localMousePos - adjustedCenter;

        // 5) Asegurar que la Y en el mundo local absoluto no sea menor que eyeMinY
        float absoluteY = adjustedCenter.y + relativePos.y;
        if (absoluteY < eyeMinY)
        {
            relativePos.y += (eyeMinY - absoluteY);
        }

        // 6) Limitar la posición al borde del círculo si se sale de él
        if (relativePos.magnitude > circleRadius)
        {
            relativePos = relativePos.normalized * circleRadius;
        }

        // 7) La posición final del ojo es el centro ajustado más la posición relativa
        Vector2 finalPos = adjustedCenter + relativePos;
        eyeTransform.localPosition = new Vector3(finalPos.x, finalPos.y, 0f);
    }

    /// <summary>
    /// Gira suavemente al jugador (flip) según la posición del ratón en X.
    /// </summary>
    private void UpdateFlip()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        bool shouldFaceRight = mouseWorld.x >= transform.position.x;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            StopAllCoroutines();
            StartCoroutine(SmoothFlip(facingRight));
        }
    }

    /// <summary>
    /// Animación de flip suave entre orientaciones.
    /// </summary>
    private IEnumerator SmoothFlip(bool faceRight)
    {
        float targetScaleX = faceRight ? 1f : -1f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(targetScaleX, startScale.y, startScale.z);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, 1f / flipSpeed);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    /// <summary>
    /// Propiedad para conocer si el jugador está mirando a la derecha.
    /// </summary>
    public bool FacingRight
    {
        get { return facingRight; }
    }

    // Dibujar Gizmos para depuración: se muestra el círculo (en rojo) en su nuevo centro
    private void OnDrawGizmosSelected()
    {
        if (eyeTransform == null)
            return;

        float flipSign = Application.isPlaying ? Mathf.Sign(transform.lossyScale.x) : 1f;
        Vector2 adjustedCenter = new Vector2(circleCenterOffset.x * flipSign, circleCenterOffset.y);
        Vector3 worldCenter = transform.TransformPoint(adjustedCenter);

        // Dibujar el círculo en rojo (zona de movimiento del ojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(worldCenter, circleRadius);

        // Dibujar una línea horizontal en eyeMinY (en azul)
        Gizmos.color = Color.blue;
        Vector3 baselineLeft = transform.TransformPoint(new Vector3(adjustedCenter.x - circleRadius, eyeMinY, 0f));
        Vector3 baselineRight = transform.TransformPoint(new Vector3(adjustedCenter.x + circleRadius, eyeMinY, 0f));
        Gizmos.DrawLine(baselineLeft, baselineRight);
    }
}
