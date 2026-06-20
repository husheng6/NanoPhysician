using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗关卡背景：使用「游戏背景」横向拼接 3 张组成完整地图。
/// </summary>
public static class BattleBackgroundLayout
{
    private const int SegmentCount = 3;
    private const string SpriteResourcesPath = "Battle/游戏背景";
    private const string SpriteEditorPath = "Assets/art assets/游戏背景.png";

    private static readonly string[] SegmentNames =
    {
        "Background",
        "Background_2",
        "Background_3"
    };

    public static void Apply(Transform mapRoot)
    {
        if (mapRoot == null)
            return;

        Sprite sprite = LoadSprite();
        if (sprite == null)
        {
            Debug.LogWarning("BattleBackgroundLayout: 找不到游戏背景资源。");
            return;
        }

        float segmentWidth = sprite.bounds.size.x;
        List<SpriteRenderer> renderers = CollectBackgroundRenderers(mapRoot);

        for (int i = 0; i < SegmentCount; i++)
        {
            SpriteRenderer renderer = GetOrCreateRenderer(mapRoot, renderers, i);
            renderer.sprite = sprite;
            renderer.sortingOrder = -1;
            renderer.transform.localPosition = new Vector3(segmentWidth * (i + 0.5f), 0f, 0f);
            renderer.gameObject.name = SegmentNames[i];
            renderer.gameObject.SetActive(true);
        }

        for (int i = SegmentCount; i < renderers.Count; i++)
            renderers[i].gameObject.SetActive(false);
    }

    private static SpriteRenderer GetOrCreateRenderer(
        Transform mapRoot,
        List<SpriteRenderer> renderers,
        int index)
    {
        if (index < renderers.Count)
            return renderers[index];

        GameObject segment = new GameObject(SegmentNames[index]);
        segment.transform.SetParent(mapRoot, false);
        return segment.AddComponent<SpriteRenderer>();
    }

    private static List<SpriteRenderer> CollectBackgroundRenderers(Transform mapRoot)
    {
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();

        for (int i = 0; i < mapRoot.childCount; i++)
        {
            Transform child = mapRoot.GetChild(i);
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderers.Add(renderer);
        }

        renderers.Sort((a, b) =>
            a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));

        return renderers;
    }

    private static Sprite LoadSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(SpriteResourcesPath);
#if UNITY_EDITOR
        if (sprite == null)
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(SpriteEditorPath);
#endif
        return sprite;
    }
}
