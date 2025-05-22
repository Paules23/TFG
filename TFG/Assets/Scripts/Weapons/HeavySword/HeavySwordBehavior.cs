using System.Collections;
using UnityEngine;

public class HeavySwordBehaviour : MonoBehaviour
{
    public float attackDuration = 0.4f;
    public float heavyDamageAmount = 50f;
    public LayerMask hittableLayers;
    public float hitboxRadius = 0.5f;

    private bool isAttacking = false;
    private Vector2 attackDirection = Vector2.right;

    public void Attack(bool facingRight)
    {
        if (!isAttacking)
        {
            // La dirección cambia la rotación (180° izquierda, 0° derecha)
            attackDirection = facingRight ? Vector2.right : Vector2.left;
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        float elapsed = 0f;
        float startAngle = attackDirection == Vector2.right ? -0f : -0f;
        float endAngle = attackDirection == Vector2.right ? -180f : -180f;

        while (elapsed < attackDuration)
        {
            float t = elapsed / attackDuration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = Quaternion.identity; // Reset después del ataque
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking) return;

        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(heavyDamageAmount);
        }
    }
}
