using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PistolBehaviour : MonoBehaviour
{
    [Header("Weapon Movement Bounds")]
    [Tooltip("Radio del círculo (zona roja) por donde se mueve el arma")]
    public float circleRadius = 1.5f;
    [Tooltip("Desplazamiento del centro del círculo en unidades locales (define la posición de referencia respecto al player)")]
    public Vector2 circleCenterOffset = new Vector2(0f, 0.6f);
    [Tooltip("Velocidad de interpolación para mover el arma (más alto = movimiento más ágil)")]
    public float aimLerpSpeed = 15f;

    [Header("Fire Settings")]
    public GameObject bulletPrefab;
    [Tooltip("Punto en el que se instancian las balas (debe estar dentro de la jerarquía del arma)")]
    public Transform muzzlePoint;
    public float bulletSpeed = 15f;
    public float fireRate = 5f;
    public float reloadTime = 1.5f;

    [Header("Ammo")]
    public int maxAmmo = 12;
    public float bulletDamage = 25f;
    public float bulletLifetime = 3f;

    [Header("Visual")]
    [Tooltip("Referencia al componente PlayerAim para saber la dirección (flip) del jugador")]
    public PlayerAim playerAim;
    [Tooltip("Referencias a los SpriteRenderer que componen el arma")]
    public SpriteRenderer[] graphics;

    // Variables para disparo
    int currentAmmo;
    bool isReloading;
    float nextFireTime;

    // Referencia al transform del jugador (padre de la pistola)
    private Transform playerTransform;

    void Start()
    {
        currentAmmo = maxAmmo;
        // Se asume que la pistola es hija del jugador.
        playerTransform = transform.parent;

        if (playerAim == null && playerTransform != null)
            playerAim = playerTransform.GetComponent<PlayerAim>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        UpdateWeaponPositionAndRotation();

        // Eliminado flip extra para evitar que se voltee en X al saltar.
        // Los gráficos se mantendrán sin modificación, pues la rotación del arma es suficiente.
    }

    /// <summary>
    /// Actualiza la posición y rotación del arma.
    /// Se calcula en el espacio local del jugador tomando la posición del ratón;
    /// se establece un centro de movimiento (desplazado según circleCenterOffset, invertido al girar)
    /// y se restringe el desplazamiento a un círculo de radio circleRadius.
    /// La rotación se ajusta para que el arma apunte desde ese centro hasta su posición.
    /// </summary>
    void UpdateWeaponPositionAndRotation()
    {
        // 1. Obtener la posición del ratón en mundo y convertirla al espacio local del player.
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 localMousePos = playerTransform.InverseTransformPoint(mouseWorld);

        // 2. Determinar el lado del jugador basado en PlayerAim.
        float playerSide = (playerAim != null && playerAim.FacingRight) ? 1f : -1f;
        // Ajustamos el centro según el lado.
        Vector2 adjustedCenter = new Vector2(circleCenterOffset.x * playerSide, circleCenterOffset.y);

        // 3. Calcular la posición deseada relativa al centro ajustado.
        Vector2 relativePos = ((Vector2)localMousePos - adjustedCenter);

        // 4. Limitar la magnitud al radio del círculo.
        if (relativePos.magnitude > circleRadius)
            relativePos = relativePos.normalized * circleRadius;

        // 5. La posición final en el espacio local es el centro ajustado más el desplazamiento.
        Vector2 candidatePos = adjustedCenter + relativePos;

        // 6. Interpolar suavemente hasta la posición deseada.
        Vector2 newLocalPos = Vector2.Lerp((Vector2)transform.localPosition, candidatePos, Time.deltaTime * aimLerpSpeed);
        transform.localPosition = new Vector3(newLocalPos.x, newLocalPos.y, transform.localPosition.z);

        // 7. Ajustar la rotación para que el arma apunte desde el centro hasta su posición.
        Vector2 direction = newLocalPos - adjustedCenter;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Llamado desde PlayerAttack al disparar.
    /// Gestiona la cadencia y recarga.
    /// </summary>
    public void TriggerFire()
    {
        if (isReloading || Time.time < nextFireTime) return;
        if (currentAmmo > 0) Shoot();
        else StartCoroutine(Reload());
    }

    /// <summary>
    /// Crea una bala en la posición de muzzlePoint.
    /// </summary>
    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector3 dir = muzzlePoint.TransformDirection(Vector3.up).normalized;
            rb.velocity = dir * bulletSpeed;
        }
        Bullet bs = bullet.GetComponent<Bullet>();
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
