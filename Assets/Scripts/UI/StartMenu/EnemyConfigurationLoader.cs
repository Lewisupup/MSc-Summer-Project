using UnityEngine;
using System.IO;

public class EnemyConfigLoader : MonoBehaviour
{
    public string resourcesJsonName = "Type1AI_default";
    public string persistentFileName = "enemy_Type1.json";

    void Awake()
    {
        CopyResourceToPersistent();
    }

    public void CopyResourceToPersistent()
    {
        TextAsset ta = Resources.Load<TextAsset>(resourcesJsonName);
        if (ta == null)
        {
            Debug.LogError($"❌ Resource file not found: Resources/{resourcesJsonName}.json");
            return;
        }

        string persistentBase = @"C:\Users\jiahui li\AppData\LocalLow\DefaultCompany\Summer Project draft";
        string path = Path.Combine(persistentBase, persistentFileName);

        try
        {
            File.WriteAllText(path, ta.text);
            Debug.Log($"✅ Copied {resourcesJsonName}.json to persistent path: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to write file: {e}");
        }
    }
}
