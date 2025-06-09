using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Parameters")]
    [Tooltip("Daño que aplicará al chocar con un IDamageable")]
    public float damage = 25f;
    [Tooltip("Tiempo (seg) antes de autodestruir la bala")]
    public float lifetime = 3f;

    [Header("Collision Layers")]
    [Tooltip("Capa(s) de entorno donde la bala se destruye sin dañar")]
    public LayerMask HitLayers;
    [Tooltip("Capa(s) de loot que la bala atraviesa sin destrucción")]
    public LayerMask lootLayers;

    private void Start()
    {
        // Autodestrucción tras lifetime segundos
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Ignorar colisión con el mismo jugador
        if (other.CompareTag("Player"))
            return;

        // 2) Si es loot, NO destruir ni dañar
        if (((1 << other.gameObject.layer) & lootLayers) != 0)
            return;

        // 3) Intentar dañar IDamageable
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 4) Si colisiona con entorno, destruir la bala (sin daño)
        if (((1 << other.gameObject.layer) & HitLayers) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // 5) Otras capas: no destruimos ni dañamos
    }
}
