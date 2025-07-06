using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int MaxHP = 100;
    public int CurrentHP;

    private void Start()
    {
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int dmg)
    {
        CurrentHP -= dmg;
        CurrentHP = Mathf.Max(0, CurrentHP); // Prevent negative HP

        Debug.Log("Player took damage: " + dmg + ", remaining: " + CurrentHP);

        if (CurrentHP <= 0)
        {
            Debug.Log("Game Over!");
            // Add game over logic
        }
    }
}
