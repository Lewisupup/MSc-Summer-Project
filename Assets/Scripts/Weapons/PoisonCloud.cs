using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Poison Cloud")]
public class PoisonCloud : Weapon
{
    public float radius = 3f;
    public float angle = 90f;
    public GameObject cloudVFXPrefab;

    protected override void PerformFire(Transform firePoint)
    {
        Vector2 firePos = firePoint.position;
        Vector2 aimDirection = GetBestDirection(firePos);
        float area = 0.5f * radius * radius * Mathf.Deg2Rad * angle;
        float actualDamage = damage / area;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, aimDirection);
        GameObject cloud = Instantiate(cloudVFXPrefab, firePos, rotation);
        cloud.transform.SetParent(firePoint); // Attach to weapon/hand/body
        PoisonCloudEffect effect = cloud.GetComponent<PoisonCloudEffect>();
        effect.Initialize(radius, angle, actualDamage, firePoint.position, aimDirection);
    }

    private Vector2 GetBestDirection(Vector2 origin)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);
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
