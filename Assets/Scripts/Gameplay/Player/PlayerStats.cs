using System;
using UnityEngine;

/// <summary>
/// 玩家运行时属性：从配置读取基础值，叠加升级加成，供战斗系统查询。
/// </summary>
[RequireComponent(typeof(Health))]
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStatsConfig config;

    [Header("升级加成（运行时累加）")]
    [SerializeField] private int bonusMaxHealth;
    [SerializeField] private int bonusDefense;
    [SerializeField] private int bonusAttackDamage;
    [SerializeField] private float bonusFireCooldown;
    [SerializeField] private float bonusProjectileSpeed;
    [SerializeField] private float bonusProjectileRange;
    [SerializeField] private float bonusMoveSpeed;

    private Health health;
    private PlayerController playerController;

    public int MaxHealth => Config.maxHealth + bonusMaxHealth;
    public int Defense => Config.defense + bonusDefense;
    public int AttackDamage => Config.attackDamage + bonusAttackDamage;
    public float FireCooldown => Mathf.Max(0.05f, Config.fireCooldown + bonusFireCooldown);
    public float ProjectileSpeed => Mathf.Max(1f, Config.projectileSpeed + bonusProjectileSpeed);
    public float ProjectileRange => Mathf.Max(1f, Config.projectileRange + bonusProjectileRange);
    public float MoveSpeed => Mathf.Max(0.1f, Config.moveSpeed + bonusMoveSpeed);

    public PlayerStatsConfig Config
    {
        get
        {
            if (config == null)
                config = Resources.Load<PlayerStatsConfig>("Player/DefaultPlayerStats");

            if (config == null)
                config = PlayerStatsConfig.CreateRuntimeDefault();

            return config;
        }
    }

    public event Action OnStatsChanged;

    private void Awake()
    {
        health = GetComponent<Health>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        ApplyStatsToComponents();
    }

    /// <summary>
    /// 升级指定属性。数值为增量（例如 +10 生命、-0.05 射击间隔）。
    /// </summary>
    public void ApplyUpgrade(PlayerStatType statType, float value)
    {
        switch (statType)
        {
            case PlayerStatType.MaxHealth:
                bonusMaxHealth += Mathf.RoundToInt(value);
                break;
            case PlayerStatType.Defense:
                bonusDefense += Mathf.RoundToInt(value);
                break;
            case PlayerStatType.AttackDamage:
                bonusAttackDamage += Mathf.RoundToInt(value);
                break;
            case PlayerStatType.FireCooldown:
                bonusFireCooldown += value;
                break;
            case PlayerStatType.ProjectileSpeed:
                bonusProjectileSpeed += value;
                break;
            case PlayerStatType.ProjectileRange:
                bonusProjectileRange += value;
                break;
            case PlayerStatType.MoveSpeed:
                bonusMoveSpeed += value;
                break;
        }

        ApplyStatsToComponents();
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 敌人对玩家造成伤害时调用，会自动计算防御。
    /// </summary>
    public void TakeDamage(int rawDamage)
    {
        if (health == null || !health.IsAlive || rawDamage <= 0)
            return;

        int finalDamage = Mathf.Max(1, rawDamage - Defense);
        health.TakeDamage(finalDamage);
    }

    public void ApplyStatsToComponents()
    {
        if (health != null)
            health.SetMaxHealth(MaxHealth, healAddedAmount: true);

        if (playerController != null)
            playerController.SetMoveSpeed(MoveSpeed);
    }

    public static PlayerStats GetOrCreate(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null)
            stats = player.AddComponent<PlayerStats>();

        return stats;
    }
}
