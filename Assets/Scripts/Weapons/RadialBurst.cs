// Scripts/Weapons/RadialBurst.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Radial Burst")]
public class RadialBurst : Weapon
{
    public GameObject projectilePrefab;
    public int bulletCount = 12;
    public float[] bulletSpeeds = new float[12]; // Customize per-bullet speeds

    protected override void PerformFire(Transform firePoint)
    {
        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(dirX, dirY).normalized;

            float speed = (i < bulletSpeeds.Length) ? bulletSpeeds[i] : 5f;

            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.damage = damage;
            bulletScript.Initialize(direction * speed);

            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            angle += angleStep;
        }
    }
}
