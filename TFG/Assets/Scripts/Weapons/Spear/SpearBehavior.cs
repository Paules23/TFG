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

    [Header("Rectangular Hitbox")]
    public Transform hitboxPoint;      // Centro de la zona de impacto
    public Vector2 hitboxSize = new Vector2(2f, 0.4f);
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

        // Movimiento siempre hacia adelante en espacio local:
        // si el objeto padre está volteado, estos offsets se invertirán automáticamente
        Vector3 localRecoilOffset = new Vector3(-spearRecoilDistance, 0f, 0f);
        Vector3 localThrustOffset = new Vector3(spearThrustDistance, 0f, 0f);

        float halfRecoil = attackDuration * recoilFraction * 0.5f;
        float thrustTime = attackDuration * (1f - recoilFraction);
        float halfReturn = attackDuration * recoilFraction * 0.5f;

        Vector3 startPos = originalLocalPos;

        // 1) Retroceso
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / halfRecoil;
            transform.localPosition = Vector3.Lerp(startPos, startPos + localRecoilOffset, t);
            yield return null;
        }

        // 2) Empuje hacia adelante
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
        // Usamos OverlapBoxAll en lugar de OverlapCircleAll:
        // el rectángulo se rota según la rotación global de la lanza
        float currentAngle = transform.eulerAngles.z;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitboxPoint.position,
            hitboxSize,
            currentAngle,
            hittableLayers
        );

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
            float currentAngle = Application.isPlaying
                ? transform.eulerAngles.z
                : transform.localRotation.eulerAngles.z;

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                hitboxPoint.position,
                Quaternion.Euler(0f, 0f, currentAngle),
                Vector3.one
            );
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
