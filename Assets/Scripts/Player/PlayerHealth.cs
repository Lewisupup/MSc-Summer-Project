using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("Player took damage: " + dmg + ", remaining: " + health);

        if (health <= 0)
        {
            Debug.Log("Game Over!");
            // Add game over logic
        }
    }
}
