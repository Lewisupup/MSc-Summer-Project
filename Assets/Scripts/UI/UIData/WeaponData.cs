using UnityEngine;

[CreateAssetMenu(menuName = "WeaponUI")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public string description;
    public Sprite icon;
    
    public Weapon weaponAsset;
}
