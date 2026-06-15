using UnityEngine;

/// <summary>
/// 对话统一字体：更纱黑体（sarasa-gothic-sc-regular）。
/// </summary>
public static class DialogueFont
{
    private const string ResourcesPath = "Fonts/sarasa-gothic-sc-regular";
    private const string EditorPath = "Assets/1_Asset/4_Font/sarasa-gothic-sc-regular.ttf";

    private static Font cached;

    public static Font Get()
    {
        if (cached != null)
            return cached;

        cached = Resources.Load<Font>(ResourcesPath);
#if UNITY_EDITOR
        if (cached == null)
            cached = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(EditorPath);
#endif

        if (cached == null)
            Debug.LogWarning("DialogueFont: 找不到 sarasa 字体。");

        return cached;
    }
}
