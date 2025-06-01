// DestructibleObject.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float health = 30f;

    [Header("Fragment Settings")]
    public GameObject lootFragmentPrefab;
    public int fragmentCount = 10;
    public float fragmentSpreadAngle = -60f;
    public float fragmentMinForce = 0.5f;
    public float fragmentMaxForce = 2.0f;
    public float fragmentGravityScale = 1.5f;
    public float objectScaleBeforeDie = 1.5f;

    [Header("Flash Settings")]
    [Tooltip("Duration of each half of the flash (grow then shrink)")]
    public float flashDuration = 0.1f;

    private bool isDying = false;

    public void TakeDamage(float amount)
    {
        if (isDying) return;
        health -= amount;
        StartCoroutine(SmoothFlashEffect());

        if (health <= 0f)
        {
            isDying = true;
            StartCoroutine(ExplodeAfterFlash());
        }
    }

    private IEnumerator SmoothFlashEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * objectScaleBeforeDie;
        Color originalColor = sr.color;

        float elapsed = 0f;

        // Grow and flash to white
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / flashDuration);
            sr.color = Color.Lerp(originalColor, Color.white, t);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Shrink back and restore color
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / flashDuration);
            sr.color = Color.Lerp(Color.white, originalColor, t);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
    }

    private IEnumerator ExplodeAfterFlash()
    {
        // Trigger camera shake immediately before flash animation
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake();
        }

        // Play smooth flash effect
        yield return SmoothFlashEffect();

        // Spawn fragments and destroy without delay
        SpawnFragments();
        Destroy(gameObject);
    }

    private void SpawnFragments()
    {
        SpriteRenderer sourceRenderer = GetComponent<SpriteRenderer>();
        Color fragmentColor = sourceRenderer != null ? sourceRenderer.color : Color.white;
        Bounds bounds = sourceRenderer.bounds;

        for (int i = 0; i < fragmentCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                transform.position.z
            );

            GameObject fragment = Instantiate(lootFragmentPrefab, spawnPos, Quaternion.identity);

            SpriteRenderer fragRenderer = fragment.GetComponent<SpriteRenderer>();
            if (fragRenderer != null)
                fragRenderer.color = fragmentColor;

            fragment.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            Rigidbody2D rb = fragment.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = fragmentGravityScale;
                rb.AddForce(Random.insideUnitCircle.normalized * Random.Range(fragmentMinForce, fragmentMaxForce), ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-150f, 150f));
            }
        }
    }
}
