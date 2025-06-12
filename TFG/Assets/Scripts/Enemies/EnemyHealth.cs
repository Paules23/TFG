using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    public float flashDuration = 0.15f; // Duración del cambio de color

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        Debug.Log($"Enemigo recibió {dmg} de daño. Vida restante: {currentHealth}");

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0f)
            Die();
    }

    private IEnumerator FlashDamage()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log("Enemigo eliminado.");
        Destroy(gameObject);
    }
}
