using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float jumpForce = 25f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Coyote Jump")]
    [Tooltip("Tiempo (en s) que permitimos saltar tras perder contacto con el suelo")]
    public float coyoteTime = 0.1f;
    private float coyoteTimer;

    [Header("Dash Settings")]
    public float dashSpeed = 24f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    [Header("Wall Slide/Fall Fix")]
    [Tooltip("Distancia para el raycast horizontal que detecta muros")]
    public float wallCheckDistance = 0.1f;
    public LayerMask wallLayer;

    // Componentes privados
    private Rigidbody2D rb;
    private Collider2D col;
    private float originalGravityScale;
    private bool isGrounded;
    private bool wasGrounded;

    // Estado de dash
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        originalGravityScale = rb.gravityScale;
        wasGrounded = true;
        coyoteTimer = 0f;
        isDashing = false;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
    }

    void Update()
    {
        // Actualizar timers de dash
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
            return; // Mientras dash, no se ejecutan Move y Jump normales.
        }

        Move();
        CheckJump();
        CheckLanding();
        CheckDashInput();
    }

    void Move()
    {
        float input = Input.GetAxisRaw("Horizontal");

        // Raycast horizontal para detectar muro solo si no estamos en el suelo.
        bool touchingWall = false;
        if (!isGrounded && input != 0f)
        {
            Vector2 origin = (Vector2)transform.position + Vector2.up * (col.bounds.size.y * 0.5f - 0.05f);
            Vector2 dir = input > 0 ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, wallLayer);
            touchingWall = hit.collider != null;
        }

        if (touchingWall)
        {
            // Si chocamos contra un muro en el aire, anulamos la velocidad X.
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            // Movimiento horizontal normal.
            rb.velocity = new Vector2(input * moveSpeed, rb.velocity.y);
        }

        // El flip del jugador se gestiona en otro script (por ejemplo, PlayerAim).
    }

    void CheckJump()
    {
        // Comprobar si estamos en suelo.
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Actualizar coyote timer.
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Salto con barra espaciadora.
        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimer > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f); // Cancelar velocidad vertical previa.
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            coyoteTimer = 0f;
        }
    }

    void CheckLanding()
    {
        bool currentlyGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        // Si acabamos de aterrizar, podría ejecutarse alguna animación de aterrizaje.
        wasGrounded = currentlyGrounded;
    }

    void CheckDashInput()
    {
        // Si pulsas Shift y no estás en dash ni en cooldown, iniciamos dash.
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        // Cancelamos cualquier velocidad previa.
        rb.velocity = Vector2.zero;

        // Desactivamos gravedad durante el dash.
        rb.gravityScale = 0f;

        // Determinar la dirección del dash.
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        float dir;
        if (horizontalInput > 0f)
            dir = 1f; // Derecha.
        else if (horizontalInput < 0f)
            dir = -1f; // Izquierda.
        else
            // Si no se pulsa izquierda/derecha, usamos la dirección basada en la escala del jugador.
            dir = (transform.localScale.x >= 0f) ? 1f : -1f;

        // Aplicar dash horizontal.
        rb.velocity = new Vector2(dir * dashSpeed, 0f);
    }

    private void EndDash()
    {
        isDashing = false;
        // Restauramos la gravedad original.
        rb.gravityScale = originalGravityScale;
        // Evitamos cambios bruscos en la velocidad vertical.
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        // Dibuja un raycast lateral para depuración en el editor.
        if (!Application.isPlaying && col != null)
        {
            float halfHeight = col.bounds.size.y * 0.5f - 0.05f;
            Vector2 origin = (Vector2)transform.position + Vector2.up * halfHeight;
            Vector2 dir = (transform.localScale.x >= 0f) ? Vector2.right : Vector2.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + dir * wallCheckDistance);
        }
    }
}
