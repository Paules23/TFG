using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearBehaviour : MonoBehaviour
{
    [Header("Parámetros de ataque")]
    public float attackDuration = 0.4f;
    [Tooltip("Fracción del tiempo total dedicada al retroceso + retorno")]
    public float recoilFraction = 0.3f;
    public float spearThrustDistance = 1f;
    public float spearRecoilDistance = 0.3f;
    public float spearDamage = 40f;

    [Header("Hitbox")]
    public Transform hitboxPoint;
    public float hitboxRadius = 0.4f;
    public LayerMask hittableLayers;

    private bool isAttacking = false;
    private bool queuedAttack = false;
    private Vector2 attackDirection = Vector2.right;
    private HashSet<Collider2D> alreadyHitTargets = new HashSet<Collider2D>();
    private Vector3 originalLocalPos;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    /// <summary>
    /// Inicia el ataque: si ya está en curso, encola máximo una vez.
    /// facingRight = true si el jugador mira a la derecha; false si mira a la izquierda.
    /// </summary>
    public void Attack(bool facingRight)
    {
        if (!isAttacking)
        {
            attackDirection = facingRight ? Vector2.right : Vector2.left;
            StartCoroutine(AttackCoroutine());
        }
        else if (!queuedAttack)
        {
            // Solo permitimos un ataque en cola
            queuedAttack = true;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        queuedAttack = false;
        alreadyHitTargets.Clear();

        // Movimiento siempre hacia adelante en espacio local
        Vector3 localRecoilOffset = new Vector3(-spearRecoilDistance, 0f, 0f);
        Vector3 localThrustOffset = new Vector3(spearThrustDistance, 0f, 0f);


        // División de tiempos
        float halfRecoil = attackDuration * recoilFraction * 0.5f;
        float thrustTime = attackDuration * (1f - recoilFraction);
        float halfReturn = attackDuration * recoilFraction * 0.5f;

        // Posición local inicial (ya adaptada a la dirección)
        Vector3 startPos = originalLocalPos;

        // 1) Retroceso
        float t = 0f;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / halfRecoil;
            transform.localPosition = Vector3.Lerp(startPos, startPos + localRecoilOffset, t);
            yield return null;
        }

        // 2) Empuje
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / thrustTime;
            transform.localPosition = Vector3.Lerp(startPos + localRecoilOffset, startPos + localThrustOffset, t);
            DetectHits();
            yield return null;
        }

        // 3) Regreso suave
        t = 0f;
        Vector3 returnStart = transform.localPosition;
        while (t < 1f)
        {
            t += Time.deltaTime / halfReturn;
            transform.localPosition = Vector3.Lerp(returnStart, startPos, t);
            yield return null;
        }

        transform.localPosition = originalLocalPos;

        // Si se encoló otro ataque, lo iniciamos
        if (queuedAttack)
        {
            queuedAttack = false;
            StartCoroutine(AttackCoroutine());
        }
        else
        {
            isAttacking = false;
        }
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
                    damageable.TakeDamage(spearDamage);
                    alreadyHitTargets.Add(hit);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hitboxPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hitboxPoint.position, hitboxRadius);
        }
    }
}
