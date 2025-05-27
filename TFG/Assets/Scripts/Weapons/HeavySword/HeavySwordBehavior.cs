using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavySwordBehaviour : MonoBehaviour
{
    public float attackDuration = 0.4f;
    public float heavyDamageAmount = 50f;
    public LayerMask hittableLayers;
    public float hitboxRadius = 0.5f;
    public Transform hitboxPoint; // Posición desde donde se hará el OverlapCircle

    private bool isAttacking = false;
    private Vector2 attackDirection = Vector2.right;
    private HashSet<Collider2D> alreadyHitTargets = new HashSet<Collider2D>();

    public void Attack(bool facingRight)
    {
        if (!isAttacking)
        {
            attackDirection = facingRight ? Vector2.right : Vector2.left;
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        alreadyHitTargets.Clear(); // 👈 Limpiamos objetivos anteriores

        float elapsed = 0f;
        float startAngle = attackDirection == Vector2.right ? 0f : 0f;
        float endAngle = attackDirection == Vector2.right ? -180f : -180f;

        while (elapsed < attackDuration)
        {
            float t = elapsed / attackDuration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            DetectHits(); // 💥 Detectar colisiones en cada frame
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = Quaternion.identity;
        isAttacking = false;
    }

    private void DetectHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitboxPoint.position, hitboxRadius, hittableLayers);
        foreach (var hit in hits)
        {
            if (!alreadyHitTargets.Contains(hit))
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(heavyDamageAmount);
                    alreadyHitTargets.Add(hit); // ✅ Añadir al set para no repetir
                }
            }
        }
    }

    // Solo para visual debugging en el editor
    private void OnDrawGizmosSelected()
    {
        if (hitboxPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hitboxPoint.position, hitboxRadius);
        }
    }
}
