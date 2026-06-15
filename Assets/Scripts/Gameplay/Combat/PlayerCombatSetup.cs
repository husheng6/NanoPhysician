using UnityEngine;

/// <summary>
/// 各关卡共用的玩家战斗组件初始化。
/// </summary>
public static class PlayerCombatSetup
{
    private const string AnimatorResourcesPath = "Player/PlayerAnimator";
    private const string AnimatorEditorPath = "Assets/art assets/PlayerAnimator.controller";
    private const string IdleSpriteEditorPath = "Assets/art assets/idle/01.png";

    public static void Setup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        EnsurePlayerAnimator(player);

        if (player.GetComponent<Health>() == null)
            player.AddComponent<Health>();

        PlayerStats.GetOrCreate(player);
        RunProgression.ApplyToPlayer(player.GetComponent<PlayerStats>());

        if (player.GetComponent<PlayerShooting>() == null)
            player.AddComponent<PlayerShooting>();

        if (player.GetComponent<PlayerMeleeAttack>() == null)
            player.AddComponent<PlayerMeleeAttack>();

        EnsurePlayerHitCollider(player);

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        PlayerHealthBarUI.TryInitialize(player.GetComponent<Health>(), playerStats);

        CombatCurrencyHUD.TryInitialize();

        LevelGameFlow.RegisterPlayer(player.GetComponent<Health>());
    }

    private static void EnsurePlayerAnimator(GameObject player)
    {
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
            animator = player.AddComponent<Animator>();

        RuntimeAnimatorController controller = LoadAnimatorController();
        if (controller != null)
            animator.runtimeAnimatorController = controller;

        if (player.GetComponent<PlayerAnimator>() == null)
            player.AddComponent<PlayerAnimator>();

        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        Sprite idleSprite = LoadIdleSprite();
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;

        animator.Rebind();
        animator.Update(0f);
    }

    private static RuntimeAnimatorController LoadAnimatorController()
    {
        RuntimeAnimatorController controller =
            Resources.Load<RuntimeAnimatorController>(AnimatorResourcesPath);
#if UNITY_EDITOR
        if (controller == null)
            controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorEditorPath);
#endif
        if (controller == null)
            Debug.LogWarning("PlayerCombatSetup: 未找到 PlayerAnimator.controller，玩家将显示默认大图。");

        return controller;
    }

    private static Sprite LoadIdleSprite()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(IdleSpriteEditorPath);
#else
        return null;
#endif
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
