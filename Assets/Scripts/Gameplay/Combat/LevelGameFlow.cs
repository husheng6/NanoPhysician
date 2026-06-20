using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡流程控制：玩家死亡时暂停关卡并弹出失败界面。
/// </summary>
public static class LevelGameFlow
{
    private const string StartSceneName = "startScene";
    private const string LevelSelectionSceneName = "levelselectionScene";

    private static bool isGameOver;
    private static bool isVictory;
    private static bool isReturnDialogOpen;

    public static bool IsGameOver => isGameOver;
    public static bool IsVictory => isVictory;
    public static bool IsLevelEnded => isGameOver || isVictory;
    public static bool IsReturnDialogOpen => isReturnDialogOpen;
    public static bool IsIntroActive { get; private set; }
    public static bool IsGameplayFrozen => IsLevelEnded || IsIntroActive || isReturnDialogOpen;

    public static void SetIntroActive(bool active)
    {
        IsIntroActive = active;
    }

    public static void ResetState()
    {
        isGameOver = false;
        isVictory = false;
        isReturnDialogOpen = false;
        IsIntroActive = false;
        Time.timeScale = 1f;
        LevelReturnConfirmUI.Hide();
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
        if (IsLevelEnded || IsIntroActive)
            return;

        isVictory = true;
        Time.timeScale = 0f;
        ShopUI.Show(nextSceneName);
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
        ShopUI.ResetState();
        LevelReturnButtonUI.ResetState();
        LevelReturnConfirmUI.ResetState();
        SceneLoader.Load(StartSceneName);
    }

    public static void RequestReturnToLevelSelection()
    {
        if (IsLevelEnded || IsIntroActive || isReturnDialogOpen)
            return;

        isReturnDialogOpen = true;
        Time.timeScale = 0f;
        LevelReturnConfirmUI.Show();
    }

    public static void ConfirmReturnToLevelSelection()
    {
        ResetState();
        ShopUI.ResetState();
        LevelReturnButtonUI.ResetState();
        LevelReturnConfirmUI.ResetState();
        SceneLoader.Load(LevelSelectionSceneName);
    }

    public static void CancelReturnToLevelSelection()
    {
        if (!isReturnDialogOpen)
            return;

        isReturnDialogOpen = false;
        Time.timeScale = 1f;
        LevelReturnConfirmUI.Hide();
    }
}
