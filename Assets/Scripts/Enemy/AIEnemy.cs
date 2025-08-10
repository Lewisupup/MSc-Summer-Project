using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class AIEnemy : Enemy
{
    [System.Serializable]
    public class MoveInstruction
    {
        public float dx;
        public float dy;
        public float speed;
        public float duration;
    }

    [System.Serializable]
    public class MoveInstructionList
    {
        public List<MoveInstruction> instructions;
    }

    private List<MoveInstruction> moveSequence = new();
    private int currentIndex = 0;
    private Vector3 currentMovement = Vector3.zero;

    public string resourcesJsonName = "Type1AI_default";

    protected override void Start()
    {
        base.Start(); // keep base setup
        LoadMovementFromPersistent(); // custom AI stuff
    }

    public void LoadMovementPatternFromResources()
    {
        var ta = Resources.Load<TextAsset>(resourcesJsonName);
        if (ta != null)
        {
            LoadMovementFromJson(ta.text);
            Debug.Log($"AIEnemy: Loaded movement from Resources/{resourcesJsonName}.json");
        }
        else
        {
            Debug.LogError($"AIEnemy: Resources/{resourcesJsonName}.json not found");
        }
    }

    public void LoadMovementFromPersistent()
    {
        // Full exact path
        string persistentBase = @"C:\Users\jiahui li\AppData\LocalLow\DefaultCompany\Summer Project draft";
        string fileName = "enemy_Type1.json";
        string path = Path.Combine(persistentBase, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"AIEnemy: Persistent JSON not found: {path}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            LoadMovementFromJson(json);
            Debug.Log($"[AIEnemy] JSON loaded from {path}:\n{json}");
            Debug.Log($"AIEnemy: Loaded movement from persistent: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AIEnemy: Failed reading {path}: {e}");
        }
    }


    public void LoadMovementFromJson(string json)
    {
        MoveInstructionList data = JsonUtility.FromJson<MoveInstructionList>(json);
        moveSequence = data.instructions;
        StartCoroutine(PlayMovementSequence());
    }

    IEnumerator PlayMovementSequence()
    {
        while (true)
        {
            if (moveSequence.Count == 0) yield break;

            MoveInstruction instr = moveSequence[currentIndex];
            float timePassed = 0f;

            while (timePassed < instr.duration)
            {
                // Base direction toward player
                Vector2 toPlayer = (player.position - transform.position).normalized;

                // Combine base direction with dx/dy offsets
                Vector2 worldDir = toPlayer * instr.dy +   // forward/back component
                            new Vector2(-toPlayer.y, toPlayer.x) * instr.dx; // strafe component

                currentMovement = worldDir.normalized * instr.speed;

                timePassed += Time.deltaTime;
                yield return null;
            }

            currentIndex = (currentIndex + 1) % moveSequence.Count;
        }
    }


    // ✅ Simplified and safe: respect force flag from base class
    protected override Vector3 GetMovementVector()
    {
        if (isControlledByForceWell)
            return base.GetMovementVector(); // returns forced movement from Enemy.cs

        return currentMovement;
    }
}
