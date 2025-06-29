// Scripts/Weapons/Weapon.cs
using UnityEngine;

public abstract class Weapon : ScriptableObject
{
    public string weaponName;
    public float cooldown;
    public float damage;
    public Sprite icon;

    protected float lastFireTime;

    public virtual void ResetCooldown()
    {
        lastFireTime = 0f;
    }


    public virtual bool CanFire()
    {
        return Time.time >= lastFireTime + cooldown;
    }

    public virtual void Fire(Transform firePoint)
    {
        if (CanFire())
        {
            lastFireTime = Time.time;
            PerformFire(firePoint);
        }
    }

    protected abstract void PerformFire(Transform firePoint);
}
