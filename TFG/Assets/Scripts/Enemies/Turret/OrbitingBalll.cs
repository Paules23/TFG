using UnityEngine;

/// Bola individual que gira alrededor del núcleo.
/// Lleva vida propia y daña al Player por contacto.
[RequireComponent(typeof(Collider2D))]
public class OrbitingBall : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 10f;
    public float touchDamage = 10f;

    [Header("Layers & Tags")]
    public string playerTag = "Player";

    private float currentHealth;

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            var dmg = other.GetComponent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(touchDamage);
            // La bola no se destruye; si quieres, coméntalo:
            // Destroy(gameObject);
        }
    }
}
