using UnityEngine;

/// <summary>
/// 近战敌人 AI：感知范围内持续追敌，靠近玩家时持续造成伤害。
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float damageRange = 0.7f;
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float damageInterval = 0.5f;

    private Transform playerTransform;
    private Health playerHealth;
    private PlayerStats playerStats;
    private Health selfHealth;
    private SpriteRenderer spriteRenderer;
    private Transform mapRoot;
    private bool isAggro;
    private float lastDamageTime;

    public void Configure(
        float speed,
        float detectionRange,
        float meleeRange,
        int damage,
        float cooldown,
        Transform mapRootTransform)
    {
        moveSpeed = speed;
        detectRange = detectionRange;
        damageRange = meleeRange;
        contactDamage = damage;
        damageInterval = cooldown;
        mapRoot = mapRootTransform;
    }

    private void Awake()
    {
        selfHealth = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;

        selfHealth.OnDeath += HandleDeath;
    }

    private void Start()
    {
        ResolvePlayerReference();
    }

    private void Update()
    {
        if (LevelGameFlow.IsGameplayFrozen || !selfHealth.IsAlive)
            return;

        if (playerTransform == null || playerHealth == null)
        {
            ResolvePlayerReference();
            if (playerTransform == null || playerHealth == null)
                return;
        }

        if (!playerHealth.IsAlive)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (!isAggro && distanceToPlayer <= detectRange)
            isAggro = true;

        if (!isAggro)
            return;

        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Vector2 direction = toPlayer.normalized;
            UpdateFacing(direction);

            float moveFactor = distanceToPlayer <= damageRange ? 0.4f : 1f;
            Vector3 nextPosition = transform.position + (Vector3)(direction * moveSpeed * moveFactor * Time.deltaTime);
            transform.position = ClampToMap(nextPosition);
        }

        if (distanceToPlayer <= damageRange)
            ApplyContactDamage();
    }

    private void ApplyContactDamage()
    {
        if (Time.time - lastDamageTime < damageInterval)
            return;

        DamagePlayer(contactDamage);
        lastDamageTime = Time.time;
    }

    private void DamagePlayer(int rawDamage)
    {
        if (playerStats != null)
            playerStats.TakeDamage(rawDamage);
        else if (playerHealth != null)
            playerHealth.TakeDamage(rawDamage);
    }

    private void ResolvePlayerReference()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        playerTransform = player.transform;
        playerHealth = player.GetComponent<Health>();
        playerStats = player.GetComponent<PlayerStats>();

        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }
    }

    private void UpdateFacing(Vector2 direction)
    {
        if (spriteRenderer == null || Mathf.Approximately(direction.x, 0f))
            return;

        spriteRenderer.flipX = direction.x < 0f;
    }

    private Vector3 ClampToMap(Vector3 position)
    {
        if (mapRoot == null || !MapBoundsUtility.TryGetBounds(mapRoot, out Bounds mapBounds))
            return position;

        // 大体积敌人若用完整精灵 bounds 会被地图边界卡死，改用固定较小半径做移动限制
        const float clampHalfSize = 0.55f;

        position.x = Mathf.Clamp(position.x,
            mapBounds.min.x + clampHalfSize,
            mapBounds.max.x - clampHalfSize);
        position.y = Mathf.Clamp(position.y,
            mapBounds.min.y + clampHalfSize,
            mapBounds.max.y - clampHalfSize);
        return position;
    }

    private void HandleDeath()
    {
        Destroy(gameObject, 0.05f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}
