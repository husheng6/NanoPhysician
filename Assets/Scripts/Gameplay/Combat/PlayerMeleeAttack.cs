using UnityEngine;

/// <summary>
/// 玩家近战攻击：鼠标左键点击，对周围范围内的敌人造成伤害（360度）。
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private int meleeMouseButton = 0;
    [SerializeField] private float meleeRange = 1.2f;
    [SerializeField] private float meleeCooldown = 0.4f;
    [SerializeField] private int meleeDamage = 20;

    private PlayerController playerController;
    private PlayerStats playerStats;
    private PlayerAnimator playerAnimator;
    private Camera mainCamera;
    private float lastMeleeTime;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        playerAnimator = GetComponent<PlayerAnimator>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (LevelGameFlow.IsGameplayFrozen)
            return;

        if (!Input.GetMouseButtonDown(meleeMouseButton))
            return;

        float cooldown = playerStats != null ? playerStats.MeleeAttackCooldown : meleeCooldown;
        if (Time.time - lastMeleeTime < cooldown)
            return;

        Vector2 attackDirection = GetAttackDirection();
        PerformMeleeAttack(attackDirection);
        lastMeleeTime = Time.time;
    }

    private Vector2 GetAttackDirection()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                return playerController.FacingDirection;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 toMouse = (Vector2)mouseWorld - (Vector2)transform.position;
        if (toMouse.sqrMagnitude <= 0.0001f)
            return playerController.FacingDirection;

        return toMouse.normalized;
    }

    private void PerformMeleeAttack(Vector2 direction)
    {
        playerController.SetFacingFromDirection(direction);

        // 播放近战攻击动画与音效
        if (playerAnimator != null)
            playerAnimator.PlayAttack(0);

        SfxManager.PlayMelee();

        float range = playerStats != null ? playerStats.MeleeAttackRange : meleeRange;
        int damage = playerStats != null ? playerStats.MeleeAttackDamage : meleeDamage;

        // 在周围圆形范围内检测敌人
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
                continue;

            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth == null)
                enemyHealth = hit.GetComponentInParent<Health>();

            if (enemyHealth == null || !enemyHealth.IsAlive)
                continue;

            enemyHealth.TakeDamage(damage);
        }
    }
}
