using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景跳转工具类。
/// </summary>
public static class SceneLoader
{
    public static void Load(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneLoader: 场景名称为空。");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
