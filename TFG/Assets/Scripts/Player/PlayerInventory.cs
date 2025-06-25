using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public WeaponData[] allWeapons;  // Lista completa de armas disponibles en orden
    public WeaponData equippedWeapon;

    private bool[] unlocked; // Un array paralelo a allWeapons

    void Awake()
    {
        unlocked = new bool[allWeapons.Length];
    }

    public bool UnlockWeapon(WeaponData weapon)
    {
        Debug.Log($"Intentando desbloquear arma: {weapon.name}");

        int index = System.Array.IndexOf(allWeapons, weapon);

        if (index == -1)
        {
            Debug.LogWarning($"[UnlockWeapon] Arma {weapon.name} no encontrada en el array 'allWeapons'.");
            return false;
        }

        if (unlocked[index])
        {
            Debug.Log($"[UnlockWeapon] El arma {weapon.name} ya estaba desbloqueada.");
            return false;
        }

        unlocked[index] = true;
        Debug.Log($"[UnlockWeapon] Arma {weapon.name} desbloqueada correctamente en el índice {index}.");

        return true;
    }


    public void EquipWeaponByIndex(int index)
    {
        if (index >= 0 && index < allWeapons.Length && unlocked[index])
        {
            equippedWeapon = allWeapons[index];
        }
    }

    public void EquipWeaponDirectly(WeaponData weapon)
    {
        int index = System.Array.IndexOf(allWeapons, weapon);
        if (index != -1 && unlocked[index])
        {
            equippedWeapon = weapon;
        }
    }
}
