using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡战斗初始化：场景加载完成后配置玩家并生成敌人。
/// 敌人由运行时脚本创建，不依赖场景里预先放置的预制体。
/// </summary>
public static class LevelCombatBootstrap
{
    private const string Level1SceneName = "level1Scence";
    private const string Level2SceneName = "level2Scence";
    private const string Level3SceneName = "level3Scence";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LevelGameFlow.ResetState();
        GameOverUI.ResetState();
        LevelVictoryUI.ResetState();
        ShopUI.ResetState();
        LevelVictoryTracker.ResetState();
        LevelCurrencyDropper.ResetState();
        Level1SegmentDialogueTrigger.ResetState();
        Level2SegmentDialogueTrigger.ResetState();
        Level3SegmentDialogueTrigger.ResetState();
        Level3CombatAggro.ResetState();
        LevelReturnButtonUI.ResetState();
        LevelReturnConfirmUI.ResetState();
        PlayerMovementBounds.ClearArena();

        switch (scene.name)
        {
            case Level1SceneName:
                SetupLevel(PlayerCombatSetup.Setup, null);
                Level1IntroDialogueRunner.Play(() =>
                {
                    BgmManager.PlayBattle();
                    SetupLevel1Spawner();
                    Level1SegmentDialogueTrigger.Setup();
                    BeginVictoryTracking(GetNextSceneName(scene.name));
                    LevelCurrencyDropper.SetupForCurrentLevel();
                });
                break;
            case Level2SceneName:
                SetupLevel(PlayerCombatSetup.Setup, null);
                Level2IntroDialogueRunner.Play(() =>
                {
                    BgmManager.PlayBattle();
                    SetupLevel2Spawner();
                    Level2SegmentDialogueTrigger.Setup();
                    BeginVictoryTracking(GetNextSceneName(scene.name));
                    LevelCurrencyDropper.SetupForCurrentLevel();
                    LevelReturnButtonUI.SetupForLevel();
                });
                break;
            case Level3SceneName:
                SetupLevel(PlayerCombatSetup.Setup, null);
                Level3IntroDialogueRunner.Play(() =>
                {
                    SetupLevel3Spawner();
                    Level3SegmentDialogueTrigger.Setup();
                    BeginVictoryTracking(null);
                    LevelCurrencyDropper.SetupForCurrentLevel();
                    LevelReturnButtonUI.SetupForLevel();
                });
                break;
        }
    }

    private static void BeginVictoryTracking(string nextSceneName)
    {
        LevelVictoryTracker.BeginTracking(nextSceneName);
    }

    private static string GetNextSceneName(string currentSceneName)
    {
        switch (currentSceneName)
        {
            case Level1SceneName:
                return Level2SceneName;
            case Level2SceneName:
                return Level3SceneName;
            default:
                return null;
        }
    }

    private static void SetupLevel(System.Action setupPlayer, System.Action setupSpawner)
    {
        ApplyBackgroundBlur();
        setupPlayer?.Invoke();
        setupSpawner?.Invoke();
    }

    private static void ApplyBackgroundBlur()
    {
        GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
        if (backgroundGroup == null)
            return;

        BattleBackgroundLayout.Apply(backgroundGroup.transform);
        BackgroundBlurEffect.ApplyToMap(backgroundGroup.transform);
    }

    private static void SetupLevel1Spawner()
    {
        CreateFreshSpawner<GlucoseEnemySpawner>();
    }

    private static void SetupLevel2Spawner()
    {
        CreateFreshSpawner<ImmuneCellEnemySpawner>();
    }

    private static void SetupLevel3Spawner()
    {
        CreateFreshSpawner<Level3MixedEnemySpawner>();
    }

    private static void CreateFreshSpawner<T>() where T : MonoBehaviour, ILevelEnemySpawner
    {
        GameObject level = GameObject.Find("Level");
        if (level == null)
        {
            Debug.LogError("[LevelCombatBootstrap] 找不到 Level 对象，无法生成敌人。");
            return;
        }

        RemoveExistingSpawners(level);

        GameObject spawnerObject = new GameObject("EnemySpawner");
        spawnerObject.transform.SetParent(level.transform);
        spawnerObject.transform.localPosition = Vector3.zero;

        T spawner = spawnerObject.AddComponent<T>();
        spawner.SpawnEnemies();
    }

    private static void RemoveExistingSpawners(GameObject level)
    {
        GlucoseEnemySpawner[] glucoseSpawners = level.GetComponentsInChildren<GlucoseEnemySpawner>(true);
        foreach (GlucoseEnemySpawner spawner in glucoseSpawners)
        {
            if (spawner != null)
                Object.DestroyImmediate(spawner.gameObject);
        }

        ImmuneCellEnemySpawner[] immuneSpawners = level.GetComponentsInChildren<ImmuneCellEnemySpawner>(true);
        foreach (ImmuneCellEnemySpawner spawner in immuneSpawners)
        {
            if (spawner != null)
                Object.DestroyImmediate(spawner.gameObject);
        }

        Level3MixedEnemySpawner[] mixedSpawners = level.GetComponentsInChildren<Level3MixedEnemySpawner>(true);
        foreach (Level3MixedEnemySpawner spawner in mixedSpawners)
        {
            if (spawner != null)
                Object.DestroyImmediate(spawner.gameObject);
        }

        BossController[] bosses = level.GetComponentsInChildren<BossController>(true);
        foreach (BossController boss in bosses)
        {
            if (boss != null)
                Object.DestroyImmediate(boss.gameObject);
        }

        Transform leftover = level.transform.Find("EnemySpawner");
        if (leftover != null)
            Object.DestroyImmediate(leftover.gameObject);
    }
}
