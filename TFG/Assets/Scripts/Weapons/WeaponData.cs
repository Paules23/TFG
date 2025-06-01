using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Spawn Offset")]
    public Vector3 localPosition;
    public Vector3 localRotationEuler;

    public string weaponName;
    public float damage;
    public float attackRate;
    public Sprite icon;
    public GameObject weaponPrefab;
}
