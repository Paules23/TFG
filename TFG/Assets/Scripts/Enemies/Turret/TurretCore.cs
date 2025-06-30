using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class TurretCore : MonoBehaviour, IDamageable
{
    [Header("Core Stats")]
    public float coreHealth = 50f;
    [Tooltip("Prefab del cuerpo del enemigo al morir")]
    public GameObject DeadBodyPrefab;

    [System.Serializable]
    public class RingSettings
    {
        public string name = "Ring";
        public GameObject ballPrefab;
        public int ballCount = 8;
        public float radius = 2f;
        public float angularSpeed = 90f;
    }

    [Header("Rings")]
    public RingSettings innerRing;
    public RingSettings outerRing;

    [Header("Visual Feedback")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.2f;

    private SpriteRenderer spriteRenderer;
    private readonly List<Transform> ringPivots = new();
    private Coroutine flashCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        CreateRing(innerRing);
        CreateRing(outerRing);
    }

    void Update()
    {
        if (ringPivots.Count > 0)
        {
            RotateRing(ringPivots[0], innerRing.angularSpeed);
            if (ringPivots.Count > 1)
                RotateRing(ringPivots[1], outerRing.angularSpeed);
        }
    }

    void CreateRing(RingSettings cfg)
    {
        if (cfg.ballPrefab == null || cfg.ballCount <= 0) return;

        GameObject pivotGO = new GameObject(cfg.name + "_Pivot");
        pivotGO.transform.SetParent(transform);
        pivotGO.transform.localPosition = Vector3.zero;
        ringPivots.Add(pivotGO.transform);

        for (int i = 0; i < cfg.ballCount; i++)
        {
            float angle = i * Mathf.PI * 2f / cfg.ballCount;
            Vector3 localPos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * cfg.radius;

            GameObject ball = Instantiate(cfg.ballPrefab, pivotGO.transform);
            ball.transform.localPosition = localPos;
        }
    }

    void RotateRing(Transform pivot, float speedDegPerSec)
        => pivot.Rotate(Vector3.forward, speedDegPerSec * Time.deltaTime, Space.Self);

    public void TakeDamage(float dmg)
    {
        coreHealth -= dmg;

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (coreHealth <= 0f)
        {
            // Instancia cuerpo muerto del core
            Instantiate(DeadBodyPrefab, transform.position, Quaternion.identity);

            // Matar todas las bolas orbitantes correctamente
            OrbitingBall[] orbitingBalls = GetComponentsInChildren<OrbitingBall>();
            foreach (OrbitingBall ball in orbitingBalls)
            {
                ball.TakeDamage(ball.maxHealth); // Fuerza la muerte con feedback visual
            }

            Destroy(gameObject);
        }
    }

    private IEnumerator FlashDamage()
    {
        if (flashCoroutine != null)
            yield break; // ya se está ejecutando, no hacemos nada

        flashCoroutine = StartCoroutine(DoFlashDamage());
    }

    private IEnumerator DoFlashDamage()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageColor;
        yield return new WaitForSecondsRealtime(flashDuration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }
}
