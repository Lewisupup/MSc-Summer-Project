// Scripts/Weapons/Weapon.cs
using UnityEngine;

public abstract class Weapon : ScriptableObject
{
    public string weaponName;
    public float cooldown;
    public float damage;
    public Sprite icon;
    protected float lastFireTime;
    public int modeType = 0;

    public float MaxCooldown => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, (lastFireTime + cooldown) - Time.time);


    public virtual void SetMode(int mode)
    {
        modeType = mode;
    }
    
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
