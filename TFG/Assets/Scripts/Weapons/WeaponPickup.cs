using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Arma que se desbloquea")]
    public WeaponData weaponToUnlock;

    [Tooltip("Tag del jugador")]
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        PlayerAttack attack = other.GetComponent<PlayerAttack>();

        if (inventory == null || attack == null)
        {
            Debug.LogWarning("WeaponPickup: El jugador no tiene PlayerInventory o PlayerAttack.");
            return;
        }

        // Añadir el arma al inventario
        bool added = inventory.UnlockWeapon(weaponToUnlock);

        // Si fue añadida (no ya desbloqueada), la equipa
        if (added)
        {
            inventory.EquipWeaponDirectly(weaponToUnlock);
            attack.EquipWeapon(weaponToUnlock);
        }

        Destroy(gameObject); // Elimina el pickup
    }
}
