using UnityEngine;

[CreateAssetMenu(menuName = "EnemyUI")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public string description;
    public Sprite icon;
}
