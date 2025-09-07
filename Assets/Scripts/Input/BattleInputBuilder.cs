using System.Collections.Generic;
using UnityEngine;

public class BattleInputBuilder : MonoBehaviour
{
    public PlayerHealth player;
    public PlayerMovement playerMovement;
    public WeaponHolder weaponHolder;
    public EnemySpawner spawner;

    public int maxEnemies = 5;
    public float maxDistance = 20f;
    public int enemyTypeCount = 3; // e.g., 0 = A, 1 = B

    public float[] BuildInputVector()
    {
        List<float> input = new List<float>();

        // --- Player input ---
        float normalizedHP = player.CurrentHP / (float)player.MaxHP;
        Vector2 clampedVelocity = Vector2.ClampMagnitude(playerMovement.Velocity, 10f);
        float wallProximity = playerMovement.IsNearWall ? 1f : 0f;

        input.Add(normalizedHP);
        input.Add(clampedVelocity.x / 10f);
        input.Add(clampedVelocity.y / 10f);
        input.Add(wallProximity);

        // --- Weapon input (variable length) ---
        int weaponCount = weaponHolder.equippedWeapons.Length;
        foreach (Weapon weapon in weaponHolder.equippedWeapons)
        {
            float clampedCooldown = Mathf.Clamp01(weapon.CooldownRemaining / weapon.MaxCooldown);
            input.Add(clampedCooldown);
        }

        // --- Enemy input ---
        List<Enemy> enemies = spawner.GetClosestEnemies(player.transform.position, maxEnemies);
        int count = enemies.Count;

        for (int i = 0; i < count; i++)
        {
            Vector2 relPos = enemies[i].transform.position - player.transform.position;
            float distance = Mathf.Clamp(relPos.magnitude / maxDistance, 0f, 1f);
            float angle = Mathf.Atan2(relPos.y, relPos.x); // radians

            Vector2 relVel = Vector2.ClampMagnitude(enemies[i].Velocity, 10f);
            float vx = relVel.x / 10f;
            float vy = relVel.y / 10f;

            float[] oneHot = new float[enemyTypeCount];
            if (enemies[i].TypeIndex >= 0 && enemies[i].TypeIndex < enemyTypeCount)
                oneHot[enemies[i].TypeIndex] = 1f;

            float hpNorm = enemies[i].HP / enemies[i].MaxHP;

            input.Add(distance);
            input.Add(angle);
            input.Add(vx);
            input.Add(vy);
            input.AddRange(oneHot);
            input.Add(hpNorm);
        }

        // --- Padding if fewer than maxEnemies ---
        int perEnemySize = 4 + enemyTypeCount + 1;
        int totalExpectedSize = 4 + weaponCount + maxEnemies * perEnemySize;
        // Debug.Log($"[AI DEBUG] Current count: {input.Count}/{totalExpectedSize}");
        while (input.Count < totalExpectedSize)
            input.Add(0f);

        return input.ToArray();
    }
}
