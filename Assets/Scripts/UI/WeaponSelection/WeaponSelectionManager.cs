using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WeaponSelectionManager : MonoBehaviour
{
    public WeaponButton[] weaponButtons;
    public WeaponSlot[] weaponSlots;
    public Image weaponImage;
    public TMP_Text weaponText;
    public Button startBattleButton;

    private WeaponButton selectedButton;
    private WeaponData selectedWeapon;
    private Dictionary<WeaponSlot, WeaponData> slotAssignments = new Dictionary<WeaponSlot, WeaponData>();

    void Start()
    {
        foreach (var slot in weaponSlots)
        {
            slotAssignments[slot] = null;
        }

        startBattleButton.onClick.AddListener(StartBattle);

        UpdateStartButton();
    }

    public void SelectWeapon(WeaponButton button)
    {
        if (selectedButton != null)
            selectedButton.Deselect();

        selectedButton = button;
        selectedWeapon = button.weaponData;
        button.Select();

        ShowWeaponInfo(selectedWeapon);
    }

    public void ClearSelectedWeapon()
    {
        if (selectedButton != null)
            selectedButton.Deselect();

        selectedButton = null;
        selectedWeapon = null;
        ClearWeaponInfo();
    }

    public bool HasSelectedWeapon() => selectedWeapon != null;

    public void AssignWeaponToSlot(WeaponSlot slot)
    {
        if (!slot.IsEmpty() || selectedWeapon == null)
            return;

        slot.Equip(selectedWeapon);
        slotAssignments[slot] = selectedWeapon;

        foreach (var button in weaponButtons)
        {
            if (button.weaponData == selectedWeapon)
                button.SetInteractable(false);
        }

        ClearSelectedWeapon();
        UpdateStartButton();
    }

    public void UnassignWeaponFromSlot(WeaponSlot slot)
    {
        if (slot.IsEmpty())
            return;

        WeaponData removed = slotAssignments[slot];
        slot.Clear();
        slotAssignments[slot] = null;

        foreach (var button in weaponButtons)
        {
            if (button.weaponData == removed)
                button.SetInteractable(true);
        }

        UpdateStartButton();
    }

    public void ShowWeaponInfo(WeaponData data)
    {
        weaponImage.sprite = data.icon;
        weaponText.text = $"{data.weaponName}\n\n{data.description}";
        weaponImage.enabled = true;
    }

    public void ClearWeaponInfo()
    {
        weaponImage.enabled = false;
        weaponText.text = "";
    }

    private void UpdateStartButton()
    {
        int equipped = 0;
        foreach (var pair in slotAssignments)
        {
            if (pair.Value != null)
                equipped++;
        }

        startBattleButton.interactable = equipped == 3;
    }

    public void StartBattle()
    {
        WeaponSelectionStore.SelectedWeapons.Clear();
        WeaponSelectionStore.SelectedWeaponsUI.Clear();
        EnemyEncounterStore.Clear();

        foreach (var pair in slotAssignments)
        {
            if (pair.Value != null)
            {
                WeaponSelectionStore.SelectedWeapons.Add(pair.Value.weaponAsset);
                WeaponSelectionStore.SelectedWeaponsUI.Add(pair.Value);
            }
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }
}
