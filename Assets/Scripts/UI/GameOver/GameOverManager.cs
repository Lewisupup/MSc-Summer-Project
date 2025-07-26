using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    [Header("Weapon UI")]
    public Transform weaponContentParent; // Parent for weapon items (inside ScrollView Content)
    public GameObject weaponItemPrefab;   // Prefab for each weapon row

    [Header("Enemy UI")]
    public Transform enemyContentParent;  // Parent for enemy items
    public GameObject enemyItemPrefab;    // Prefab for each enemy row

    [Header("Buttons")]
    public Button backButton; // Goes back to start menu

    private void Start()
    {
        // Example data – Replace with your real references
        List<WeaponData> selectedWeapons = WeaponSelectionStore.SelectedWeaponsUI;
        List<EnemyData> encounteredEnemies = EnemyEncounterStore.EncounteredEnemies;

        // Debug Weapons
        Debug.Log($"[GameOver] SelectedWeaponsUI count: {selectedWeapons.Count}");
        for (int i = 0; i < selectedWeapons.Count; i++)
        {
            Debug.Log($"[GameOver] Weapon {i + 1}: {selectedWeapons[i].weaponName}");
        }

        // Debug Enemies
        Debug.Log($"[GameOver] EncounteredEnemies count: {encounteredEnemies.Count}");
        for (int i = 0; i < encounteredEnemies.Count; i++)
        {
            Debug.Log($"[GameOver] Enemy {i + 1}: {encounteredEnemies[i].enemyName}");
        }

        PopulateWeaponPanel(selectedWeapons);
        PopulateEnemyPanel(encounteredEnemies);

        backButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
        });
    }

    void PopulateWeaponPanel(List<WeaponData> weapons)
    {
        foreach (Transform child in weaponContentParent) Destroy(child.gameObject);
        foreach (var weapon in weapons)
        {
            GameObject item = Instantiate(weaponItemPrefab, weaponContentParent);
            item.transform.Find("Icon").GetComponent<Image>().sprite = weapon.icon;
            item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = weapon.weaponName;
            item.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = weapon.description;
        }
    }

    void PopulateEnemyPanel(List<EnemyData> enemies)
    {
        foreach (Transform child in enemyContentParent) Destroy(child.gameObject);
        foreach (var enemy in enemies)
        {
            GameObject item = Instantiate(enemyItemPrefab, enemyContentParent);
            item.transform.Find("Icon").GetComponent<Image>().sprite = enemy.icon;
            item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = enemy.enemyName;
            item.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = enemy.description;
        }
    }
}
