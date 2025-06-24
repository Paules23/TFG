using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class ChargingBull : MonoBehaviour, IDamageable
{
    public enum BullState { Idle, Preparing, Charging, Cooling, SpearStunned }

    [Header("Stats")]
    public float maxHealth = 50f;
    public float detectionRange = 5f;
    public float preparationTime = 2f;
    public float chargeSpeed = 12f;
    public float stopThreshold = 0.5f;
    public float slideFriction = 0.5f;
    public float minSlideSpeed = 0.3f;
    public float damageToPlayer = 20f;
    public float spearStunTime = 1f;

    [Header("Flash on Hit")]
    public Color hitFlashColor = new Color(0.5f, 0f, 0.5f); // Lila
    public float hitFlashDuration = 0.15f;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color idleColor = Color.green;
    public Color preparingColor = Color.yellow;
    public Color chargingColor = Color.red;
    public Color coolingColor = Color.blue;
    public Color spearStunColor = Color.magenta;

    private BullState state;
    private float health;
    private float fixedY;
    private float speedX;
    private float chargeDir;
    private Transform player;
    private Collider2D col;
    private Rigidbody2D rb;

    void Awake()
    {
        health = maxHealth;
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();

        col.isTrigger = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        fixedY = transform.position.y;

        state = BullState.Idle;
        UpdateColor();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("ChargingBull: No se encontró el Player.");
    }

    void Update()
    {
        if (player == null) return;

        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);

        switch (state)
        {
            case BullState.Idle: TryDetect(); break;
            case BullState.Preparing: break;
            case BullState.Charging: DoCharge(); break;
            case BullState.Cooling: DoCooling(); break;
            case BullState.SpearStunned: break;
        }
    }

    void TryDetect()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        if (dx <= detectionRange)
        {
            state = BullState.Preparing;
            UpdateColor();
            StartCoroutine(PrepareAndCharge());
        }
    }

    IEnumerator PrepareAndCharge()
    {
        yield return new WaitForSeconds(preparationTime);
        state = BullState.Charging;
        chargeDir = Mathf.Sign(player.position.x - transform.position.x);
        speedX = chargeDir * chargeSpeed;
        UpdateColor();
    }

    void DoCharge()
    {
        transform.Translate(Vector2.right * speedX * Time.deltaTime);

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        if (dx <= stopThreshold)
        {
            state = BullState.Cooling;
            UpdateColor();
        }
    }

    void DoCooling()
    {
        speedX *= 1f - slideFriction * Time.deltaTime;
        transform.Translate(Vector2.right * speedX * Time.deltaTime);

        if (Mathf.Abs(speedX) < minSlideSpeed)
        {
            speedX = 0f;
            state = BullState.Preparing;
            UpdateColor();
            StartCoroutine(PrepareAndCharge());
        }
    }

    public void ApplySpearStun()
    {
        Debug.Log("[ChargingBull] ApplySpearStun() llamado desde lanza.");
        StopAllCoroutines();
        speedX = 0f;
        StartCoroutine(SpearStun());
    }

    IEnumerator SpearStun()
    {
        state = BullState.SpearStunned;
        UpdateColor();
        Debug.Log($"[ChargingBull] Aturdido durante {spearStunTime}s.");
        float t = 0f;
        while (t < spearStunTime)
        {
            t += Time.deltaTime;
            yield return null;
        }
        Debug.Log("[ChargingBull] Fin de stun → Preparing.");
        state = BullState.Preparing;
        UpdateColor();
        StartCoroutine(PrepareAndCharge());
    }

    public void TakeDamage(float dmg)
    {
        if (state == BullState.Idle)
        {
            Debug.Log("[ChargingBull] Ignora el daño porque está en estado Idle.");
            return;
        }

        health -= dmg;
        StartCoroutine(HitFlash());
        if (health <= 0f)
            Destroy(gameObject);
    }


    IEnumerator HitFlash()
    {
        Color prev = spriteRenderer.color;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        UpdateColor();
    }

    void UpdateColor()
    {
        if (spriteRenderer == null) return;
        switch (state)
        {
            case BullState.Idle: spriteRenderer.color = idleColor; break;
            case BullState.Preparing: spriteRenderer.color = preparingColor; break;
            case BullState.Charging: spriteRenderer.color = chargingColor; break;
            case BullState.Cooling: spriteRenderer.color = coolingColor; break;
            case BullState.SpearStunned: spriteRenderer.color = spearStunColor; break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var pd = other.GetComponent<IDamageable>();
            if (pd != null) pd.TakeDamage(damageToPlayer);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
