using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class LootFragment : MonoBehaviour
{
    [Header("Attraction Settings")]
    [Tooltip("Velocidad máxima a la que el loot se mueve hacia el jugador")]
    [SerializeField] private float attractionSpeed = 3.5f;

    [Tooltip("Qué tan rápido acelera al moverse hacia el jugador")]
    [SerializeField] private float acceleration = 10f;

    [Tooltip("Distancia máxima a la que el loot comienza a ser atraído")]
    [SerializeField] private float attractionRange = 3f;

    [Tooltip("Tag del jugador que atraerá el loot")]
    [SerializeField] private string playerTag = "Player";

    [Header("Cleanup")]
    [Tooltip("Y mínima antes de autodestruir el loot (por caída fuera del mapa)")]
    [SerializeField] private float destroyYThreshold = -50f;

    [Header("Game Feel")]
    [Tooltip("Cantidad de zoom al recoger el loot")]
    [SerializeField] private float zoomAmount = 0.4f;
    [SerializeField] private float zoomDuration = 0.1f;


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
            if (CameraZoom.Instance != null)
            {
                CameraZoom.Instance.Zoom(zoomAmount, zoomDuration);
            }

            Destroy(gameObject);
        }
    }
}
