using System.Collections;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Eye Settings")]
    [Tooltip("Transform del ojo (GameObject hijo)")]
    public Transform eyeTransform;
    [Tooltip("SpriteRenderer del ojo para asegurarnos de que existe")]
    public SpriteRenderer eyeSprite;

    [Header("Ellipse Bounds (Upper half of head)")]
    [Tooltip("Radio horizontal completo de la elipse (a) en unidades")]
    public float ellipseRadiusX = 0.4f;
    [Tooltip("Altura total de la elipse (2b) en unidades")]
    public float ellipseRadiusY = 0.6f;

    [Header("Dead Zone & Lower Limit")]
    [Tooltip("Anchura de la Dead Zone central (± en X) donde el ojo siempre se queda en eyeMinY")]
    public float centerDeadZoneX = 0.1f;
    [Tooltip("Límite inferior (en Y) donde no puede bajar el ojo")]
    public float eyeMinY = 0.0f;

    [Header("Flip Settings")]
    [Tooltip("Velocidad a la que el jugador se da la vuelta (un valor mayor = más rápido)")]
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
            Debug.LogError("PlayerAim: Debes asignar eyeTransform en el Inspector.");
        if (eyeSprite == null)
            Debug.LogError("PlayerAim: Debes asignar eyeSprite (SpriteRenderer) en el Inspector.");

        if (cursorTexture != null)
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    void Update()
    {
        UpdateEyePosition();
        UpdateFlip();
    }

    /// <summary>
    /// Mueve el ojo para que siga al ratón, con estas reglas:
    /// - Nunca baja de eyeMinY.
    /// - Si |x| < centerDeadZoneX → y = eyeMinY (Dead Zone).
    /// - Si está por encima de eyeMinY y fuera de Dead Zone, se ubica dentro de la media-elipse superior.
    /// </summary>
    private void UpdateEyePosition()
    {
        // 1) Tomamos la posición del ratón en mundo
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // 2) Convertimos a coordenadas locales del jugador
        Vector3 localMousePos = transform.InverseTransformPoint(mouseWorld);

        // 3) Datos de la elipse:
        float a = ellipseRadiusX;       // radio X
        float b = ellipseRadiusY * 0.5f; // radio Y (mitad)
        float centerY = b;               // centro Y de la elipse (la media-elipse va de y=0 a y=2b)

        // 4) Si la X cae en la Dead Zone central → el ojo se queda en y = eyeMinY, y x se limita a ±a
        if (Mathf.Abs(localMousePos.x) < centerDeadZoneX)
        {
            float clampedX = Mathf.Clamp(localMousePos.x, -a, a);
            eyeTransform.localPosition = new Vector3(clampedX, eyeMinY, 0f);
            return;
        }

        // 5) Fuera de Dead Zone:
        //    - Forzamos que y no baje de eyeMinY
        localMousePos.y = Mathf.Max(localMousePos.y, eyeMinY);

        //    - Si está por encima de la elipse (considerando la media-elipse):
        //      la condición de pertenencia a la elipse centrada en (0, centerY) es:
        //      ((x)/a)^2 + ((y-centerY)/b)^2 <= 1
        float relX = localMousePos.x;
        float relY = localMousePos.y - centerY;
        float norm = (relX * relX) / (a * a) + (relY * relY) / (b * b);

        if (norm > 1f)
        {
            // Está fuera de la frontera: proyectamos sobre la media-elipse superior
            float theta = Mathf.Atan2(relY / b, relX / a);
            // Aseguramos que theta ∈ [0, π] para la mitad superior
            if (theta < 0f) theta += Mathf.PI;

            float cos = Mathf.Cos(theta);
            float sin = Mathf.Sin(theta);

            localMousePos.x = a * cos;
            localMousePos.y = centerY + b * sin;
        }

        // 6) Asignamos la posición resultante al ojo
        eyeTransform.localPosition = new Vector3(localMousePos.x, localMousePos.y, 0f);
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
    /// Indica si el jugador mira a la derecha.
    /// </summary>
    public bool FacingRight
    {
        get { return facingRight; }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Dibujar Gizmos: muestra:
    //  - Media-elipse superior (azul)
    //  - Línea y = eyeMinY (rojo)
    //  - Dead Zone central en x ∈ [–centerDeadZoneX, +centerDeadZoneX] (verde)
    // ════════════════════════════════════════════════════════════════════════════
    private void OnDrawGizmosSelected()
    {
        if (eyeTransform == null)
            return;

        // 1) Datos de la elipse
        float a = ellipseRadiusX;
        float b = ellipseRadiusY * 0.5f;
        float centerY = b;
        int segments = 40;

        // (A) Dibujar media-elipse superior en azul
        Gizmos.color = Color.blue;
        Vector3 prevPt = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float theta = Mathf.PI * i / segments; // [0..π]
            float x = a * Mathf.Cos(theta);
            float y = centerY + b * Mathf.Sin(theta);
            Vector3 worldPt = transform.TransformPoint(new Vector3(x, y, 0f));
            if (i > 0)
                Gizmos.DrawLine(prevPt, worldPt);
            prevPt = worldPt;
        }

        // (B) Dibujar línea horizontal y = eyeMinY en rojo
        Gizmos.color = Color.green;
        Vector3 leftLine = transform.TransformPoint(new Vector3(-a, eyeMinY, 0f));
        Vector3 rightLine = transform.TransformPoint(new Vector3(a, eyeMinY, 0f));
        Gizmos.DrawLine(leftLine, rightLine);

        // (C) Dibujar Dead Zone central como dos líneas verticales en verde
        Gizmos.color = Color.green;
        // Intersección de la elipse con x = ±centerDeadZoneX
        float xClamp = centerDeadZoneX;
        float rel = 1f - (xClamp * xClamp) / (a * a);
        rel = Mathf.Max(0f, rel);
        float topY = centerY + b * Mathf.Sqrt(rel);

        Vector3 leftTop = transform.TransformPoint(new Vector3(-xClamp, topY, 0f));
        Vector3 leftBottom = transform.TransformPoint(new Vector3(-xClamp, eyeMinY, 0f));
        Gizmos.DrawLine(leftTop, leftBottom);

        Vector3 rightTop = transform.TransformPoint(new Vector3(xClamp, topY, 0f));
        Vector3 rightBottom = transform.TransformPoint(new Vector3(xClamp, eyeMinY, 0f));
        Gizmos.DrawLine(rightTop, rightBottom);
    }
}
