﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class FlyingShooter : MonoBehaviour, IDamageable
{
    public enum FlyState { Idle, Approaching, Attacking }

    [Header("Stats")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("Ranges")]
    public float detectionRange = 10f;
    public Vector2 closeRangeBoxSize = new Vector2(4f, 4f);
    public Vector3 closeRangeBoxCenterOffset = new Vector3(0f, 1f, 0f);

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float moveInterval = 2f;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float attackInterval = 1f;

    [Header("Flash on Hit")]
    public Color hitFlashColor = new Color(0.5f, 0f, 0.5f);
    public float hitFlashDuration = 0.15f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color idleColor = Color.green;
    public Color approachColor = Color.yellow;
    public Color attackColor = Color.red;

    [Tooltip("Prefab del cuerpo del enemigo al morir")]
    public GameObject DeadBodyPrefab;

    private FlyState state = FlyState.Idle;
    private Transform player;
    private float moveTimer;
    private float attackTimer;
    private Vector3 randomTarget;

    private HitScaleEffect hitEffect;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("FlyingShooter: no se encontró ningún GameObject con tag 'Player'.");

        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        hitEffect = GetComponent<HitScaleEffect>();

        UpdateColor();
    }

    void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case FlyState.Idle:
                if (distToPlayer <= detectionRange)
                {
                    state = FlyState.Approaching;
                    Debug.Log("[FlyingShooter] Idle→Approaching");
                    UpdateColor();
                }
                break;

            case FlyState.Approaching:
                MoveTowards(player.position);
                if (IsInsideCloseBox(transform.position))
                {
                    Debug.Log("[FlyingShooter] Approaching→Attacking");
                    EnterAttacking();
                }
                break;

            case FlyState.Attacking:
                if (!IsInsideCloseBox(transform.position))
                {
                    state = FlyState.Approaching;
                    Debug.Log("[FlyingShooter] Attacking→Approaching (salió del close box)");
                    UpdateColor();
                    break;
                }

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    ShootAtPlayer();
                    attackTimer = 0f;
                }

                moveTimer += Time.deltaTime;
                if (moveTimer >= moveInterval)
                {
                    ChooseNewRandomTarget();
                    moveTimer = 0f;
                }
                MoveTowards(randomTarget);
                break;
        }
    }

    void EnterAttacking()
    {
        state = FlyState.Attacking;
        attackTimer = attackInterval;
        moveTimer = moveInterval;
        ChooseNewRandomTarget();
        UpdateColor();
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void ShootAtPlayer()
    {
        if (bulletPrefab == null || shootPoint == null) return;
        Vector3 dir = (player.position - shootPoint.position).normalized;
        GameObject b = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        var bullet = b.GetComponent<EnemyBullet>();
        if (bullet != null)
            bullet.SetDirection(dir);
        Debug.Log("[FlyingShooter] Disparó al jugador");
    }

    void ChooseNewRandomTarget()
    {
        Vector3 center = player.position + closeRangeBoxCenterOffset;
        float halfX = closeRangeBoxSize.x * 0.5f;
        float halfY = closeRangeBoxSize.y * 0.5f;
        float rx = Random.Range(-halfX, halfX);
        float ry = Random.Range(-halfY, halfY);
        randomTarget = center + new Vector3(rx, ry, 0f);
        Debug.Log("[FlyingShooter] Nuevo destino en closeBox: " + randomTarget);
    }

    bool IsInsideCloseBox(Vector3 pos)
    {
        Vector3 center = player.position + closeRangeBoxCenterOffset;
        Vector3 delta = pos - center;
        return Mathf.Abs(delta.x) <= closeRangeBoxSize.x * 0.5f
            && Mathf.Abs(delta.y) <= closeRangeBoxSize.y * 0.5f;
    }

    public void TakeDamage(float dmg)
    {
        if (state == FlyState.Idle)
        {
            Debug.Log("[FlyingShooter] Ignora el daño porque está en estado Idle.");
            return;
        }

        currentHealth -= dmg;
        StartCoroutine(FlashHit());

        // Activar efecto visual de impacto si existe
        if (hitEffect != null)
            hitEffect.PlayEffect();

        if (currentHealth <= 0f)
        {
            Instantiate(DeadBodyPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    IEnumerator FlashHit()
    {
        Color prev = spriteRenderer.color;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        UpdateColor();
    }

    void UpdateColor()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = state switch
        {
            FlyState.Idle => idleColor,
            FlyState.Approaching => approachColor,
            FlyState.Attacking => attackColor,
            _ => spriteRenderer.color
        };
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = player.position + closeRangeBoxCenterOffset;
            Gizmos.DrawWireCube(center, new Vector3(closeRangeBoxSize.x, closeRangeBoxSize.y, 0f));
        }
    }
}
