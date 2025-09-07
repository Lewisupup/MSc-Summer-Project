using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Force Well Weapon")]
public class ForceWellWeapon : Weapon
{
    [Header("Force Well Settings")]
    public GameObject wellPrefab;
    
    [HideInInspector]
    public float duration;
    public float radius;
    public bool isPull;
    public float forceConstant;

    [Header("Spawn Logic")]
    public float detectionRadius;

    protected override void PerformFire(Transform firePoint)
    {
        // Set parameters based on mode
        ConfigureMode();

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

    private void ConfigureMode()
    {
        switch (modeType)
        {
            case 0: // Black Hole (Strong Pull, Short Duration)
                radius = 0.8f;
                duration = 2.5f;
                forceConstant = 32f;
                isPull = true;
                detectionRadius = 8f;
                damage = 0f;
                cooldown = 8f;
                break;

            case 1: // Gravity Field (Weak Pull, Long Duration)
                radius = 1.4f;
                duration = 4f;
                forceConstant = 16f;
                isPull = true;
                detectionRadius = 12f;
                damage = 0f;
                cooldown = 8f;
                break;

            case 2: // Kinetic Blast (Push)
                radius = 0.6f;
                duration = 2f;
                forceConstant = 32f;
                isPull = false;
                detectionRadius = 8f;
                damage = 0f;
                cooldown = 8f;
                break;

            default:
                radius = 5f;
                duration = 3f;
                forceConstant = 10f;
                isPull = true;
                detectionRadius = 10f;
                damage = 0f;
                cooldown = 10f;
                Debug.LogWarning($"Unknown modeType {modeType} in ForceWellWeapon. Using fallback config.");
                break;
        }
    }
}
