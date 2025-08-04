using UnityEngine;

public class WeaponConfigLoader : MonoBehaviour
{
    public RadialBurst radialBurstPrefab; // Assign in Inspector (a prefab or asset with the script attached)

    void Awake()
    {
        radialBurstPrefab.LoadDefaultConfig();
        Debug.Log("[WeaponConfigLoader] RadialBurst config loaded on start.");
    }
}
