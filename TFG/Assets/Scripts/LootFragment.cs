using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class LootFragment : MonoBehaviour
{
    public float attractionSpeed = 5f;
    public float acceleration = 10f;
    public float attractionRange = 5f;
    public string playerTag = "Player";

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isAttracted = false;
    private float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;


        if (player == null)
        {
            Debug.LogWarning("No player found with tag: " + playerTag);
        }
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attractionRange)
        {
            if (!isAttracted)
            {
                isAttracted = true;

                // Activar modo atracción
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = true;
                col.isTrigger = true; // Evita colisión física con el player
            }

            // Movimiento hacia el jugador
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, attractionSpeed);

            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);
        }
        else if (isAttracted)
        {
            // Salió del rango de atracción
            isAttracted = false;
            currentSpeed = 0f;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            col.isTrigger = false; // Vuelve a tener colisión física normal
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Más adelante aquí puedes sumar al inventario según color, etc.
            Destroy(gameObject);
        }
    }
}
