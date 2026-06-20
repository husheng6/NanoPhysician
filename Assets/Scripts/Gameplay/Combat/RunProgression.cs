using System;

/// <summary>
/// 单局闯关进度：货币与商店升级等级跨关卡保留；挑战失败后仍保留，可在首页商城继续强化。
/// </summary>
public static class RunProgression
{
    public const int MaxUpgradeLevel = 10;
    public const int UpgradeCost = 1;

    private static int currency;
    private static readonly int[] upgradeLevels = new int[4];

    public static int Currency => currency;

    public static event Action OnChanged;

    public static void ResetRun()
    {
        currency = 0;
        for (int i = 0; i < upgradeLevels.Length; i++)
            upgradeLevels[i] = 0;

        NotifyChanged();
    }

    public static int GetUpgradeLevel(ShopUpgradeType type)
    {
        return upgradeLevels[(int)type];
    }

    public static void AddCurrency(int amount)
    {
        if (amount <= 0)
            return;

        currency += amount;
        NotifyChanged();
    }

    public static bool TryUpgrade(ShopUpgradeType type)
    {
        int index = (int)type;
        if (currency < UpgradeCost || upgradeLevels[index] >= MaxUpgradeLevel)
            return false;

        currency -= UpgradeCost;
        upgradeLevels[index]++;
        NotifyChanged();
        return true;
    }

    public static void ApplyToPlayer(PlayerStats stats)
    {
        if (stats == null)
            return;

        stats.ApplyShopUpgradeLevels(
            GetUpgradeLevel(ShopUpgradeType.Health),
            GetUpgradeLevel(ShopUpgradeType.MeleeAttack),
            GetUpgradeLevel(ShopUpgradeType.RangedAttack),
            GetUpgradeLevel(ShopUpgradeType.Mana));
    }

    public static float GetUpgradeValue(ShopUpgradeType type)
    {
        switch (type)
        {
            case ShopUpgradeType.Health:
                return 15f;
            case ShopUpgradeType.MeleeAttack:
                return 4f;
            case ShopUpgradeType.RangedAttack:
                return 3f;
            case ShopUpgradeType.Mana:
                return 10f;
            default:
                return 0f;
        }
    }

    public static string GetDisplayName(ShopUpgradeType type)
    {
        switch (type)
        {
            case ShopUpgradeType.Health:
                return "血量";
            case ShopUpgradeType.MeleeAttack:
                return "近战攻击力";
            case ShopUpgradeType.RangedAttack:
                return "远程攻击力";
            case ShopUpgradeType.Mana:
                return "蓝量";
            default:
                return type.ToString();
        }
    }

    private static void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}
