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
    private Checkpoint lastCheckpoint;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    public float flashDuration = 0.2f;
    private Color originalColor;

    void Awake()
    {
        maxHealth = 10f;
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
        lastCheckpointPosition = defaultSpawnPosition != Vector3.zero ? defaultSpawnPosition : transform.position;

        UpdateHealthColor();
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"Jugador recibió {dmg} de daño. Vida restante: {currentHealth}");

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        UpdateHealthColor();

        if (currentHealth <= 0f)
            RespawnAtCheckpoint();
    }

    private IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        UpdateHealthColor();
    }

    private void RespawnAtCheckpoint()
    {
        Debug.Log("Jugador ha muerto. Respawn en el checkpoint.");
        transform.position = lastCheckpointPosition;
        currentHealth = maxHealth;
        UpdateHealthColor();
    }

    private void UpdateHealthColor()
    {
        if (spriteRenderer == null) return;

        float ratio = currentHealth / maxHealth;
        spriteRenderer.color = Color.Lerp(Color.red, originalColor, ratio);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            lastCheckpointPosition = other.transform.position;
            Debug.Log($"Nuevo checkpoint activado: {lastCheckpointPosition}");

            // Desactivar el checkpoint anterior visualmente
            if (lastCheckpoint != null && lastCheckpoint != other.GetComponent<Checkpoint>())
                lastCheckpoint.SetActive(false);

            // Activar el nuevo checkpoint visualmente
            Checkpoint flag = other.GetComponent<Checkpoint>();
            if (flag != null)
            {
                flag.SetActive(true);
                lastCheckpoint = flag;
            }
        }
    }
}
