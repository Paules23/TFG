// CameraShake.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    [Tooltip("Duration of the shake in seconds")]
    public float shakeDuration = 0.05f;
    [Tooltip("Magnitude of the shake displacement")]
    public float shakeMagnitude = 0.05f;

    private Coroutine shakeCoroutine;
    private Vector3 originalPos;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Inicia un shake con los par�metros del Inspector.
    /// </summary>
    public void Shake()
    {
        // Si ya est� temblando, reiniciamos
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        // Guardamos posici�n actual como base
        originalPos = transform.position;
        shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 randPoint = Random.insideUnitCircle * magnitude;
            transform.position = originalPos + new Vector3(randPoint.x, randPoint.y, 0f);
            yield return null;
        }

        // Restaurar posici�n original
        transform.position = originalPos;
        shakeCoroutine = null;
    }

    /// <summary>
    /// Detiene cualquier shake en curso y restaura posici�n.
    /// Solo act�a si realmente hay un shake activo.
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine == null) return;

        StopCoroutine(shakeCoroutine);
        transform.position = originalPos;
        shakeCoroutine = null;
    }
}
