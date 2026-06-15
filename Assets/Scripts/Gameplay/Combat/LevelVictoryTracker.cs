using UnityEngine;

/// <summary>
/// 追踪场上敌人，全部消灭后触发关卡胜利。
/// </summary>
public static class LevelVictoryTracker
{
    private static int remainingEnemies;
    private static bool isTracking;
    private static string nextSceneName;

    public static void ResetState()
    {
        isTracking = false;
        remainingEnemies = 0;
        nextSceneName = null;
    }

    public static void BeginTracking(string nextLevelSceneName)
    {
        ResetState();
        nextSceneName = nextLevelSceneName;

        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();
            if (health == null || !health.IsAlive)
                continue;

            remainingEnemies++;
            health.OnDeath += HandleEnemyDeath;
        }

        isTracking = true;

        if (remainingEnemies <= 0)
            LevelGameFlow.NotifyVictory(nextSceneName);
    }

    private static void HandleEnemyDeath()
    {
        if (!isTracking || LevelGameFlow.IsLevelEnded)
            return;

        remainingEnemies--;
        if (remainingEnemies <= 0)
            LevelGameFlow.NotifyVictory(nextSceneName);
    }
}
