using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform weaponPivot; // Asignar el objeto SwordPivot en el Inspector

    private WeaponData currentWeaponData;
    private HeavySwordBehaviour currentWeapon;
    private PlayerMovement2D movement;
    private PlayerInventory inventory;

    void Start()
    {
        movement = GetComponent<PlayerMovement2D>();
        inventory = GetComponent<PlayerInventory>();
        EquipWeapon(inventory.equippedWeapon);
        
    }

    void Update()
    {
        TryAttack();
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.EquipWeaponByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.EquipWeaponByIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.EquipWeaponByIndex(2);
    }

    void TryAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentWeapon != null)
        {
            currentWeapon.Attack(movement.FacingRight);
        }
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        currentWeaponData = weaponData;

        GameObject weaponInstance = Instantiate(weaponData.weaponPrefab, weaponPivot);
        weaponInstance.transform.localPosition = Vector3.zero;

        currentWeapon = weaponInstance.GetComponent<HeavySwordBehaviour>();
    }
}
