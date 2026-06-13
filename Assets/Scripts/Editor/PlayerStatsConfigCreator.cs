#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 创建默认玩家属性配置资源。
/// </summary>
public static class PlayerStatsConfigCreator
{
    private const string ResourcePath = "Assets/Resources/Player/DefaultPlayerStats.asset";

    [MenuItem("NanoPhysician/Create Default Player Stats Config")]
    public static void CreateDefaultConfig()
    {
        if (File.Exists(ResourcePath))
        {
            EditorUtility.DisplayDialog(
                "Player Stats Config",
                "默认配置已存在：\n" + ResourcePath,
                "OK");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<PlayerStatsConfig>(ResourcePath);
            return;
        }

        string directory = Path.GetDirectoryName(ResourcePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        PlayerStatsConfig config = ScriptableObject.CreateInstance<PlayerStatsConfig>();
        AssetDatabase.CreateAsset(config, ResourcePath);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;

        Debug.Log("[PlayerStats] 已创建默认配置：" + ResourcePath);
    }
}
#endif
