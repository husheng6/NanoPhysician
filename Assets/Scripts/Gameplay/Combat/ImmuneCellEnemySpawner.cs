using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 第二关免疫细胞敌人生成器：按顺序生成五类远程射击敌人。
/// </summary>
public class ImmuneCellEnemySpawner : MonoBehaviour, ILevelEnemySpawner
{
    [System.Serializable]
    public class EnemyTypeConfig
    {
        public string displayName;
        public Sprite sprite;
        [Range(3, 5)] public int spawnCount = 4;
        public float scale = 0.12f;
        public int maxHealth = 40;
        public float moveSpeed = 0.9f;
        public float detectRange = 5.5f;
        public float shootRange = 6f;
        public int projectileDamage = 10;
        public float fireInterval = 2.5f;
        public float projectileSpeed = 4f;
        public float projectileRange = 8f;
    }

    [SerializeField] private Transform mapRoot;
    [SerializeField] private float spawnStartX = 12f;
    [SerializeField] private float spawnInterval = 4.5f;
    [SerializeField] private float verticalSpread = 2.5f;
    [SerializeField] private EnemyTypeConfig[] enemyTypes;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }

        EnsureDefaultConfigs();
    }

    private void Start()
    {
        if (spawnedEnemies.Count == 0)
            SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        ClearSpawnedEnemies();
        SpawnAllEnemies();
    }

    private void ClearSpawnedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] != null)
                Destroy(spawnedEnemies[i]);
        }

        spawnedEnemies.Clear();
    }

    private void EnsureDefaultConfigs()
    {
        if (enemyTypes != null && enemyTypes.Length > 0 && enemyTypes[0].sprite != null)
            return;

        enemyTypes = CreateDefaultConfigs();
        TryAssignSpritesFromArtAssets();
    }

    private void TryAssignSpritesFromArtAssets()
    {
#if UNITY_EDITOR
        enemyTypes[0].sprite = LoadSprite("中性粒细胞.png");
        enemyTypes[1].sprite = LoadSprite("巨噬细胞.png");
        enemyTypes[2].sprite = LoadSprite("T 淋巴细胞.png");
        enemyTypes[3].sprite = LoadSprite("B淋巴细胞.png");
        enemyTypes[4].sprite = LoadSprite("NK细胞.png");
#endif
    }

#if UNITY_EDITOR
    private static Sprite LoadSprite(string fileName)
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/art assets/{fileName}");
    }
#endif

    private static EnemyTypeConfig[] CreateDefaultConfigs()
    {
        return new[]
        {
            new EnemyTypeConfig
            {
                displayName = "中性粒细胞",
                spawnCount = 4,
                scale = 0.11f,
                maxHealth = 40,
                moveSpeed = 0.95f,
                detectRange = 5.5f,
                shootRange = 6f,
                projectileDamage = 10,
                fireInterval = 2.5f,
                projectileSpeed = 3.5f,
                projectileRange = 8f
            },
            new EnemyTypeConfig
            {
                displayName = "巨噬细胞",
                spawnCount = 4,
                scale = 0.12f,
                maxHealth = 65,
                moveSpeed = 0.85f,
                detectRange = 8f,
                shootRange = 6.5f,
                projectileDamage = 14,
                fireInterval = 2.5f,
                projectileSpeed = 4f,
                projectileRange = 8.5f
            },
            new EnemyTypeConfig
            {
                displayName = "T淋巴细胞",
                spawnCount = 4,
                scale = 0.13f,
                maxHealth = 90,
                moveSpeed = 0.8f,
                detectRange = 10.5f,
                shootRange = 7f,
                projectileDamage = 18,
                fireInterval = 2.5f,
                projectileSpeed = 4.2f,
                projectileRange = 9f
            },
            new EnemyTypeConfig
            {
                displayName = "B淋巴细胞",
                spawnCount = 4,
                scale = 0.14f,
                maxHealth = 110,
                moveSpeed = 0.75f,
                detectRange = 10.5f,
                shootRange = 7f,
                projectileDamage = 22,
                fireInterval = 2.5f,
                projectileSpeed = 4.2f,
                projectileRange = 9f
            },
            new EnemyTypeConfig
            {
                displayName = "NK细胞",
                spawnCount = 4,
                scale = 0.15f,
                maxHealth = 140,
                moveSpeed = 0.7f,
                detectRange = 13f,
                shootRange = 7.5f,
                projectileDamage = 28,
                fireInterval = 2.5f,
                projectileSpeed = 4.5f,
                projectileRange = 9.5f
            }
        };
    }

    private void SpawnAllEnemies()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
            return;

        float spawnX = spawnStartX;
        int yPatternIndex = 0;
        float[] yOffsets = { 0f, verticalSpread, -verticalSpread, verticalSpread * 0.5f, -verticalSpread * 0.5f };

        foreach (EnemyTypeConfig config in enemyTypes)
        {
            for (int i = 0; i < config.spawnCount; i++)
            {
                float spawnY = yOffsets[yPatternIndex % yOffsets.Length];
                yPatternIndex++;

                Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
                spawnX += spawnInterval;

                if (MapBoundsUtility.TryGetBounds(mapRoot, out Bounds mapBounds))
                {
                    spawnPosition.y = Mathf.Clamp(spawnPosition.y,
                        mapBounds.min.y + 1f,
                        mapBounds.max.y - 1f);
                }

                SpawnEnemy(config, spawnPosition);
            }
        }
    }

    private void SpawnEnemy(EnemyTypeConfig config, Vector3 position)
    {
        GameObject enemyObject = new GameObject($"Enemy_{config.displayName}");
        enemyObject.tag = "Enemy";
        enemyObject.transform.SetParent(transform);
        enemyObject.transform.position = position;
        enemyObject.transform.localScale = Vector3.one * config.scale;

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = config.sprite;
        spriteRenderer.sortingOrder = 2;

        Health health = enemyObject.AddComponent<Health>();
        health.Configure(config.maxHealth);

        RangedEnemyController enemyController = enemyObject.AddComponent<RangedEnemyController>();
        enemyController.Configure(
            config.moveSpeed,
            config.detectRange,
            config.shootRange,
            config.projectileDamage,
            config.fireInterval,
            config.projectileSpeed,
            config.projectileRange,
            mapRoot);

        CircleCollider2D enemyCollider = enemyObject.GetComponent<CircleCollider2D>();
        float worldHitRadius = Mathf.Clamp(spriteRenderer.bounds.extents.x * 0.45f, 0.35f, 1.2f);
        enemyCollider.radius = worldHitRadius / config.scale;

        GameObject healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(enemyObject.transform);
        healthBarObject.transform.localScale = Vector3.one / config.scale;
        WorldHealthBar healthBar = healthBarObject.AddComponent<WorldHealthBar>();
        healthBar.Initialize(health, new Vector3(0f, 0.8f, 0f));

        spawnedEnemies.Add(enemyObject);
    }
}
