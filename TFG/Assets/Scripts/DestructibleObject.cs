using UnityEngine;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    public float health = 30f;
    public GameObject lootPrefab; // Puedes poner aquí pociones, materiales, etc.

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (lootPrefab != null)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
