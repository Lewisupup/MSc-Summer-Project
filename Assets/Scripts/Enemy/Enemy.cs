using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public int damage;
    public int health;
    private Transform player;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    // Force Well Control
    public bool isControlledByForceWell = false;
    private Vector3 forcedDirection = Vector3.zero;
    private float forcedSpeed = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

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
        // Debug.Log($"{gameObject.name} took {dmg} damage. Remaining HP: {health}");
        
        if (health <= 0)
            Destroy(gameObject);
            // Debug.Log("Enemy is destroyed!");
    }

    void OnTriggerEnter2D(Collider2D col)
    {   
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy hit the player!");
            col.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            // Optionally destroy self after hit
            // Destroy(gameObject);
        }
    }
}
