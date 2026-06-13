using UnityEngine;

/// <summary>
/// 远程敌人 AI：缓慢靠近，进入射程后原地射击玩家（默认 1 秒 1 发）。
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CircleCollider2D))]
public class RangedEnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.9f;
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float shootRange = 6f;
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private float fireInterval = 2.5f;
    [SerializeField] private float projectileSpeed = 4f;
    [SerializeField] private float projectileTravelDistance = 8f;
    [SerializeField] private float spawnOffset = 0.45f;

    private Transform playerTransform;
    private Health selfHealth;
    private SpriteRenderer spriteRenderer;
    private Transform mapRoot;
    private bool isAggro;
    private float lastFireTime;

    public void Configure(
        float speed,
        float detectionRange,
        float attackRange,
        int damage,
        float cooldown,
        float bulletSpeed,
        float bulletRange,
        Transform mapRootTransform)
    {
        moveSpeed = speed;
        detectRange = detectionRange;
        shootRange = attackRange;
        projectileDamage = damage;
        fireInterval = cooldown;
        projectileSpeed = bulletSpeed;
        projectileTravelDistance = bulletRange;
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }
    }

    private void Update()
    {
        if (LevelGameFlow.IsLevelEnded || !selfHealth.IsAlive)
            return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            if (playerTransform == null)
                return;
        }

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

            if (distanceToPlayer > shootRange)
            {
                Vector3 nextPosition = transform.position + (Vector3)(direction * moveSpeed * Time.deltaTime);
                transform.position = ClampToMap(nextPosition);
            }
            else
            {
                TryShoot(direction);
            }
        }
    }

    private void TryShoot(Vector2 direction)
    {
        if (Time.time - lastFireTime < fireInterval)
            return;

        Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffset);
        GameObject projectileObject = new GameObject("EnemyProjectile");
        projectileObject.transform.position = spawnPosition;

        EnemyProjectile projectile = projectileObject.AddComponent<EnemyProjectile>();
        projectile.Launch(direction, projectileDamage, projectileSpeed, projectileTravelDistance);

        lastFireTime = Time.time;
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

        const float clampHalfSize = 0.55f;
        position.x = Mathf.Clamp(position.x, mapBounds.min.x + clampHalfSize, mapBounds.max.x - clampHalfSize);
        position.y = Mathf.Clamp(position.y, mapBounds.min.y + clampHalfSize, mapBounds.max.y - clampHalfSize);
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
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
