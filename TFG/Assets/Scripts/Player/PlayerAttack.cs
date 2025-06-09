using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Pivot para instanciar el arma")]
    public Transform weaponPivot;

    private WeaponData currentWeaponData;
    private MonoBehaviour currentWeapon;      // Guardaremos ahí la referencia al script concreto (HeavySwordBehaviour o SpearBehaviour)
    private PlayerMovement2D movement;
    private PlayerInventory inventory;
    private PlayerAim aim;                   // Para saber hacia dónde mira el ratón

    void Start()
    {
        movement = GetComponent<PlayerMovement2D>();
        inventory = GetComponent<PlayerInventory>();
        aim = GetComponent<PlayerAim>();

        EquipWeapon(inventory.equippedWeapon);
    }

    void Update()
    {
        TryAttack();

        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeaponByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeaponByIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeaponByIndex(2);
    }

    private void TryAttack()
    {
        if (Input.GetMouseButtonDown(0) && currentWeapon != null)
        {
            // Determinamos “facingRight” en base al PlayerAim (donde está el ratón), no a movement.FacingRight
            bool facingRight = aim != null && aim.FacingRight;

            // Si currentWeapon es una SpearBehaviour, llamamos a spear.Attack(...)
            if (currentWeapon is SpearBehaviour spear)
            {
                spear.Attack(facingRight);
            }
            // Si currentWeapon es una HeavySwordBehaviour, llamamos a sword.Attack(...)
            else if (currentWeapon is HeavySwordBehaviour sword)
            {
                sword.Attack(facingRight);
            }
            // En el futuro podremos añadir otros tipos (por ejemplo, PistolBehaviour) aquí
        }
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null) return;

        // Destruimos la instancia anterior
        if (currentWeapon != null)
        {
            Destroy(((Component)currentWeapon).gameObject);
            currentWeapon = null;
        }

        currentWeaponData = weaponData;

        // Instanciamos el prefab y lo anclamos a weaponPivot
        GameObject weaponInstance = Instantiate(
            weaponData.weaponPrefab,
            weaponPivot
        );

        // Ajustamos posición/rotación según lo que tengamos en WeaponData (si existe)
        weaponInstance.transform.localPosition = weaponData.localPosition;
        weaponInstance.transform.localRotation = Quaternion.Euler(weaponData.localRotationEuler);

        // Obtenemos el script concreto que hereda de MonoBehaviour (HeavySwordBehaviour, SpearBehaviour, etc.)
        currentWeapon = weaponInstance.GetComponent<MonoBehaviour>();

        if (currentWeapon == null)
        {
            Debug.LogWarning("PlayerAttack: El prefab de arma no contiene un script que herede de MonoBehaviour.");
        }
    }

    private void EquipWeaponByIndex(int index)
    {
        inventory.EquipWeaponByIndex(index);
        EquipWeapon(inventory.equippedWeapon);
    }
}
