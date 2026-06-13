using UnityEngine;

/// <summary>
/// 玩家属性基础配置，可在 Inspector 或 ScriptableObject 资源中编辑。
/// </summary>
[CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "NanoPhysician/Player Stats Config")]
public class PlayerStatsConfig : ScriptableObject
{
    [Header("生存")]
    [Min(1)] public int maxHealth = 100;
    [Min(0)] public int defense = 0;

    [Header("射击")]
    [Min(1)] public int attackDamage = 15;
    [Min(0.05f)] public float fireCooldown = 0.25f;
    [Min(1f)] public float projectileSpeed = 14f;
    [Min(1f)] public float projectileRange = 7f;

    [Header("移动")]
    [Min(0.1f)] public float moveSpeed = 4f;

    public static PlayerStatsConfig CreateRuntimeDefault()
    {
        PlayerStatsConfig config = CreateInstance<PlayerStatsConfig>();
        config.name = "RuntimeDefaultPlayerStats";
        return config;
    }
}
