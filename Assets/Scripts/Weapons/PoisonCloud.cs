using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Poison Cloud")]
public class PoisonCloud : Weapon
{
    public GameObject cloudVFXPrefab;

    protected override void PerformFire(Transform firePoint)
    {
        // Configure based on mode
        float radius, angle;
        ConfigureMode(out radius, out angle);

        Vector2 firePos = firePoint.position;
        Vector2 aimDirection = GetBestDirection(firePos);

        float area = 0.5f * radius * radius * Mathf.Deg2Rad * angle;
        float actualDamage = damage / area;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, aimDirection);
        GameObject cloud = Instantiate(cloudVFXPrefab, firePos, rotation);
        cloud.transform.SetParent(firePoint);

        PoisonCloudEffect effect = cloud.GetComponent<PoisonCloudEffect>();
        effect.Initialize(radius, angle, actualDamage, firePoint.position, aimDirection);
    }

    private void ConfigureMode(out float radius, out float angle)
    {
        switch (modeType)
        {
            case 0: // Sniper Spray
                radius = 7.5f;
                angle = 45f;
                damage = 3600f;
                cooldown = 6f;
                break;

            case 1: // Wide Coverage AOE
                radius = 13.5f;
                angle = 180f;
                damage = 4670f;
                cooldown = 4.5f;
                break;

            case 2: // Balanced Burst
                radius = 10.5f;
                angle = 90f;
                damage = 7000f;
                cooldown = 3.6f;
                break;

            default:
                radius = 3f;
                angle = 90f;
                damage = 20f;
                cooldown = 3.0f;
                Debug.LogWarning($"Unknown modeType {modeType} in PoisonCloud. Using fallback config.");
                break;
        }
    }

    private Vector2 GetBestDirection(Vector2 origin)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, 5f); // Large radius for scanning
        Vector2 avgDirection = Vector2.zero;
        int count = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector2 dir = ((Vector2)hit.transform.position - origin).normalized;
                avgDirection += dir;
                count++;
            }
        }

        return count > 0 ? (avgDirection / count).normalized : Vector2.right;
    }
}
