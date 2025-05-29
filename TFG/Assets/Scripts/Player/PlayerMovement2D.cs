using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
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

    public bool FacingRight { get; private set; } = true;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isSquashing;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wasGrounded = true;
        isSquashing = false;
    }

    void Update()
    {
        Move();
        CheckJump();
        CheckLanding();
    }

    void Move()
    {
        float input = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(input * moveSpeed, rb.velocity.y);

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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            StartCoroutine(SquashRoutine());
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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

        // Capturamos la escala actual (incluye signo en X)
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(
            startScale.x * squashWidthFactor,
            startScale.y * squashHeightFactor,
            startScale.z
        );

        // Fase 1: hacia targetScale
        float timer = 0f;
        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Fase 2: de vuelta a startScale
        timer = 0f;
        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            yield return null;
        }

        // Fijar escala exacta y reset flag
        transform.localScale = startScale;
        isSquashing = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
