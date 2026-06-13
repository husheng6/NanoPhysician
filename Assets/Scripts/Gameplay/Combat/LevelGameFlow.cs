using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡流程控制：玩家死亡时暂停关卡并弹出失败界面。
/// </summary>
public static class LevelGameFlow
{
    private const string StartSceneName = "startScene";

    private static bool isGameOver;
    private static bool isVictory;

    public static bool IsGameOver => isGameOver;
    public static bool IsVictory => isVictory;
    public static bool IsLevelEnded => isGameOver || isVictory;

    public static void ResetState()
    {
        isGameOver = false;
        isVictory = false;
        Time.timeScale = 1f;
    }

    public static void RegisterPlayer(Health playerHealth)
    {
        if (playerHealth == null)
            return;

        playerHealth.OnDeath -= HandlePlayerDeath;
        playerHealth.OnDeath += HandlePlayerDeath;
    }

    private static void HandlePlayerDeath()
    {
        if (IsLevelEnded)
            return;

        isGameOver = true;
        Time.timeScale = 0f;
        GameOverUI.Show();
    }

    public static void NotifyVictory(string nextSceneName)
    {
        if (IsLevelEnded)
            return;

        isVictory = true;
        Time.timeScale = 0f;
        LevelVictoryUI.Show(nextSceneName);
    }

    public static void GoToNextLevel(string sceneName)
    {
        ResetState();
        SceneLoader.Load(sceneName);
    }

    public static void RestartCurrentLevel()
    {
        ResetState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void ReturnToStartScene()
    {
        ResetState();
        SceneLoader.Load(StartSceneName);
    }
}
