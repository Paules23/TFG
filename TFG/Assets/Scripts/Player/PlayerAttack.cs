using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform weaponPivot; 

    private WeaponData currentWeaponData;
    private MonoBehaviour currentWeapon; 
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

        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeaponByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeaponByIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeaponByIndex(2);
    }

    void TryAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentWeapon != null)
        {
            if (currentWeapon is SpearBehaviour spear)
            {
                spear.Attack(movement.FacingRight);
            }
            else if (currentWeapon is HeavySwordBehaviour sword)
            {
                sword.Attack(movement.FacingRight);
            }
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
        weaponInstance.transform.localPosition = weaponData.localPosition;
        weaponInstance.transform.localRotation = Quaternion.Euler(weaponData.localRotationEuler);

        currentWeapon = weaponInstance.GetComponent<MonoBehaviour>();
    }

    private void EquipWeaponByIndex(int index)
    {
        inventory.EquipWeaponByIndex(index);
        EquipWeapon(inventory.equippedWeapon);
    }
}
