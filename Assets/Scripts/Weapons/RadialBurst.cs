// Scripts/Weapons/RadialBurst.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Radial Burst")]
public class RadialBurst : Weapon
{
    public GameObject projectilePrefab;

    protected override void PerformFire(Transform firePoint)
    {
        int actualBulletCount;
        float[] speeds;

        // Get bullet behavior based on mode
        ConfigureMode(out actualBulletCount, out speeds);

        float angleStep = 360f / actualBulletCount;
        float angle = 0f;

        for (int i = 0; i < actualBulletCount; i++)
        {
            float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(dirX, dirY).normalized;

            float speed = (i < speeds.Length) ? speeds[i] : 5f;

            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.damage = damage;
            bulletScript.Initialize(direction * speed);

            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            angle += angleStep;
        }
    }

    private void ConfigureMode(out int count, out float[] speeds)
    {
        switch (modeType)
        {
            case 0:
                count = 6;
                speeds = new float[count];
                for (int i = 0; i < count; i++) speeds[i] = 30f;
                damage = 25f;
                cooldown = 2f;
                break;

            case 1:
                count = 16;
                speeds = new float[count];
                for (int i = 0; i < count; i++) speeds[i] = 10f;
                damage = 8f;
                cooldown = 1.5f;
                break;

            case 2:
                count = 8;
                speeds = new float[count];
                for (int i = 0; i < count; i++) speeds[i] = 15f;
                damage = 5f;
                cooldown = 0.8f;
                break;

            default:
                // Fallback values to satisfy compiler
                count = 1;
                speeds = new float[1] { 10f };
                damage = 10f;
                cooldown = 1.0f;
                Debug.LogWarning($"Unknown modeType {modeType} in RadialBurst. Using fallback config.");
                break;
    }
}

}
