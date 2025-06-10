using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PistolBehaviour : MonoBehaviour
{
    [Header("Orbit & Aim")]
    public Transform playerTransform;
    public float orbitRadius = 0.6f;
    public float aimLerpSpeed = 15f;

    [Header("Fire Settings")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;
    public float bulletSpeed = 15f;
    public float fireRate = 5f;
    public float reloadTime = 1.5f;

    [Header("Ammo")]
    public int maxAmmo = 12;
    public float bulletDamage = 25f;
    public float bulletLifetime = 3f;

    [Header("Visual Flip")]
    public PlayerAim playerAim;
    public SpriteRenderer[] graphics;

    int currentAmmo;
    bool isReloading;
    float nextFireTime;
    bool lastFacingRight;

    void Start()
    {
        currentAmmo = maxAmmo;
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerAim == null && playerTransform)
            playerAim = playerTransform.GetComponent<PlayerAim>();
        lastFacingRight = playerAim != null ? playerAim.FacingRight : true;
    }

    void Update()
    {
        if (playerTransform == null) return;

        Orbit();
        Aim();  // Ya rota correctamente

        // Cálculo de toMouse para la dirección hacia el cursor
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 toMouse = (mouseWorld - (Vector2)transform.position).normalized;

        // ------ FLIP LÓGICO CON DOT Y CORRECCIÓN DE VARIABLE ------
        bool flip = Vector2.Dot(transform.right, toMouse) < 0f;
        foreach (var sr in graphics)
            sr.flipY = flip;

        // Conservamos escala unitaria en Y y flip X del jugador
        float signX = playerTransform.localScale.x > 0 ? 1f : -1f;
        transform.localScale = new Vector3(signX, 1f, 1f);

        // Disparo gestionado externamente por PlayerAttack
    }


    void Orbit()
    {
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mw.z = 0f;
        Vector3 dir = (mw - playerTransform.position).normalized;
        Vector3 tgt = playerTransform.position + dir * orbitRadius;
        transform.position = Vector3.Lerp(transform.position, tgt, Time.deltaTime * aimLerpSpeed);
    }

    void Aim()
    {
        Vector3 mw3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mw3.z = 0f;
        Vector2 mouseDir = (Vector2)(mw3 - playerTransform.position);

        float baseAng = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

        // Always rotate by baseAng
        float sm = Mathf.LerpAngle(transform.eulerAngles.z, baseAng,
                                   Mathf.Clamp01(Time.deltaTime * aimLerpSpeed));
        transform.rotation = Quaternion.Euler(0f, 0f, sm);
    }

    // Llamar desde PlayerAttack para disparar
    public void TriggerFire()
    {
        if (isReloading || Time.time < nextFireTime) return;
        if (currentAmmo > 0) Shoot();
        else StartCoroutine(Reload());
    }

    void Shoot()
    {
        GameObject b = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector3 dir = muzzlePoint.TransformDirection(Vector3.up).normalized;
            rb.velocity = dir * bulletSpeed;
        }
        Bullet bs = b.GetComponent<Bullet>();
        if (bs != null)
        {
            bs.damage = bulletDamage;
            bs.lifetime = bulletLifetime;
        }

        currentAmmo--;
        nextFireTime = Time.time + 1f / fireRate;
        if (currentAmmo <= 0)
            StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
