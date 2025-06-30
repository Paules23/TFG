using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StretchAndSquash : MonoBehaviour
{
    [Header("Stretch & Squash Settings")]
    public float squashAmount = 1.2f;
    public float stretchAmount = 1.2f;
    public float phaseDuration = 0.1f;
    public float returnDuration = 0.2f;

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

        effectCoroutine = StartCoroutine(SquashAndStretchSequence());
    }

    private IEnumerator SquashAndStretchSequence()
    {
        // Fase 1: Squash (X +, Y -)
        Vector3 squashScale = new Vector3(
            originalScale.x * squashAmount,
            originalScale.y / squashAmount,
            originalScale.z
        );
        yield return ScaleOverTime(squashScale, phaseDuration);

        // Fase 2: Stretch (X -, Y +)
        Vector3 stretchScale = new Vector3(
            originalScale.x / stretchAmount,
            originalScale.y * stretchAmount,
            originalScale.z
        );
        yield return ScaleOverTime(stretchScale, phaseDuration);

        // Fase 3: Volver a escala original
        yield return ScaleOverTime(originalScale, returnDuration);

        effectCoroutine = null;
    }

    private IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
