using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 按关卡配置敌人掉落货币数量。
/// </summary>
public static class LevelCurrencyDropper
{
    private const string Level1SceneName = "level1Scence";
    private const string Level2SceneName = "level2Scence";

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

        int dropCount = GetDropCount(candidates.Count);
        Shuffle(candidates);

        for (int i = 0; i < dropCount; i++)
            RegisterDropper(candidates[i]);
    }

    private static int GetDropCount(int candidateCount)
    {
        if (candidateCount <= 0)
            return 0;

        string sceneName = SceneManager.GetActiveScene().name;
        int target;

        switch (sceneName)
        {
            case Level1SceneName:
            case Level2SceneName:
                // 第一、二关约 15 个，略有浮动
                target = UnityEngine.Random.Range(14, 17);
                break;
            default:
                target = UnityEngine.Random.Range(1, 4);
                break;
        }

        return Mathf.Min(target, candidateCount);
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
