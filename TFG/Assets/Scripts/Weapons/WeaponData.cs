using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage;
    public float attackRate;
    public Sprite icon; // Para UI si lo necesitas
    public GameObject weaponPrefab; // Por si tiene una representación en el juego
}
