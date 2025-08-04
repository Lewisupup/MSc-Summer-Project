// Scripts/Weapons/Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public float maxLifeTime = 5f;
    private Vector2 velocity;

    public AudioClip fireSound;
    private AudioSource audioSource;

    // Called by the weapon when firing the bullet
    public void Initialize(Vector2 initialVelocity)
    {
        velocity = initialVelocity;
        
        // 🔊 Play fire sound here
        audioSource = GetComponent<AudioSource>();
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        Destroy(gameObject, maxLifeTime); // auto-destroy if it lives too long
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(Mathf.RoundToInt(damage));
                int dmg = Mathf.RoundToInt(damage);
                // Debug.Log($"Bullet hit enemy! Dealt {dmg} damage to {enemy.name}");
            }

            Destroy(gameObject); // bullet disappears on hit
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject); // bullet disappears off screen
    }
}
