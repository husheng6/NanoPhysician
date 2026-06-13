using System;
using UnityEngine;

/// <summary>
/// 通用生命值组件，玩家与敌人共用。
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void Configure(int health)
    {
        maxHealth = Mathf.Max(1, health);
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// 调整最大生命值。升级时可选择是否将新增上限转化为治疗。
    /// </summary>
    public void SetMaxHealth(int newMax, bool healAddedAmount = false)
    {
        int oldMax = maxHealth;
        maxHealth = Mathf.Max(1, newMax);

        if (CurrentHealth <= 0)
            CurrentHealth = maxHealth;
        else
        {
            if (healAddedAmount && maxHealth > oldMax)
                CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + (maxHealth - oldMax));
            else if (CurrentHealth > maxHealth)
                CurrentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (!IsAlive)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
