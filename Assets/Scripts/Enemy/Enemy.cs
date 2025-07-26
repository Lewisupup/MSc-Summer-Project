using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyUIData;
    public float speed;
    public int damage;
    public int health;
    public float MaxHP = 100f;
    public int TypeIndex = 0;

    public float minX, maxX, minY, maxY;

    public Vector2 Velocity;
    public EnemySpawner spawner;

    private Transform player;
    private bool isAddedToEncounterStore = false;

    // Force Well Control
    public bool isControlledByForceWell = false;
    private Vector3 forcedDirection = Vector3.zero;
    private float forcedSpeed = 0f;

    private float playerDamageReceivedThisFrame = 0f;

    public float HP => health;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Add this enemy's data to EncounterStore (only once)
        if (enemyUIData != null && !isAddedToEncounterStore)
        {
            EnemyEncounterStore.AddEnemyEncounter(enemyUIData);
            isAddedToEncounterStore = true;
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 oldPos = transform.position;

        Vector3 movement;

        if (isControlledByForceWell)
        {
            movement = forcedDirection * forcedSpeed;
        }
        else
        {
            Vector3 dir = (player.position - transform.position).normalized;
            movement = dir * speed;
        }

        transform.position += movement * Time.deltaTime;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;

        Velocity = (transform.position - oldPos) / Time.deltaTime;
    }

    public void SetForcedMovement(Vector3 direction, float speed)
    {
        isControlledByForceWell = true;
        forcedDirection = direction;
        forcedSpeed = speed;
    }

    public void ReleaseForcedMovement()
    {
        isControlledByForceWell = false;
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        playerDamageReceivedThisFrame += dmg;

        if (health <= 0)
        {
            ReportFinalDamageToSpawner();
            spawner.TotalEnemiesKilled += 1;
            Destroy(gameObject);
        }
    }

    public float ConsumePlayerDamageThisFrame()
    {
        float dmg = playerDamageReceivedThisFrame;
        playerDamageReceivedThisFrame = 0f;
        return dmg;
    }

    private void ReportFinalDamageToSpawner()
    {
        if (spawner != null)
        {
            spawner.RegisterDamageBeforeDeath(playerDamageReceivedThisFrame);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.activeEnemies.Remove(this);
        }
    }
}
