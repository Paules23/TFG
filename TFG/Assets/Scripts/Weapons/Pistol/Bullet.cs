using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Parameters")]
    [Tooltip("Daño que aplicará al chocar con un IDamageable")]
    public float damage = 25f;
    [Tooltip("Rango máximo que puede recorrer la bala")]
    public float maxRange = 10f;

    [Header("Collision Layers")]
    [Tooltip("Capa(s) en las que la bala infligirá daño (p.ej. enemigos u objetos dañables)")]
    public LayerMask damageLayers;
    [Tooltip("Capa(s) de entorno o suelo donde se destruye la bala")]
    public LayerMask environmentLayers;
    [Tooltip("Capa(s) de loot que la bala atraviesa sin destrucción")]
    public LayerMask lootLayers;

    [Header("Game feel")]
    public float timeStop;
    public float ShakeDuration = 0.3f;
    public float ShakeMagnitude = 0.2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Bullet] Colisionó con '{other.gameObject.name}' en layer {other.gameObject.layer} ('{LayerMask.LayerToName(other.gameObject.layer)}')");

        if (other.CompareTag("Player"))
            return;

        int otherLayer = 1 << other.gameObject.layer;

        if ((otherLayer & lootLayers) != 0)
            return;

        if ((otherLayer & damageLayers) != 0)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(ShakeDuration, ShakeMagnitude);

            HitStopManager.Instance.FreezeFrame(timeStop);
            Destroy(gameObject);
            return;
        }

        if ((otherLayer & environmentLayers) != 0)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;
        int otherLayer = 1 << other.gameObject.layer;

        if ((otherLayer & damageLayers) != 0)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(ShakeDuration, ShakeMagnitude);

            HitStopManager.Instance.FreezeFrame(timeStop);
            Destroy(gameObject);
            return;
        }

        if ((otherLayer & environmentLayers) != 0)
        {
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(ShakeDuration, ShakeMagnitude);

            Destroy(gameObject);
        }
    }
}
