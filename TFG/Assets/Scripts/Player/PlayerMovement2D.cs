using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float jumpForce = 25f;
    public bool canFlip = true;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Squash & Stretch")]
    [Tooltip("Factor extra de ancho (X) al squash")]
    public float squashWidthFactor = 1.2f;
    [Tooltip("Factor extra de alto (Y) al squash")]
    public float squashHeightFactor = 0.8f;
    [Tooltip("Duración de cada fase (segundos)")]
    public float squashDuration = 0.1f;

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

    public bool FacingRight { get; private set; } = true;

    // Componentes privados
    private Rigidbody2D rb;
    private Collider2D col;
    private float originalGravityScale;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isSquashing;

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
        isSquashing = false;
        coyoteTimer = 0f;
        isDashing = false;
        dashTimer = 0f;
        dashCooldownTimer = 0f;

        // Aseguramos que el player no tenga fricción al moverse
        var mat = new PhysicsMaterial2D();
        mat.friction = 0f;
        col.sharedMaterial = mat;


    }

    void Update()
    {
        // Actualizar timers
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
            return; // mientras dashing, no hacemos Move/Jump normales
        }

        Move();
        CheckJump();
        CheckLanding();
        CheckDashInput();
    }

    void Move()
    {
        float input = Input.GetAxisRaw("Horizontal");

        // Raycast horizontal para detectar muro solo si no estamos en el suelo
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
            // Si chocamos contra un muro en el aire, anulamos la velocidad X
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else
        {
            // movimiento horizontal normal
            rb.velocity = new Vector2(input * moveSpeed, rb.velocity.y);
        }

        // Flip visual
        if (canFlip && input != 0f)
        {
            FacingRight = input > 0f;
            Vector3 scale = transform.localScale;
            scale.x = FacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void CheckJump()
    {
        // Comprobamos si estamos en el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Actualizamos coyote timer
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Salto
        if (Input.GetKeyDown(KeyCode.W) && coyoteTimer > 0f)
        {
            StartCoroutine(SquashRoutine());
            rb.velocity = new Vector2(rb.velocity.x, 0f); // cancelar Y anterior
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            coyoteTimer = 0f;
        }
    }

    void CheckLanding()
    {
        bool currentlyGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (!wasGrounded && currentlyGrounded)
        {
            StartCoroutine(SquashRoutine());
        }
        wasGrounded = currentlyGrounded;
    }

    private IEnumerator SquashRoutine()
    {
        if (isSquashing) yield break;
        isSquashing = true;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(
            startScale.x * squashWidthFactor,
            startScale.y * squashHeightFactor,
            startScale.z
        );

        float timer = 0f;
        // Fase 1: squash
        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Fase 2: restore
        timer = 0f;
        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            yield return null;
        }

        transform.localScale = startScale;
        isSquashing = false;
    }

    void CheckDashInput()
    {
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

        // Resetear velocidad vertical y horizontal
        rb.velocity = Vector2.zero;
        // Desactivamos gravedad para un dash puro horizontal
        rb.gravityScale = 0f;

        float dir = FacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dir * dashSpeed, 0f);
    }

    private void EndDash()
    {
        isDashing = false;
        // Restaurar gravityScale original (el que viene del Inspector)
        rb.gravityScale = originalGravityScale;
        // Mantiene velocidad vertical en 0 para no saltar bruscamente
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        // Dibuja raycast de muro
        if (!Application.isPlaying && col != null)
        {
            float halfHeight = col.bounds.size.y * 0.5f - 0.05f;
            Vector2 origin = (Vector2)transform.position + Vector2.up * halfHeight;
            Vector2 dir = FacingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + dir * wallCheckDistance);
        }
    }
}
