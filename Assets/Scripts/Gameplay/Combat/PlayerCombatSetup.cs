using UnityEngine;

/// <summary>
/// 各关卡共用的玩家战斗组件初始化。
/// </summary>
public static class PlayerCombatSetup
{
    public static void Setup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        if (player.GetComponent<Health>() == null)
            player.AddComponent<Health>();

        PlayerStats.GetOrCreate(player);

        if (player.GetComponent<PlayerShooting>() == null)
            player.AddComponent<PlayerShooting>();

        EnsurePlayerHitCollider(player);

        if (Object.FindObjectOfType<PlayerHealthBarUI>() == null)
            PlayerHealthBarUI.Create(player.GetComponent<Health>());

        LevelGameFlow.RegisterPlayer(player.GetComponent<Health>());
    }

    private static void EnsurePlayerHitCollider(GameObject player)
    {
        CircleCollider2D collider = player.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = player.AddComponent<CircleCollider2D>();

        collider.isTrigger = true;

        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        float worldRadius = spriteRenderer != null
            ? Mathf.Max(spriteRenderer.bounds.extents.x, spriteRenderer.bounds.extents.y)
            : 0.35f;

        float localScale = Mathf.Max(player.transform.lossyScale.x, 0.001f);
        collider.radius = worldRadius / localScale;
    }
}
