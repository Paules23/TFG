using UnityEngine;

public class ShellCasing : MonoBehaviour
{
    [Header("Movimiento")]
    public Vector2 initialVelocity = new Vector2(-2f, 2f); // dirección inicial
    public float angularVelocity = 360f; // grados/segundo
    public float lifetime = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = initialVelocity;
            rb.angularVelocity = angularVelocity;
        }

        Destroy(gameObject, lifetime);
    }
}
