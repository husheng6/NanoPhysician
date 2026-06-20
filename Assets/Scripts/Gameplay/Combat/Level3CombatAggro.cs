using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 第三关战斗规则：玩家不主动攻击时敌人保持中立；一旦玩家出手，画面内的敌人立即进入敌对。
/// </summary>
public static class Level3CombatAggro
{
    private const string Level3SceneName = "level3Scence";

    private static bool isProvoked;

    public static bool IsActive => SceneManager.GetActiveScene().name == Level3SceneName;
    public static bool IsProvoked => isProvoked;

    public static void ResetState()
    {
        isProvoked = false;
    }

    public static bool AllowsPassiveAggro()
    {
        return !IsActive || isProvoked;
    }

    public static void NotifyPlayerAttacked()
    {
        if (!IsActive)
            return;

        if (!isProvoked)
        {
            isProvoked = true;
            AggroVisibleEnemies();
        }
    }

    private static void AggroVisibleEnemies()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !IsVisibleOnCamera(camera, enemy.transform.position))
                continue;

            ForceAggro(enemy);
        }
    }

    private static bool IsVisibleOnCamera(Camera camera, Vector3 worldPosition)
    {
        Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
        if (viewport.z <= 0f)
            return false;

        const float margin = 0.05f;
        return viewport.x >= -margin
            && viewport.x <= 1f + margin
            && viewport.y >= -margin
            && viewport.y <= 1f + margin;
    }

    private static void ForceAggro(GameObject enemy)
    {
        BossController boss = enemy.GetComponent<BossController>();
        if (boss != null)
        {
            boss.ForceAggro();
            return;
        }

        EnemyController meleeEnemy = enemy.GetComponent<EnemyController>();
        if (meleeEnemy != null)
        {
            meleeEnemy.ForceAggro();
            return;
        }

        RangedEnemyController rangedEnemy = enemy.GetComponent<RangedEnemyController>();
        if (rangedEnemy != null)
            rangedEnemy.ForceAggro();
    }
}
