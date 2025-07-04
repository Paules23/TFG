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
    public float coyoteTime = 0.1f;
    private float coyoteTimer;

    [Header("Dash Settings")]
    public float dashSpeed = 24f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    [Header("Wall Slide/Fall Fix")]
    public float wallCheckDistance = 0.1f;
    public LayerMask wallLayer;

    [Header("Dash Cooldown Color")]
    [Tooltip("Color que adopta el jugador al hacer dash")]
    public Color cooldownColor = Color.gray;

    [Header("God Mode Settings")]
    public float godModeSpeed = 10f;
    private bool isGodMode = false;

    private Color preDashColor;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Collider2D col;
    private float originalGravityScale;
    private bool isGrounded;
    private bool wasGrounded;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalGravityScale = rb.gravityScale;
        wasGrounded = true;
        coyoteTimer = 0f;
        isDashing = false;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
    }

    void Update()
    {
        // Toggle God Mode antes de cualquier otro movimiento
        ToggleGodMode();

        // Si God Mode está activo, usar movimiento libre y salir
        if (isGodMode)
        {
            GodModeMovement();
            return;
        }

        // Actualizamos cooldown dash y color
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
            float t = 1f - (dashCooldownTimer / dashCooldown);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(cooldownColor, preDashColor, t);
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                EndDash();
            return;
        }

        Move();
        CheckJump();
        CheckLanding();
        CheckDashInput();
    }

    void Move()
    {
        float input = Input.GetAxisRaw("Horizontal");
        bool touchingWall = false;

        if (!isGrounded && input != 0f)
        {
            Vector2 origin = (Vector2)transform.position +
                             Vector2.up * (col.bounds.size.y * 0.5f - 0.05f);
            Vector2 dir = input > 0 ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, wallLayer);
            touchingWall = hit.collider != null;
        }

        if (touchingWall)
            rb.velocity = new Vector2(0f, rb.velocity.y);
        else
            rb.velocity = new Vector2(input * moveSpeed, rb.velocity.y);
    }

    void CheckJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimer > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            coyoteTimer = 0f;
        }
    }

    void CheckLanding()
    {
        bool currentlyGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        wasGrounded = currentlyGrounded;
    }

    void CheckDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing)
            StartDash();
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (spriteRenderer != null)
            preDashColor = spriteRenderer.color;
        if (spriteRenderer != null)
            spriteRenderer.color = cooldownColor;

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float dir;
        if (horizontalInput > 0f) dir = 1f;
        else if (horizontalInput < 0f) dir = -1f;
        else dir = (transform.localScale.x >= 0f) ? 1f : -1f;

        rb.velocity = new Vector2(dir * dashSpeed, 0f);
    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = originalGravityScale;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    void ToggleGodMode()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            isGodMode = !isGodMode;

            if (isGodMode)
            {
                col.enabled = false;
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
            }
            else
            {
                col.enabled = true;
                rb.gravityScale = originalGravityScale;
            }
        }
    }

    void GodModeMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;

        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        rb.velocity = direction * godModeSpeed;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

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
