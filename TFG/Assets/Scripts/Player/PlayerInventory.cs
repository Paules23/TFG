using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<WeaponData> weapons = new List<WeaponData>();
    public WeaponData equippedWeapon;

    public void EquipWeapon(WeaponData weapon)
    {
        if (weapons.Contains(weapon))
        {
            equippedWeapon = weapon;
            Debug.Log("Equipada arma: " + weapon.weaponName);
        }
    }

    // Solo para test rápido desde el editor (opcional)
    public void EquipWeaponByIndex(int index)
    {
        if (index >= 0 && index < weapons.Count)
        {
            EquipWeapon(weapons[index]);
        }
    }
}
