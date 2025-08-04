using UnityEngine;
using System.IO;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapons/Radial Burst")]
public class RadialBurst : Weapon
{
    public GameObject projectilePrefab;
    public TextAsset fallbackConfigJson; // Optional: assign a default config in the Inspector

    private Dictionary<string, RadialBurstConfig> modeConfigs;

    public void LoadConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, "radial_config.json");
        string json;

        if (File.Exists(path))
        {
            json = File.ReadAllText(path);
        }
        // else if (fallbackConfigJson != null)
        // {
        //     json = fallbackConfigJson.text;
        // }
        else
        {
            Debug.LogWarning("No config file found. RadialBurst will not fire.");
            return;
        }

        RadialBurstModeData data = JsonUtility.FromJson<RadialBurstModeData>(json);
        // Convert list into Dictionary<string, RadialBurstConfig>
        modeConfigs = new Dictionary<string, RadialBurstConfig>();
        foreach (var entry in data.modes)
        {
            modeConfigs[entry.key] = entry.value;
        }
        Debug.Log($"✅ RadialBurst config loaded (mode count: {modeConfigs.Count})");
    }

    public void LoadDefaultConfig()
    {
        string json;
        json = fallbackConfigJson.text;
        RadialBurstModeData data = JsonUtility.FromJson<RadialBurstModeData>(json);
        // Convert list into Dictionary<string, RadialBurstConfig>
        modeConfigs = new Dictionary<string, RadialBurstConfig>();
        foreach (var entry in data.modes)
        {
            modeConfigs[entry.key] = entry.value;
        }
        Debug.Log($"✅ RadialBurst config loaded (mode count: {modeConfigs.Count})");

        // Refresh Application.persistentDataPath data
        string path = Path.Combine(Application.persistentDataPath, "radial_config.json");

        try
        {
            // Re-serialize from object to string
            string jsonOut = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, jsonOut);
            Debug.Log($"📄 Default config written to: {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to write config to disk: {ex.Message}");
        }
    }

    protected override void PerformFire(Transform firePoint)
    {
        if (modeConfigs == null || !modeConfigs.ContainsKey(modeType.ToString()))
        {
            Debug.LogWarning($"No config found for mode {modeType}. Aborting fire.");
            return;
        }

        RadialBurstConfig config = modeConfigs[modeType.ToString()];

        for (int i = 0; i < config.bulletCount; i++)
        {
            float angle = config.angles[i];
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            float speed = (i < config.speeds.Length) ? config.speeds[i] : 5f;

            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            var bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.damage = config.damage;
            bulletScript.Initialize(dir * speed);
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        damage = config.damage;
        cooldown = config.cooldown;
    }
}
