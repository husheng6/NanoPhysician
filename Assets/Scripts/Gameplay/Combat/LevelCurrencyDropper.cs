using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 每关随机指定 1-3 个敌人掉落货币。
/// </summary>
public static class LevelCurrencyDropper
{
    private static readonly Dictionary<Health, Action> deathHandlers = new Dictionary<Health, Action>();

    public static void ResetState()
    {
        foreach (KeyValuePair<Health, Action> pair in deathHandlers)
        {
            if (pair.Key != null)
                pair.Key.OnDeath -= pair.Value;
        }

        deathHandlers.Clear();
    }

    public static void SetupForCurrentLevel()
    {
        ResetState();

        if (LevelGameFlow.IsIntroActive)
            return;

        List<Health> candidates = CollectAliveEnemyHealth();
        if (candidates.Count == 0)
            return;

        int dropCount = Mathf.Min(UnityEngine.Random.Range(1, 4), candidates.Count);
        Shuffle(candidates);

        for (int i = 0; i < dropCount; i++)
            RegisterDropper(candidates[i]);
    }

    private static void RegisterDropper(Health health)
    {
        if (health == null || deathHandlers.ContainsKey(health))
            return;

        Action handler = () => HandleEnemyDeath(health);
        deathHandlers.Add(health, handler);
        health.OnDeath += handler;
    }

    private static void HandleEnemyDeath(Health health)
    {
        if (health == null)
            return;

        if (deathHandlers.TryGetValue(health, out Action handler))
        {
            health.OnDeath -= handler;
            deathHandlers.Remove(health);
        }

        CurrencyPickup.Spawn(health.transform.position);
    }

    private static List<Health> CollectAliveEnemyHealth()
    {
        List<Health> result = new List<Health>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health != null && health.IsAlive)
                result.Add(health);
        }

        return result;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
