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

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Inicia un shake con los parámetros del Inspector.
    /// </summary>
    public void Shake()
    {
        StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        Vector3 originalPos = transform.position;  // Base dynamic position

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 randPoint = Random.insideUnitCircle * magnitude;
            transform.position = originalPos + new Vector3(randPoint.x, randPoint.y, 0f);
            yield return null;
        }

        // Restaurar posición original para que no afecte otros scripts
        transform.position = originalPos;
    }
}
