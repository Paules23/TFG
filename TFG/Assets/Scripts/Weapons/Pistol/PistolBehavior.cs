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
    [Tooltip("Referencia al PlayerAim para saber dónde mira")]
    public PlayerAim playerAim;
    [Tooltip("SpriteRenderers de la pistola (handle + barrel)")]
    public SpriteRenderer[] graphics;

    int currentAmmo;
    bool isReloading;
    float nextFireTime;
    bool lastFacing;

    private void Start()
    {
        currentAmmo = maxAmmo;
        if (playerTransform == null && transform.parent)
            playerTransform = transform.parent;
        if (playerAim == null && playerTransform)
            playerAim = playerTransform.GetComponent<PlayerAim>();
        lastFacing = playerAim != null ? playerAim.FacingRight : true;
    }

    private void Update()
    {
        if (!playerTransform) return;

        Orbit();
        Aim();

        // flip visual de sprites
        bool facing = playerAim != null ? playerAim.FacingRight
                                        : (transform.position.x >= playerTransform.position.x);
        if (facing != lastFacing)
        {
            lastFacing = facing;
            foreach (var sr in graphics)
                sr.flipX = !facing;
        }

        if (isReloading) return;
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0) Shoot();
            else StartCoroutine(Reload());
        }
    }

    private void Orbit()
    {
        var mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mw.z = 0;
        var dir = (mw - playerTransform.position).normalized;
        var target = playerTransform.position + dir * orbitRadius;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * aimLerpSpeed);
    }

    private void Aim()
    {
        // 1) Calculamos el ángulo base hacia el ratón
        var mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mw.z = 0f;
        var toMouse = (mw - transform.position).normalized;
        float baseAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;

        // 2) Si estamos mirando a la izquierda, sumamos 180°
        float targetAngle = lastFacing ? baseAngle : baseAngle + 180f;

        // 3) Interpolamos suavemente usando LerpAngle, que respeta wrapping
        float currentAngle = transform.eulerAngles.z;
        float smoothed = Mathf.LerpAngle(
            currentAngle,
            targetAngle,
            Mathf.Clamp01(Time.deltaTime * aimLerpSpeed)
        );

        transform.rotation = Quaternion.Euler(0f, 0f, smoothed);
    }

    private void Shoot()
    {
        var b = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        var rb = b.GetComponent<Rigidbody2D>();
        if (rb)
        {
            // dispara según el up local transformado a world (ignora escala)
            var dir = muzzlePoint.TransformDirection(Vector3.up).normalized;
            rb.velocity = dir * bulletSpeed;
        }
        var bs = b.GetComponent<Bullet>();
        if (bs != null)
        {
            bs.damage = bulletDamage;
            bs.lifetime = bulletLifetime;
        }

        currentAmmo--;
        nextFireTime = Time.time + 1f / fireRate;
        if (currentAmmo <= 0) StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), $"Ammo: {currentAmmo}/{maxAmmo}");
        if (isReloading)
            GUI.Label(new Rect(10, 30, 200, 20), "Reloading...");
    }
}
