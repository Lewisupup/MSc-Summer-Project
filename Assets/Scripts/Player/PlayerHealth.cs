using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int MaxHP = 100;
    public int CurrentHP;

    [Header("Health Bar UI")]
    public GameObject healthBarContainer;
    public Image redBar;   // Foreground (HP left)
    public Image greyBar;  // Background (consumed HP)
    public float barVisibleDuration = 10f;
    private float hideTimer;

    [Header("HUD Health Bar (Top-Left)")]
    public Image hudRedBar;
    public Image hudGreyBar;
    public TextMeshProUGUI hudHPText;

    private void Start()
    {
        CurrentHP = MaxHP;
        UpdateHealthBar();

        // start hidden
        if (healthBarContainer != null)
            healthBarContainer.SetActive(false);
    }

    public void TakeDamage(int dmg)
    {
        CurrentHP -= dmg;
        CurrentHP = Mathf.Max(0, CurrentHP); // Prevent negative HP
        UpdateHealthBar();
        Debug.Log("Player took damage: " + dmg + ", remaining: " + CurrentHP);
        
        // show bar and reset hide timer
        if (healthBarContainer != null)
            healthBarContainer.SetActive(true);
        hideTimer = barVisibleDuration;

        if (CurrentHP <= 0)
        {
            Debug.Log("Game Over!");
            // Add game over logic
        }
    }

    private void Update()
    {
        if (healthBarContainer != null && healthBarContainer.activeSelf)
        {
            if (hideTimer > 0)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    healthBarContainer.SetActive(false); // hide after timer
                }
            }
        }
    }

    private void UpdateHealthBar()
    {
        float fillValue = (float)CurrentHP / MaxHP;
        // World bar
        if (redBar != null)
            redBar.fillAmount = fillValue;
        if (greyBar != null)
            greyBar.fillAmount = 1f; // stays full, acts as background


        // HUD bar
        if (hudRedBar != null)
            hudRedBar.fillAmount = fillValue;
        if (hudGreyBar != null)
            hudGreyBar.fillAmount = 1f;

        // HUD text
        if (hudHPText != null)
            hudHPText.text = $"HP: {CurrentHP}/{MaxHP}";
        
    }
}
