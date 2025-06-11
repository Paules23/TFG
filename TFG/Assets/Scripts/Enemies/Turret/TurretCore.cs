using System.Collections.Generic;
using UnityEngine;

/// Núcleo estático que genera anillos de bolas y los hace rotar.
[RequireComponent(typeof(Collider2D))]
public class TurretCore : MonoBehaviour, IDamageable
{
    [Header("Core Stats")]
    public float coreHealth = 30f;

    [System.Serializable]
    public class RingSettings
    {
        public string name = "Ring";
        public GameObject ballPrefab;
        public int ballCount = 8;
        public float radius = 2f;
        public float angularSpeed = 90f; // grados/segundo (+ horario, – antihorario)
    }

    [Header("Rings")]
    public RingSettings innerRing;
    public RingSettings outerRing;

    private readonly List<Transform> ringPivots = new(); // pivotes que rota el núcleo

    void Start()
    {
        CreateRing(innerRing);
        CreateRing(outerRing);
    }

    void Update()
    {
        // Rotar cada anillo
        if (ringPivots.Count > 0)
        {
            RotateRing(ringPivots[0], innerRing.angularSpeed);
            if (ringPivots.Count > 1)
                RotateRing(ringPivots[1], outerRing.angularSpeed);
        }
    }

    /* ───────────────────────────────────────── helpers ───────────────────────────────────────── */

    void CreateRing(RingSettings cfg)
    {
        if (cfg.ballPrefab == null || cfg.ballCount <= 0) return;

        // 1. Crear pivote vacío como hijo del núcleo (para rotarlo fácilmente).
        GameObject pivotGO = new GameObject(cfg.name + "_Pivot");
        pivotGO.transform.SetParent(transform);
        pivotGO.transform.localPosition = Vector3.zero;
        ringPivots.Add(pivotGO.transform);

        // 2. Instanciar bolas alrededor del pivote.
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

    /* ────────────────────────────── IDamageable implementation ─────────────────────────────── */

    public void TakeDamage(float dmg)
    {
        coreHealth -= dmg;
        if (coreHealth <= 0f) Destroy(gameObject);
    }
}
