using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 进入主页或选关界面时清理商店 UI 实例；货币与强化等级保留，供首页商城继续消费。
/// </summary>
public static class RunProgressionSceneReset
{
    private const string StartSceneName = "startScene";
    private const string LevelSelectionSceneName = "levelselectionScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != StartSceneName && scene.name != LevelSelectionSceneName)
            return;

        ShopUI.ResetState();
    }
}
