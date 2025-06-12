using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Checkpoints")]
    private Vector3 lastCheckpointPosition;
    public Vector3 defaultSpawnPosition;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    public float flashDuration = 0.2f;
    private Color originalColor; // Se guarda una sola vez y nunca cambia

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Guardamos el color base una vez y nunca se modifica
        originalColor = spriteRenderer.color;

        lastCheckpointPosition = defaultSpawnPosition != Vector3.zero ? defaultSpawnPosition : transform.position;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        Debug.Log($"Jugador recibió {dmg} de daño. Vida restante: {currentHealth}");

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0f)
            RespawnAtCheckpoint();
    }

    private IEnumerator FlashDamage()
    {
        // Siempre restauramos el color original al final
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Reinicia la vida y teletransporta al jugador al último checkpoint registrado.
    /// Ahora también **restaura su color original** para evitar que se quede rojo.
    /// </summary>
    private void RespawnAtCheckpoint()
    {
        Debug.Log("Jugador ha muerto. Respawn en el checkpoint.");
        transform.position = lastCheckpointPosition;
        currentHealth = maxHealth;

        // Siempre restaura el color original al revivir
        spriteRenderer.color = originalColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            lastCheckpointPosition = other.transform.position;
            Debug.Log($"Nuevo checkpoint activado: {lastCheckpointPosition}");
        }
    }
}
