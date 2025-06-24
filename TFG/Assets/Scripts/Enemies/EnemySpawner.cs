using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour, IDamageable
{
    [Header("Enemy Prefabs")]
    [Tooltip("Lista de prefabs de enemigos a spawnear")]
    public List<GameObject> enemyPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Intervalo en segundos entre spawns")]
    public float spawnInterval = 3f;
    [Tooltip("Número máximo de enemigos activos")]
    public int maxEnemies = 10;
    [Tooltip("Tamaño del área de spawn (se utiliza para calcular posiciones aleatorias)")]
    public Vector2 spawnAreaSize = new Vector2(8f, 5f);
    [Tooltip("Desplazamiento adicional al centro del spawner")]
    public Vector2 spawnOffset = Vector2.zero;

    [Header("Player Detection")]
    [Tooltip("Radio en el que el spawner detecta al jugador para comenzar a spawnear")]
    public float playerDetectionRange = 15f;

    [Header("Spawner Health")]
    [Tooltip("Salud máxima del spawner")]
    public float spawnerMaxHealth = 100f;
    private float spawnerCurrentHealth;

    [Header("Flash on Hit")]
    [Tooltip("Color con el que destella al recibir daño")]
    public Color hitFlashColor = new Color(0.5f, 0f, 0.5f);
    [Tooltip("Duración del flash (segundos)")]
    public float hitFlashDuration = 0.15f;

    [Header("Visual")]
    [Tooltip("Referencia al SpriteRenderer para mostrar el flash al recibir daño")]
    public SpriteRenderer spriteRenderer;

    // Internos
    private float timer;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Transform player;

    void Start()
    {
        spawnerCurrentHealth = spawnerMaxHealth;
        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();

        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("EnemySpawner: No se encontró ningún GameObject con tag 'Player'.");
    }

    void Update()
    {
        // Si el spawner está muerto, no se hace nada
        if (spawnerCurrentHealth <= 0f)
            return;

        // Asegurar que el jugador esté presente
        if (player == null)
            return;

        // Siempre se elimina de la lista a los enemigos que hayan sido destruidos
        spawnedEnemies.RemoveAll(e => e == null);

        // El timer siempre cuenta, incluso si el jugador no está en el área
        timer += Time.deltaTime;

        // Determinamos si el jugador se encuentra dentro del área de detección
        bool playerInRange = (Vector2.Distance(transform.position, player.position) <= playerDetectionRange);

        // Si ya pasó el intervalo, hay menos enemigos que el máximo Y el jugador está en rango...
        if (timer >= spawnInterval && spawnedEnemies.Count < maxEnemies && playerInRange)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0)
            return;

        // Se elige un prefab aleatoriamente
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        // Se calcula una posición aleatoria en el área de spawn definida
        Vector2 randomPos = new Vector2(
            Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
            Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f)
        ) + (Vector2)transform.position + spawnOffset;

        // Se instancia el enemigo y se guarda la referencia
        GameObject enemy = Instantiate(prefab, randomPos, Quaternion.identity);
        spawnedEnemies.Add(enemy);
    }

    void OnDrawGizmosSelected()
    {
        // Dibuja el área de spawn en rojo
        Gizmos.color = Color.red;
        Vector3 spawnCenter = transform.position + (Vector3)spawnOffset;
        Gizmos.DrawWireCube(spawnCenter, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f));

        // Dibuja el área de detección del jugador en azul
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
    }

    // Implementación de la interfaz IDamageable
    public void TakeDamage(float dmg)
    {
        spawnerCurrentHealth -= dmg;
        StartCoroutine(FlashHit());
        if (spawnerCurrentHealth <= 0f)
            Die();
    }

    IEnumerator FlashHit()
    {
        if (spriteRenderer != null)
        {
            Color origColor = spriteRenderer.color;
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = origColor;
        }
    }

    void Die()
    {
        // Se pueden agregar efectos de destrucción o sonidos aquí
        Destroy(gameObject);
    }
}
