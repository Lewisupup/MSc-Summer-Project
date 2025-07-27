using System.Collections.Generic;
using UnityEngine;

public static class WeaponSelectionStore
{
    public static List<Weapon> SelectedWeapons = new List<Weapon>();
    public static List<WeaponData> SelectedWeaponsUI = new List<WeaponData>();

    
    public static void Clear()
    {
        SelectedWeapons.Clear();
        SelectedWeaponsUI.Clear();
    }
}
