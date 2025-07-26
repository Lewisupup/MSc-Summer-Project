using System.Collections.Generic;

public static class EnemyEncounterStore
{
    public static List<EnemyData> EncounteredEnemies = new List<EnemyData>();

    public static void AddEnemyEncounter(EnemyData enemyData)
    {
        if (!EncounteredEnemies.Contains(enemyData))
        {
            EncounteredEnemies.Add(enemyData);
        }
    }

    public static void Clear()
    {
        EncounteredEnemies.Clear();
    }
}
