using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class LootFragment : MonoBehaviour
{
    public float attractionSpeed = 3.5f;
    public float acceleration = 10f;
    public float attractionRange = 3f;
    public string playerTag = "Player";

    public float destroyYThreshold = -50f; // 👈 Umbral de destrucción por caída

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
        //  Destruye si cae fuera del mapa
        if (transform.position.y <= destroyYThreshold)
        {
            Destroy(gameObject);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attractionRange)
        {
            if (!isAttracted)
            {
                isAttracted = true;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = true;
                col.isTrigger = true;
            }

            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, attractionSpeed);

            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);
        }
        else if (isAttracted)
        {
            isAttracted = false;
            currentSpeed = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            col.isTrigger = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Destroy(gameObject);
        }
    }
}
