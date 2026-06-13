using UnityEngine;

/// <summary>
/// 关卡活动边界：Boss 战时限制玩家移动与镜头，仅在进入决斗区后生效。
/// </summary>
public static class PlayerMovementBounds
{
    private static Bounds? arenaBounds;
    private static bool isArenaLocked;

    public static bool IsArenaLocked => isArenaLocked;

    public static void LockArena(Bounds bounds)
    {
        arenaBounds = bounds;
        isArenaLocked = true;
    }

    public static void ClearArena()
    {
        arenaBounds = null;
        isArenaLocked = false;
    }

    public static bool TryGetMovementBounds(Transform mapRoot, out Bounds bounds)
    {
        if (isArenaLocked && arenaBounds.HasValue)
        {
            bounds = arenaBounds.Value;
            return true;
        }

        return MapBoundsUtility.TryGetBounds(mapRoot, out bounds);
    }
}
