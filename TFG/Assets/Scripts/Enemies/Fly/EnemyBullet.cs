using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 3f;
    public float damage = 15f;

    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var dmg = other.GetComponent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
