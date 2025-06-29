using System.Collections.Generic;
using UnityEngine;

public class ForceWellBehaviour : MonoBehaviour
{
    private float radius;
    private bool isPull;
    private float forceConstant;
    private float duration;
    private float startTime;

    public SpriteRenderer visualEffect; // Assign in prefab
    private Color pushColor = Color.white;
    private Color pullColor = Color.black;

    private HashSet<Enemy> affectedEnemies = new HashSet<Enemy>();

    public void Initialize(float radius, bool isPull, float forceConstant, float duration)
    {
        this.radius = radius;
        this.isPull = isPull;
        this.forceConstant = forceConstant;
        this.duration = duration;
        startTime = Time.time;

        if (visualEffect != null)
        {
            visualEffect.color = isPull ? pullColor : pushColor;
            visualEffect.transform.localScale = Vector3.one * radius * 2f;
        }
    }

    private void Update()
    {
        if (Time.time > startTime + duration)
        {
            Destroy(gameObject);
            return;
        }

        ApplyForceToEnemies();
    }

    private void OnDestroy()
    {
        foreach (var enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                enemy.ReleaseForcedMovement();
            }
        }
        affectedEnemies.Clear();
    }

    void ApplyForceToEnemies()
    {
        // Track enemies that are currently in the radius this frame
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        HashSet<Enemy> currentFrameEnemies = new HashSet<Enemy>();

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    currentFrameEnemies.Add(enemy);

                    Vector3 dir = (transform.position - enemy.transform.position).normalized;
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (!isPull) dir = -dir;

                    float strength;

                    if (isPull)
                    {
                        // Pull: Stronger near center (closer = stronger)
                        strength = forceConstant * (1 - distance / radius);
                    }
                    else
                    {
                        // Push: Stronger when farther from center (farther = stronger)
                        strength = forceConstant * (distance / radius);
                    }

                    strength = Mathf.Clamp(strength, 0f, forceConstant);

                    enemy.SetForcedMovement(dir, strength);
                    affectedEnemies.Add(enemy);
                }
            }
        }

        // Any enemy that was affected before, but is not in range anymore
        var enemiesToRemove = new List<Enemy>();
        foreach (var enemy in affectedEnemies)
        {
            if (!currentFrameEnemies.Contains(enemy))
            {
                if (enemy != null)
                    enemy.ReleaseForcedMovement();

                enemiesToRemove.Add(enemy);
            }
        }

        // Clean up removed ones from the tracking set
        foreach (var enemy in enemiesToRemove)
        {
            affectedEnemies.Remove(enemy);
        }
    }
}
