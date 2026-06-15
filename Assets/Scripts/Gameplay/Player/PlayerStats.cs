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
    [SerializeField] private int bonusMeleeAttackDamage;
    [SerializeField] private float bonusMeleeAttackRange;
    [SerializeField] private float bonusMeleeAttackCooldown;
    [SerializeField] private int bonusMaxMana;
    [SerializeField] private int bonusManaCostPerShot;

    private Health health;
    private PlayerController playerController;
    private int currentMana;
    private float manaRegenAccumulator;

    public int MaxHealth => Config.maxHealth + bonusMaxHealth;
    public int Defense => Config.defense + bonusDefense;
    public int AttackDamage => Config.attackDamage + bonusAttackDamage;
    public float FireCooldown => Mathf.Max(0.05f, Config.fireCooldown + bonusFireCooldown);
    public float ProjectileSpeed => Mathf.Max(1f, Config.projectileSpeed + bonusProjectileSpeed);
    public float ProjectileRange => Mathf.Max(1f, Config.projectileRange + bonusProjectileRange);
    public float MoveSpeed => Mathf.Max(0.1f, Config.moveSpeed + bonusMoveSpeed);
    public int MeleeAttackDamage => Mathf.Max(1, Config.meleeAttackDamage + bonusMeleeAttackDamage);
    public float MeleeAttackRange => Mathf.Max(0.1f, Config.meleeAttackRange + bonusMeleeAttackRange);
    public float MeleeAttackCooldown => Mathf.Max(0.05f, Config.meleeAttackCooldown + bonusMeleeAttackCooldown);
    public int MaxMana => Mathf.Max(1, Config.maxMana + bonusMaxMana);
    public int CurrentMana => currentMana;
    public int ManaCostPerShot => Mathf.Max(1, Config.manaCostPerShot + bonusManaCostPerShot);
    public float ManaRegenPerSecond => Mathf.Max(0f, Config.manaRegenPerSecond);

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
    public event Action<int, int> OnManaChanged;

    private void Awake()
    {
        health = GetComponent<Health>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        ApplyStatsToComponents();
    }

    private void Update()
    {
        RegenerateMana();
    }

    /// <summary>
    /// 远程攻击消耗法力，不足时返回 false。
    /// </summary>
    public bool TryConsumeMana(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentMana < amount)
            return false;

        currentMana -= amount;
        OnManaChanged?.Invoke(currentMana, MaxMana);
        return true;
    }

    private void RegenerateMana()
    {
        if (LevelGameFlow.IsLevelEnded || LevelGameFlow.IsIntroActive)
            return;

        float regenRate = ManaRegenPerSecond;
        if (regenRate <= 0f || currentMana >= MaxMana)
            return;

        manaRegenAccumulator += regenRate * Time.deltaTime;
        if (manaRegenAccumulator < 1f)
            return;

        int restored = Mathf.FloorToInt(manaRegenAccumulator);
        manaRegenAccumulator -= restored;
        int before = currentMana;
        currentMana = Mathf.Min(MaxMana, currentMana + restored);
        if (currentMana != before)
            OnManaChanged?.Invoke(currentMana, MaxMana);
    }

    public void ApplyShopUpgradeLevels(int healthLevel, int meleeLevel, int rangedLevel, int manaLevel)
    {
        bonusMaxHealth = 0;
        bonusDefense = 0;
        bonusAttackDamage = 0;
        bonusFireCooldown = 0;
        bonusProjectileSpeed = 0;
        bonusProjectileRange = 0;
        bonusMoveSpeed = 0;
        bonusMeleeAttackDamage = 0;
        bonusMeleeAttackRange = 0;
        bonusMeleeAttackCooldown = 0;
        bonusMaxMana = 0;
        bonusManaCostPerShot = 0;

        bonusMaxHealth = healthLevel * 15;
        bonusMeleeAttackDamage = meleeLevel * 4;
        bonusAttackDamage = rangedLevel * 3;
        bonusMaxMana = manaLevel * 10;

        ApplyStatsToComponents();
        OnStatsChanged?.Invoke();
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
            case PlayerStatType.MeleeAttackDamage:
                bonusMeleeAttackDamage += Mathf.RoundToInt(value);
                break;
            case PlayerStatType.MeleeAttackRange:
                bonusMeleeAttackRange += value;
                break;
            case PlayerStatType.MeleeAttackCooldown:
                bonusMeleeAttackCooldown += value;
                break;
            case PlayerStatType.MaxMana:
                bonusMaxMana += Mathf.RoundToInt(value);
                break;
            case PlayerStatType.ManaCostPerShot:
                bonusManaCostPerShot += Mathf.RoundToInt(value);
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

        InitializeMana(refillToMax: currentMana <= 0);
    }

    private void InitializeMana(bool refillToMax)
    {
        int maxMana = MaxMana;
        if (refillToMax || currentMana > maxMana)
            currentMana = maxMana;

        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public static PlayerStats GetOrCreate(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null)
            stats = player.AddComponent<PlayerStats>();

        return stats;
    }
}
