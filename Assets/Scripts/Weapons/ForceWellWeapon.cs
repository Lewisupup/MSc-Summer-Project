// Scripts/Weapons/ForceWellWeapon.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Force Well Weapon")]
public class ForceWellWeapon : Weapon
{
    [Header("Force Well Settings")]
    public GameObject wellPrefab;
    public float duration = 3f;
    public float radius = 5f;
    public bool isPull = true;
    public float forceConstant = 10f;

    [Header("Spawn Logic")]
    public float detectionRadius = 10f;

    protected override void PerformFire(Transform firePoint)
    {
        Vector2 spawnPos = firePoint.position;

        // Find nearby enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(firePoint.position, detectionRadius);
        Vector2 sum = Vector2.zero;
        int count = 0;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                sum += (Vector2)hit.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            spawnPos = sum / count; // spawn at average enemy position
        }

        GameObject well = Instantiate(wellPrefab, spawnPos, Quaternion.identity);
        var behavior = well.GetComponent<ForceWellBehaviour>();
        if (behavior != null)
        {
            behavior.Initialize(radius, isPull, forceConstant, duration);
        }
    }
}
