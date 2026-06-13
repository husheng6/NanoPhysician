using UnityEngine;

/// <summary>
/// 根据地图根节点下所有 SpriteRenderer 计算合并边界。
/// </summary>
public static class MapBoundsUtility
{
    public static bool TryGetBounds(Transform mapRoot, out Bounds bounds)
    {
        bounds = default;

        if (mapRoot == null)
            return false;

        SpriteRenderer[] renderers = mapRoot.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0)
            return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return true;
    }
}
