using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡选择场景初始化：确保右上角返回开始菜单按钮存在。
/// </summary>
public static class LevelSelectionBootstrap
{
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
        if (scene.name != LevelSelectionSceneName)
            return;

        if (Object.FindObjectOfType<LevelSelectionController>() != null)
            return;

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
            LevelReturnButtonUI.SetupForLevelSelection(canvas.transform);
    }
}
