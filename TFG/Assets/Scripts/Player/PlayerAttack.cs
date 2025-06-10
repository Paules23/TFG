using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Pivot para instanciar el arma")]
    public Transform weaponPivot;

    private WeaponData currentWeaponData;
    private MonoBehaviour currentWeapon;
    private PlayerMovement2D movement;
    private PlayerInventory inventory;
    private PlayerAim aim;

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
            bool facingRight = aim != null && aim.FacingRight;

            if (currentWeapon is SpearBehaviour spear)
                spear.Attack(facingRight);
            else if (currentWeapon is HeavySwordBehaviour sword)
                sword.Attack(facingRight);
            else if (currentWeapon is PistolBehaviour pistol)
                pistol.TriggerFire();
        }
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (currentWeapon != null)
            Destroy(((Component)currentWeapon).gameObject);
        if (weaponData == null) return;

        currentWeaponData = weaponData;
        GameObject weaponInstance = Instantiate(weaponData.weaponPrefab, weaponPivot);
        weaponInstance.transform.localPosition = weaponData.localPosition;
        weaponInstance.transform.localRotation = Quaternion.Euler(weaponData.localRotationEuler);
        currentWeapon = weaponInstance.GetComponent<MonoBehaviour>();
        if (currentWeapon == null)
            Debug.LogWarning("PlayerAttack: prefab sin script MonoBehaviour.");
    }

    private void EquipWeaponByIndex(int index)
    {
        inventory.EquipWeaponByIndex(index);
        EquipWeapon(inventory.equippedWeapon);
    }
}
