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
    private Quaternion defaultRotation;

    [Header("Swing Acceleration and angle")]
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float startAngle ;
    public float endAngle;

    void Start()
    {
        defaultRotation = transform.localRotation;
    }

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
        alreadyHitTargets.Clear();

        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            float t = elapsed / attackDuration;
            float curveT = swingCurve.Evaluate(t);

            // Interpolamos desde 0 al ángulo deseado (positivo o negativo según dirección)
            float angle = Mathf.Lerp(startAngle, endAngle, curveT);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            DetectHits();

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restablecer rotación al final del golpe
        transform.localRotation = defaultRotation;
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
