using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public Transform firePoint;

    public Weapon[] equippedWeapons;

    void Start()
    {
        // Convert the list from WeaponSelectionStore to an array
        equippedWeapons = WeaponSelectionStore.SelectedWeapons.ToArray();

        foreach (Weapon weapon in equippedWeapons)
        {
            if (weapon != null)
                weapon.ResetCooldown();
        }
    }

    void Update()
    {
        foreach (Weapon weapon in equippedWeapons)
        {
            if (weapon == null) continue;

            Transform origin = firePoint != null ? firePoint : transform;
            weapon.Fire(origin);
        }
    }
}
