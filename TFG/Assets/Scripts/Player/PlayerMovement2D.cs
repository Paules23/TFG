using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float jumpForce = 25f;

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

    // Componentes privados
    private Rigidbody2D rb;
    private Collider2D col;
    private float originalGravityScale;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isSquashing;

    // Escala original (para evitar acumulación de squash)
    private Vector3 baseScale;

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

        // Guardamos la escala inicial del personaje
        baseScale = transform.localScale;

        // PhysicsMaterial2D para evitar “pegado” a muros
        var mat = new PhysicsMaterial2D();
        mat.friction = 0f;
        col.sharedMaterial = mat;
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
            return; // mientras dash, no hacemos Move/Jump normales
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

        // **NO hacemos flip aquí**. El flip lo gestiona PlayerAim u otro script externo.
    }

    void CheckJump()
    {
        // Comprobar si estamos en suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Actualizar coyote timer
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Salto con barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimer > 0f)
        {
            StartCoroutine(SquashRoutine());
            rb.velocity = new Vector2(rb.velocity.x, 0f); // cancelar Y previa
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
        // Si ya se está “squasheando”, salimos para no superponer
        if (isSquashing) yield break;
        isSquashing = true;

        // Siempre partimos de la escala base (sin acumulaciones anteriores)
        Vector3 startScale = baseScale;
        Vector3 targetScale = new Vector3(
            baseScale.x * squashWidthFactor,
            baseScale.y * squashHeightFactor,
            baseScale.z
        );

        float timer = 0f;
        // Fase 1: deformar a targetScale
        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Fase 2: volver a baseScale
        timer = 0f;
        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            yield return null;
        }

        // Aseguramos que queda exactamente en baseScale
        transform.localScale = baseScale;
        isSquashing = false;
    }

    void CheckDashInput()
    {
        // Si pulsas Shift y no está en cooldown ni ya dashing
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

        // Anulamos toda velocidad previa
        rb.velocity = Vector2.zero;

        // Desactivamos gravedad durante el dash
        rb.gravityScale = 0f;

        // Determinar dirección de dash:
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        float dir;
        if (horizontalInput > 0f)
        {
            dir = 1f;         // moviéndonos hacia la derecha
        }
        else if (horizontalInput < 0f)
        {
            dir = -1f;        // moviéndonos hacia la izquierda
        }
        else
        {
            // Si no se pulsa izquierda/derecha, usar la dirección de flip (localScale.x)
            dir = (transform.localScale.x >= 0f) ? 1f : -1f;
        }

        // Aplicar dash horizontal
        rb.velocity = new Vector2(dir * dashSpeed, 0f);
    }

    private void EndDash()
    {
        isDashing = false;
        // Restaurar gravedad original
        rb.gravityScale = originalGravityScale;
        // Mantener velocidad vertical en 0 para evitar caída brusca
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        // Dibuja raycast lateral para depuración
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
