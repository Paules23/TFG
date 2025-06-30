using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class OrbitingBall : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 10f;
    public float touchDamage = 10f;

    [Header("Visual Feedback")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.15f;

    [Header("Layers & Tags")]
    public string playerTag = "Player";

    [Tooltip("Prefab del cuerpo del enemigo al morir")]
    public GameObject DeadBodyPrefab;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0f)
        {
            //instancia enemigo muerto
            Instantiate(DeadBodyPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }           
    }

    private IEnumerator FlashDamage()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            var dmg = other.GetComponent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage(touchDamage);
        }
    }
}
