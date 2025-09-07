using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject enemyTypeA;
    public GameObject enemyTypeB;
    public GameObject enemyType1AI;

    public GameObject redCrossPrefab; 
    public float crossLifetime = 2f;  


    public float minX, maxX, minY, maxY;
    public float spawnRadius = 10f;
    public float groupRadius = 1.5f;
    public int numberOfGroups = 3;
    public int enemiesPerGroup = 3;
    public float groupSpawnDelay = 0.5f;
    public float waveDelay = 5f;

    public AudioClip enemySpawnClip;
    private AudioSource audioSource;


    [HideInInspector] public List<Enemy> activeEnemies = new List<Enemy>();
    public int TotalEnemiesKilled = 0;
    private float pendingDamageFromDeadEnemies = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        InvokeRepeating(nameof(SpawnWave), 1f, waveDelay);
    }

    void SpawnWave()
    {
        if (enemySpawnClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(enemySpawnClip);
        }
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

        int typeIndex = Random.Range(0, 3); 
        // int typeIndex = 2;
        GameObject enemyPrefab =
            (typeIndex == 0) ? enemyTypeA :
            (typeIndex == 1) ? enemyTypeB :
            enemyType1AI;

        Vector2 spawnCenter = GetSpawnPointAroundPlayer(player.position, spawnRadius);

        // Spawn the visual marker
        if (redCrossPrefab != null)
        {
            GameObject cross = Instantiate(redCrossPrefab, spawnCenter, Quaternion.identity);
            Destroy(cross, crossLifetime); 
        }

        for (int i = 0; i < enemiesPerGroup; i++)
        {
            Vector2 offset = Random.insideUnitCircle * groupRadius;
            Vector2 spawnPos = spawnCenter + offset;
            spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
            spawnPos.y = Mathf.Clamp(spawnPos.y, minY, maxY);

            GameObject obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TypeIndex = typeIndex;
                enemy.spawner = this; 
                activeEnemies.Add(enemy);
            }
        }
    }

    Vector2 GetSpawnPointAroundPlayer(Vector3 playerPos, float radius)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        return (Vector2)playerPos + dir * radius;
    }

    public List<Enemy> GetClosestEnemies(Vector3 playerPos, int maxCount)
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies
            .OrderBy(e => (e.transform.position - playerPos).sqrMagnitude)
            .Take(maxCount)
            .ToList();
    }

    public void RegisterDamageBeforeDeath(float damage)
    {
        pendingDamageFromDeadEnemies += damage;
    }

    public float ConsumePendingDamage()
    {
        float dmg = pendingDamageFromDeadEnemies;
        pendingDamageFromDeadEnemies = 0f;
        return dmg;
    }
    
}
