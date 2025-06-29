using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;               // Assign your Player here
    public GameObject enemyTypeA;          // Assign prefab in Inspector
    public GameObject enemyTypeB;          // Assign prefab in Inspector

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public float spawnRadius = 10f;
    public float groupRadius = 1.5f;       // Distance between enemies in a group
    public int numberOfGroups = 3;
    public int enemiesPerGroup = 3;
    public float groupSpawnDelay = 0.5f;
    public float waveDelay = 5f;

    void Start()
    {
        // Start spawning waves repeatedly
        InvokeRepeating(nameof(SpawnWave), 1f, waveDelay);
    }

    void SpawnWave()
    {
        StartCoroutine(SpawnWaveStaggered());
    }

    IEnumerator SpawnWaveStaggered()
    {
        for (int i = 0; i < numberOfGroups; i++)
        {
            SpawnEnemyGroup();
            yield return new WaitForSeconds(groupSpawnDelay);
        }
    }

    void SpawnEnemyGroup()
    {
        if (player == null) return;

        // Randomly choose which type of enemy this group is
        GameObject enemyPrefab = Random.value < 0.5f ? enemyTypeA : enemyTypeB;

        // Pick a spawn direction and get spawn center at the edge
        Vector2 spawnCenter = GetSpawnPointAroundPlayer(player.position, spawnRadius);

        // Spawn multiple enemies close together (cluster)
        for (int i = 0; i < enemiesPerGroup; i++)
        {
            Vector2 offset = Random.insideUnitCircle * groupRadius;
            Vector2 spawnPos = spawnCenter + offset;
            spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
            spawnPos.y = Mathf.Clamp(spawnPos.y, minY, maxY);

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector2 GetSpawnPointAroundPlayer(Vector3 playerPos, float radius)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        return (Vector2)playerPos + dir * radius;
    }
}
