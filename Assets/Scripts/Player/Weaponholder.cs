// Scripts/Player/WeaponHolder.cs
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public Transform firePoint;
    public Weapon[] equippedWeapons;

    void Start()
    {
        foreach (Weapon weapon in equippedWeapons)
        {
            weapon.ResetCooldown();
        }
    }


    void Update()
    {
        foreach (Weapon weapon in equippedWeapons)
        {
            Transform origin = firePoint != null ? firePoint : transform;
            weapon.Fire(origin);
        }
    }
}
