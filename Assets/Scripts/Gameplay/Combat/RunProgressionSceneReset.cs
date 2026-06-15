using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 进入主页或选关界面时重置单局进度，避免上一局货币/升级残留。
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

        RunProgression.ResetRun();
        ShopUI.ResetState();
    }
}
