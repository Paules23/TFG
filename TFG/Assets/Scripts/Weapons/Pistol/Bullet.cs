using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Parameters")]
    [Tooltip("Da�o que aplicar� al chocar con un IDamageable")]
    public float damage = 25f;
    [Tooltip("Tiempo (seg) antes de autodestruir la bala")]
    public float lifetime = 3f;
    [Tooltip("Rango m�ximo que puede recorrer la bala")]
    public float maxRange = 10f;

    [Header("Collision Layers")]
    [Tooltip("Capa(s) en las que la bala infligir� da�o (p.ej. enemigos u objetos da�ables)")]
    public LayerMask damageLayers;
    [Tooltip("Capa(s) de entorno en las que la bala se destruye sin da�o (suelo, paredes, etc.)")]
    public LayerMask environmentLayers;
    [Tooltip("Capa(s) de loot que la bala atraviesa sin destrucci�n")]
    public LayerMask lootLayers;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
        // Asegurarse de destruir la bala si pasa su tiempo de vida, por si no llega al rango.
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Si la distancia recorrida es mayor o igual al rango m�ximo, destruir la bala.
        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Ignorar si es el jugador.
        if (other.CompareTag("Player"))
            return;

        int otherLayer = 1 << other.gameObject.layer;

        // 2) Si es loot, no hacer nada.
        if ((otherLayer & lootLayers) != 0)
            return;

        // 3) Si colisiona con un objeto en damageLayers, intenta da�ar y luego se destruye.
        if ((otherLayer & damageLayers) != 0)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 4) Si colisiona con un objeto en environmentLayers, se destruye sin aplicar da�o.
        if ((otherLayer & environmentLayers) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // 5) Otras capas: no se hace nada.
    }
}
