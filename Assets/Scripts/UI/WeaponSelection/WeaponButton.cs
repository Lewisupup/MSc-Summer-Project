using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponButton : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image icon;             // Drag your child Image here (weapon sprite)
    public Image background;       // Drag root button Image here

    [Header("Data")]
    public WeaponData weaponData;

    private Button button;
    private WeaponSelectionManager selectionManager;
    private bool isSelected = false;

    private void Start()
    {
        button = GetComponent<Button>();
        selectionManager = FindObjectOfType<WeaponSelectionManager>();
        button.onClick.AddListener(ToggleSelect);
    }

    private void ToggleSelect()
    {
        if (!button.interactable)
            return;

        if (isSelected)
        {
            Deselect();
            selectionManager.ClearSelectedWeapon();
        }
        else
        {
            selectionManager.SelectWeapon(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selectionManager.ShowWeaponInfo(weaponData);
    }

    public void Select()
    {
        isSelected = true;
        background.color = Color.green;
    }

    public void Deselect()
    {
        isSelected = false;
        background.color = Color.white;
    }

    public void SetInteractable(bool value)
    {
        button.interactable = value;
        background.color = value ? Color.white : Color.gray;
        isSelected = false;
    }
}
