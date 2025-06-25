using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class PistolBehaviour : MonoBehaviour
{
    [Header("Weapon Movement Bounds")]
    public float circleRadius = 1.5f;
    public Vector2 circleCenterOffset = new Vector2(0f, 0.6f);
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

    [Header("Visual")]
    public PlayerAim playerAim;
    [Tooltip("Color to tint all weapon sprites while reloading")]
    public Color reloadTintColor = Color.red; 

    // Estado interno
    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;
    private Transform playerTransform;

    // Sprites de arma y sus colores originales
    private SpriteRenderer[] graphics;
    private Color[] originalColors;

    void Awake()
    {
        playerTransform = transform.parent;
        if (playerAim == null && playerTransform != null)
            playerAim = playerTransform.GetComponent<PlayerAim>();

        // Obtener todos los SpriteRenderer hijos (excluye este)
        graphics = GetComponentsInChildren<SpriteRenderer>(true)
            .Where(sr => sr.gameObject != this.gameObject)
            .ToArray();
    }

    void Start()
    {
        currentAmmo = maxAmmo;

        // Guardar colores originales
        originalColors = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
            originalColors[i] = graphics[i].color;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowAmmoUI(true);
            UIManager.Instance.UpdateAmmoCount(currentAmmo, maxAmmo);
        }
    }

    void OnEnable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowAmmoUI(true);
    }

    void OnDisable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowAmmoUI(false);
        RestoreGraphicsColor();
    }

    void Update()
    {
        if (playerTransform == null) return;
        UpdateWeaponPositionAndRotation();
    }

    public void TriggerFire()
    {
        if (isReloading || Time.time < nextFireTime) return;
        if (currentAmmo > 0) Shoot();
        else StartCoroutine(Reload());
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector3 dir = muzzlePoint.TransformDirection(Vector3.up).normalized;
            rb.velocity = dir * bulletSpeed;
        }
        if (bullet.TryGetComponent<Bullet>(out var bs))
        {
            bs.damage = bulletDamage;
            bs.lifetime = bulletLifetime;
        }

        currentAmmo--;
        nextFireTime = Time.time + 1f / fireRate;
        UIManager.Instance?.UpdateAmmoCount(currentAmmo, maxAmmo);

        if (currentAmmo <= 0)
            StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;
        TintGraphics(reloadTintColor);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        RestoreGraphicsColor();
        UIManager.Instance?.UpdateAmmoCount(currentAmmo, maxAmmo);
    }

    void TintGraphics(Color tint)
    {
        foreach (var sr in graphics)
            sr.color = tint;
    }

    void RestoreGraphicsColor()
    {
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].color = originalColors[i];
    }

    void UpdateWeaponPositionAndRotation()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 localMouse = playerTransform.InverseTransformPoint(mouseWorld);

        float side = (playerAim != null && playerAim.FacingRight) ? 1f : -1f;
        Vector2 center = new Vector2(circleCenterOffset.x * side, circleCenterOffset.y);

        Vector2 delta = (Vector2)localMouse - center;
        if (delta.magnitude > circleRadius)
            delta = delta.normalized * circleRadius;

        Vector2 targetPos = center + delta;
        Vector2 newPos = Vector2.Lerp(transform.localPosition, targetPos, Time.deltaTime * aimLerpSpeed);
        transform.localPosition = new Vector3(newPos.x, newPos.y, transform.localPosition.z);

        float angle = Mathf.Atan2(newPos.y - center.y, newPos.x - center.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
