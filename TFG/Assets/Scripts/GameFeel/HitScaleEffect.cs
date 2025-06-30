using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HitScaleEffect : MonoBehaviour
{
    [Header("Hit Scale Settings")]
    [Tooltip("Cuánto se amplía el sprite en todas las direcciones")]
    public float scaleMultiplier = 1.3f;

    [Tooltip("Duración del efecto de expansión")]
    public float scaleUpDuration = 0.01f;

    [Tooltip("Duración para volver suavemente a la escala original")]
    public float returnDuration = 0.01f;

    private Vector3 originalScale;
    private Coroutine effectCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void PlayEffect()
    {
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(HitEffectSequence());
    }

    private IEnumerator HitEffectSequence()
    {
        Vector3 targetScale = originalScale * scaleMultiplier;

        // Escalar hacia arriba
        yield return ScaleOverTime(targetScale, scaleUpDuration);

        // Volver a la escala normal
        yield return ScaleOverTime(originalScale, returnDuration);

        effectCoroutine = null;
    }

    private IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
