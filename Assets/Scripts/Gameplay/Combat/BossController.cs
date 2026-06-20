using UnityEngine;

/// <summary>
/// 第三关 Boss：高血量，缓慢靠近，近战伤害高，远程伤害为近战一半。
/// 发现玩家后锁定决斗区域，限制玩家移动范围。
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CircleCollider2D))]
public class BossController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.45f;
    [SerializeField] private float detectRange = 12f;
    [SerializeField] private float meleeRange = 1.1f;
    [SerializeField] private float shootRange = 8.5f;
    [SerializeField] private int meleeDamage = 36;
    [SerializeField] private int rangedDamage = 18;
    [SerializeField] private float meleeInterval = 0.85f;
    [SerializeField] private float rangedInterval = 1.4f;
    [SerializeField] private float projectileSpeed = 4f;
    [SerializeField] private float projectileRange = 9f;
    [SerializeField] private float arenaWidth = 20f;
    [SerializeField] private float arenaHeightPadding = 1.2f;
    [SerializeField] private float spawnOffset = 0.7f;

    private Transform playerTransform;
    private PlayerStats playerStats;
    private Health playerHealth;
    private Health selfHealth;
    private SpriteRenderer spriteRenderer;
    private Transform mapRoot;
    private bool isAggro;
    private bool arenaLocked;
    private bool minionsCleared;
    private Bounds fightArena;
    private bool hasFightArena;
    private float lastMeleeTime;
    private float lastRangedTime;

    public void Configure(
        int maxHealth,
        float speed,
        int meleeDmg,
        int rangedDmg,
        Transform mapRootTransform)
    {
        selfHealth = GetComponent<Health>();
        selfHealth.Configure(maxHealth);
        moveSpeed = speed;
        meleeDamage = meleeDmg;
        rangedDamage = rangedDmg;
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
        ResolvePlayer();
        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }

        TryBuildFightArena();
    }

    private void Update()
    {
        if (LevelGameFlow.IsGameplayFrozen || !selfHealth.IsAlive)
            return;

        ResolvePlayer();
        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (!arenaLocked && hasFightArena && IsPlayerInsideFightArena())
            LockArenaForFight();
        else if (!isAggro && distanceToPlayer <= detectRange && Level3CombatAggro.AllowsPassiveAggro())
        {
            isAggro = true;
            ClearMinions();
        }

        if (!isAggro)
            return;

        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude <= 0.0001f)
            return;

        Vector2 direction = toPlayer.normalized;
        UpdateFacing(direction);

        if (distanceToPlayer > meleeRange * 0.85f)
        {
            Vector3 nextPosition = transform.position + (Vector3)(direction * moveSpeed * Time.deltaTime);
            transform.position = ClampToMap(nextPosition);
        }

        if (distanceToPlayer <= meleeRange)
            TryMeleeAttack();
        else if (distanceToPlayer <= shootRange)
            TryRangedAttack(direction);
    }

    private void TryMeleeAttack()
    {
        if (Time.time - lastMeleeTime < meleeInterval)
            return;

        DamagePlayer(meleeDamage);
        lastMeleeTime = Time.time;
    }

    private void TryRangedAttack(Vector2 direction)
    {
        if (Time.time - lastRangedTime < rangedInterval)
            return;

        Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffset);
        GameObject projectileObject = new GameObject("BossProjectile");
        projectileObject.transform.position = spawnPosition;

        EnemyProjectile projectile = projectileObject.AddComponent<EnemyProjectile>();
        projectile.Launch(direction, rangedDamage, projectileSpeed, projectileRange);

        lastRangedTime = Time.time;
    }

    private void DamagePlayer(int rawDamage)
    {
        if (playerStats != null)
            playerStats.TakeDamage(rawDamage);
        else if (playerHealth != null)
            playerHealth.TakeDamage(rawDamage);
    }

    private void ResolvePlayer()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        playerTransform = player.transform;
        playerStats = player.GetComponent<PlayerStats>();
        playerHealth = player.GetComponent<Health>();
    }

    private void TryBuildFightArena()
    {
        if (!MapBoundsUtility.TryGetBounds(mapRoot, out Bounds mapBounds))
            return;

        fightArena = new Bounds(
            new Vector3(mapBounds.max.x - arenaWidth * 0.5f, mapBounds.center.y, 0f),
            new Vector3(arenaWidth, mapBounds.size.y - arenaHeightPadding * 2f, 1f));
        hasFightArena = true;
    }

    private bool IsPlayerInsideFightArena()
    {
        Vector3 playerPosition = playerTransform.position;
        return playerPosition.x >= fightArena.min.x
            && playerPosition.x <= fightArena.max.x
            && playerPosition.y >= fightArena.min.y
            && playerPosition.y <= fightArena.max.y;
    }

    public void ForceAggro()
    {
        isAggro = true;
        ClearMinions();
    }

    private void LockArenaForFight()
    {
        if (arenaLocked || !hasFightArena)
            return;

        arenaLocked = true;
        ClearMinions();
        PlayerMovementBounds.LockArena(fightArena);
    }

    private void ClearMinions()
    {
        if (minionsCleared)
            return;

        minionsCleared = true;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || enemy == gameObject)
                continue;

            if (enemy.GetComponent<BossController>() != null)
                continue;

            Health health = enemy.GetComponent<Health>();
            if (health != null && health.IsAlive)
                health.TakeDamage(health.CurrentHealth);
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
        if (!PlayerMovementBounds.TryGetMovementBounds(mapRoot, out Bounds bounds)
            && !MapBoundsUtility.TryGetBounds(mapRoot, out bounds))
            return position;

        const float halfSize = 0.8f;
        position.x = Mathf.Clamp(position.x, bounds.min.x + halfSize, bounds.max.x - halfSize);
        position.y = Mathf.Clamp(position.y, bounds.min.y + halfSize, bounds.max.y - halfSize);
        return position;
    }

    private void HandleDeath()
    {
        PlayerMovementBounds.ClearArena();
        Destroy(gameObject, 0.05f);
    }

    private void OnDestroy()
    {
        if (selfHealth != null)
            selfHealth.OnDeath -= HandleDeath;
    }
}
