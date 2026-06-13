using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 第三关：随机混合前两关敌人，并在关卡末段生成 Boss。
/// </summary>
public class Level3MixedEnemySpawner : MonoBehaviour, ILevelEnemySpawner
{
    [System.Serializable]
    private class MixedEnemyEntry
    {
        public string displayName;
        public Sprite sprite;
        public bool isRanged;
        public float scale = 0.12f;
        public int maxHealth = 40;
        public float moveSpeed = 1f;
        public float detectRange = 6f;
        public float attackRange = 0.65f;
        public int attackDamage = 10;
        public float attackCooldown = 0.5f;
        public float projectileSpeed = 4f;
        public float projectileRange = 8f;
    }

    [SerializeField] private Transform mapRoot;
    [SerializeField] private int totalEnemyCount = 22;
    [SerializeField] private float spawnStartX = 12f;
    [SerializeField] private float spawnInterval = 3.2f;
    [SerializeField] private float verticalSpread = 2.5f;
    [SerializeField] private float bossSpawnOffsetFromEnd = 6f;
    [SerializeField] private float bossScale = 0.18f;
    [SerializeField] private int bossMaxHealth = 900;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private MixedEnemyEntry[] enemyPool;

    private void Awake()
    {
        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }

        enemyPool = BuildEnemyPool();
        TryAssignSpritesFromArtAssets();
    }

    private void Start()
    {
        if (spawnedObjects.Count == 0)
            SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        ClearSpawnedObjects();
        SpawnMixedEnemies();
        SpawnBoss();
    }

    private void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
                Destroy(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
        PlayerMovementBounds.ClearArena();
    }

    private void SpawnMixedEnemies()
    {
        if (enemyPool == null || enemyPool.Length == 0)
            return;

        int yPatternIndex = 0;
        float[] yOffsets = { 0f, verticalSpread, -verticalSpread, verticalSpread * 0.5f, -verticalSpread * 0.5f };

        float endX = GetBossAreaStartX() - 4f;
        float startX = spawnStartX;

        for (int i = 0; i < totalEnemyCount; i++)
        {
            MixedEnemyEntry entry = enemyPool[Random.Range(0, enemyPool.Length)];
            float spawnY = yOffsets[yPatternIndex % yOffsets.Length];
            yPatternIndex++;

            float progress = totalEnemyCount <= 1 ? 0f : (float)i / (totalEnemyCount - 1);
            float spawnX = Mathf.Lerp(startX, endX, progress);

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

            if (MapBoundsUtility.TryGetBounds(mapRoot, out Bounds mapBounds))
            {
                spawnPosition.y = Mathf.Clamp(spawnPosition.y,
                    mapBounds.min.y + 1f,
                    mapBounds.max.y - 1f);
            }

            if (entry.isRanged)
                SpawnRangedEnemy(entry, spawnPosition);
            else
                SpawnMeleeEnemy(entry, spawnPosition);
        }
    }

    private void SpawnMeleeEnemy(MixedEnemyEntry entry, Vector3 position)
    {
        float scale = ResolveGlucoseScale(entry);

        GameObject enemyObject = CreateEnemyBase(entry, position, scale);
        EnemyController controller = enemyObject.AddComponent<EnemyController>();
        controller.Configure(
            entry.moveSpeed,
            entry.detectRange,
            entry.attackRange,
            entry.attackDamage,
            entry.attackCooldown,
            mapRoot);

        SetupColliderAndHealthBar(enemyObject, entry, scale);
    }

    private void SpawnRangedEnemy(MixedEnemyEntry entry, Vector3 position)
    {
        GameObject enemyObject = CreateEnemyBase(entry, position, entry.scale);

        RangedEnemyController controller = enemyObject.AddComponent<RangedEnemyController>();
        controller.Configure(
            entry.moveSpeed,
            entry.detectRange,
            entry.attackRange,
            entry.attackDamage,
            entry.attackCooldown,
            entry.projectileSpeed,
            entry.projectileRange,
            mapRoot);

        SetupColliderAndHealthBar(enemyObject, entry, entry.scale);
    }

    private GameObject CreateEnemyBase(MixedEnemyEntry entry, Vector3 position, float scale)
    {
        GameObject enemyObject = new GameObject($"Enemy_{entry.displayName}");
        enemyObject.tag = "Enemy";
        enemyObject.transform.SetParent(transform);
        enemyObject.transform.position = position;
        enemyObject.transform.localScale = Vector3.one * scale;

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = entry.sprite;
        spriteRenderer.sortingOrder = 2;

        Health health = enemyObject.AddComponent<Health>();
        health.Configure(entry.maxHealth);

        spawnedObjects.Add(enemyObject);
        return enemyObject;
    }

    private void SetupColliderAndHealthBar(GameObject enemyObject, MixedEnemyEntry entry, float scale)
    {
        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = enemyObject.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = enemyObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        float worldHitRadius = Mathf.Clamp(spriteRenderer.bounds.extents.x * 0.45f, 0.35f, 1.2f);
        collider.radius = worldHitRadius / scale;

        Health health = enemyObject.GetComponent<Health>();

        GameObject healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(enemyObject.transform);
        healthBarObject.transform.localScale = Vector3.one / scale;
        WorldHealthBar healthBar = healthBarObject.AddComponent<WorldHealthBar>();
        healthBar.Initialize(health, new Vector3(0f, 0.8f, 0f));
    }

    private void SpawnBoss()
    {
        if (!MapBoundsUtility.TryGetBounds(mapRoot, out Bounds mapBounds))
            return;

        Sprite bossSprite = LoadBossSprite();
        Vector3 bossPosition = new Vector3(mapBounds.max.x - bossSpawnOffsetFromEnd, mapBounds.center.y, 0f);

        GameObject bossObject = new GameObject("Boss");
        bossObject.tag = "Enemy";
        bossObject.transform.SetParent(transform);
        bossObject.transform.position = bossPosition;
        bossObject.transform.localScale = Vector3.one * bossScale;

        SpriteRenderer spriteRenderer = bossObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = bossSprite;
        spriteRenderer.sortingOrder = 3;

        Health health = bossObject.AddComponent<Health>();
        CircleCollider2D collider = bossObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 2.5f;

        BossController boss = bossObject.AddComponent<BossController>();
        boss.Configure(
            bossMaxHealth,
            0.45f,
            36,
            18,
            mapRoot);

        GameObject healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(bossObject.transform);
        healthBarObject.transform.localScale = Vector3.one / bossScale;
        WorldHealthBar healthBar = healthBarObject.AddComponent<WorldHealthBar>();
        healthBar.Initialize(health, new Vector3(0f, 1.1f, 0f));

        spawnedObjects.Add(bossObject);
    }

    private float GetBossAreaStartX()
    {
        if (MapBoundsUtility.TryGetBounds(mapRoot, out Bounds mapBounds))
            return mapBounds.max.x - 24f;

        return spawnStartX + totalEnemyCount * spawnInterval;
    }

    private float ResolveGlucoseScale(MixedEnemyEntry entry)
    {
        if (entry.displayName != "基本血糖" || entry.sprite == null)
            return entry.scale;

        MixedEnemyEntry dimer = System.Array.Find(enemyPool, e => e.displayName == "二聚体");
        if (dimer == null || dimer.sprite == null)
            return entry.scale;

        float dimerWorldHeight = dimer.sprite.bounds.size.y * dimer.scale;
        return (dimerWorldHeight * 0.5f) / entry.sprite.bounds.size.y;
    }

    private static MixedEnemyEntry[] BuildEnemyPool()
    {
        return new[]
        {
            new MixedEnemyEntry { displayName = "基本血糖", isRanged = false, scale = 0.08f, maxHealth = 30, moveSpeed = 1.4f, detectRange = 5.5f, attackRange = 0.65f, attackDamage = 8, attackCooldown = 0.5f },
            new MixedEnemyEntry { displayName = "二聚体", isRanged = false, scale = 0.12f, maxHealth = 55, moveSpeed = 1.2f, detectRange = 8f, attackRange = 0.7f, attackDamage = 12, attackCooldown = 0.5f },
            new MixedEnemyEntry { displayName = "血糖聚合体", isRanged = false, scale = 0.14f, maxHealth = 85, moveSpeed = 1.05f, detectRange = 10.5f, attackRange = 0.75f, attackDamage = 16, attackCooldown = 0.45f },
            new MixedEnemyEntry { displayName = "血糖结晶", isRanged = false, scale = 0.16f, maxHealth = 120, moveSpeed = 0.95f, detectRange = 13f, attackRange = 0.85f, attackDamage = 22, attackCooldown = 0.4f },
            new MixedEnemyEntry { displayName = "中性粒细胞", isRanged = true, scale = 0.11f, maxHealth = 40, moveSpeed = 0.85f, detectRange = 5.5f, attackRange = 6f, attackDamage = 10, attackCooldown = 2.5f, projectileSpeed = 3.5f, projectileRange = 8f },
            new MixedEnemyEntry { displayName = "巨噬细胞", isRanged = true, scale = 0.12f, maxHealth = 65, moveSpeed = 0.8f, detectRange = 8f, attackRange = 6.5f, attackDamage = 14, attackCooldown = 2.5f, projectileSpeed = 4f, projectileRange = 8.5f },
            new MixedEnemyEntry { displayName = "T淋巴细胞", isRanged = true, scale = 0.13f, maxHealth = 90, moveSpeed = 0.75f, detectRange = 10.5f, attackRange = 7f, attackDamage = 18, attackCooldown = 2.5f, projectileSpeed = 4.2f, projectileRange = 9f },
            new MixedEnemyEntry { displayName = "B淋巴细胞", isRanged = true, scale = 0.14f, maxHealth = 110, moveSpeed = 0.7f, detectRange = 10.5f, attackRange = 7f, attackDamage = 22, attackCooldown = 2.5f, projectileSpeed = 4.2f, projectileRange = 9f },
            new MixedEnemyEntry { displayName = "NK细胞", isRanged = true, scale = 0.15f, maxHealth = 140, moveSpeed = 0.65f, detectRange = 13f, attackRange = 7.5f, attackDamage = 28, attackCooldown = 2.5f, projectileSpeed = 4.5f, projectileRange = 9.5f }
        };
    }

    private void TryAssignSpritesFromArtAssets()
    {
#if UNITY_EDITOR
        AssignSprite("基本血糖", "基本血糖游戏素材.png");
        AssignSprite("二聚体", "二聚体游戏素材.png");
        AssignSprite("血糖聚合体", "血糖聚合体游戏素材.png");
        AssignSprite("血糖结晶", "血糖结晶游戏素材.png");
        AssignSprite("中性粒细胞", "中性粒细胞.png");
        AssignSprite("巨噬细胞", "巨噬细胞.png");
        AssignSprite("T淋巴细胞", "T 淋巴细胞.png");
        AssignSprite("B淋巴细胞", "B淋巴细胞.png");
        AssignSprite("NK细胞", "NK细胞.png");
#endif
    }

#if UNITY_EDITOR
    private void AssignSprite(string displayName, string fileName)
    {
        Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/art assets/{fileName}");
        if (sprite == null)
            return;

        foreach (MixedEnemyEntry entry in enemyPool)
        {
            if (entry.displayName == displayName)
                entry.sprite = sprite;
        }
    }
#endif

    private static Sprite LoadBossSprite()
    {
#if UNITY_EDITOR
        Sprite editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/art assets/boss.png");
        if (editorSprite != null)
            return editorSprite;
#endif
        return Resources.Load<Sprite>("Boss/boss");
    }
}
