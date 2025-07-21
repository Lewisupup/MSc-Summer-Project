using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WeaponSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image baseImage;  // Shown when no weapon equipped
    public Image icon;       // Shown when weapon is equipped

    [Header("State")]
    public WeaponData equippedWeapon;

    private WeaponSelectionManager selectionManager;

    private void Start()
    {
        selectionManager = FindObjectOfType<WeaponSelectionManager>();
        GetComponent<Button>().onClick.AddListener(OnClick); // Register click

        UpdateVisual(); // Show correct image on start
    }

    private void OnClick()
    {
        if (equippedWeapon != null)
        {
            selectionManager.UnassignWeaponFromSlot(this);
        }
        else if (selectionManager.HasSelectedWeapon())
        {
            selectionManager.AssignWeaponToSlot(this);
        }
    }

    public void Equip(WeaponData weapon)
    {
        equippedWeapon = weapon;
        icon.sprite = weapon.icon;
        UpdateVisual();
    }

    public void Clear()
    {
        equippedWeapon = null;
        UpdateVisual();
    }

    public bool IsEmpty() => equippedWeapon == null;

    private void UpdateVisual()
    {
        bool hasWeapon = equippedWeapon != null;
        baseImage.enabled = !hasWeapon;
        icon.enabled = hasWeapon;
    }
}
