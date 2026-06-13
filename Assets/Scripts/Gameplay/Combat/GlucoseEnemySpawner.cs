using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 第一关血糖敌人生成器：按顺序在地图中每隔一段距离生成四类敌人。
/// </summary>
public class GlucoseEnemySpawner : MonoBehaviour, ILevelEnemySpawner
{
    [System.Serializable]
    public class EnemyTypeConfig
    {
        public string displayName;
        public Sprite sprite;
        [Range(3, 5)] public int spawnCount = 4;
        public float scale = 0.12f;
        public int maxHealth = 30;
        public float moveSpeed = 1.5f;
        public float detectRange = 5f;
        public float attackRange = 0.55f;
        public int attackDamage = 10;
        public float attackCooldown = 1.2f;
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
        enemyTypes[0].sprite = LoadSprite("基本血糖游戏素材.png");
        enemyTypes[1].sprite = LoadSprite("二聚体游戏素材.png");
        enemyTypes[2].sprite = LoadSprite("血糖聚合体游戏素材.png");
        enemyTypes[3].sprite = LoadSprite("血糖结晶游戏素材.png");
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
                displayName = "基本血糖",
                spawnCount = 4,
                scale = 0.08f,
                maxHealth = 30,
                moveSpeed = 1.6f,
                detectRange = 5.5f,
                attackRange = 0.65f,
                attackDamage = 8,
                attackCooldown = 0.5f
            },
            new EnemyTypeConfig
            {
                displayName = "二聚体",
                spawnCount = 4,
                scale = 0.12f,
                maxHealth = 55,
                moveSpeed = 1.4f,
                detectRange = 8f,
                attackRange = 0.7f,
                attackDamage = 12,
                attackCooldown = 0.5f
            },
            new EnemyTypeConfig
            {
                displayName = "血糖聚合体",
                spawnCount = 4,
                scale = 0.14f,
                maxHealth = 85,
                moveSpeed = 1.25f,
                detectRange = 10.5f,
                attackRange = 0.75f,
                attackDamage = 16,
                attackCooldown = 0.45f
            },
            new EnemyTypeConfig
            {
                displayName = "血糖结晶",
                spawnCount = 4,
                scale = 0.16f,
                maxHealth = 120,
                moveSpeed = 1.15f,
                detectRange = 13f,
                attackRange = 0.85f,
                attackDamage = 22,
                attackCooldown = 0.4f
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
        float scale = ResolveEnemyScale(config);

        GameObject enemyObject = new GameObject($"Enemy_{config.displayName}");
        enemyObject.tag = "Enemy";
        enemyObject.transform.SetParent(transform);
        enemyObject.transform.position = position;
        enemyObject.transform.localScale = Vector3.one * scale;

        SpriteRenderer spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = config.sprite;
        spriteRenderer.sortingOrder = 2;

        Health health = enemyObject.AddComponent<Health>();
        health.Configure(config.maxHealth);

        EnemyController enemyController = enemyObject.AddComponent<EnemyController>();
        enemyController.Configure(
            config.moveSpeed,
            config.detectRange,
            config.attackRange,
            config.attackDamage,
            config.attackCooldown,
            mapRoot);

        CircleCollider2D enemyCollider = enemyObject.GetComponent<CircleCollider2D>();
        float hitRadius = Mathf.Clamp(spriteRenderer.bounds.extents.x * 0.45f, 0.35f, 1.2f);
        enemyCollider.radius = hitRadius / scale;

        GameObject healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(enemyObject.transform);
        healthBarObject.transform.localScale = Vector3.one / scale;
        WorldHealthBar healthBar = healthBarObject.AddComponent<WorldHealthBar>();
        healthBar.Initialize(health, new Vector3(0f, 0.8f, 0f));

        spawnedEnemies.Add(enemyObject);
    }

    /// <summary>
    /// 基本血糖素材像素尺寸大，缩至约为二聚体的 50% 视觉大小。
    /// </summary>
    private float ResolveEnemyScale(EnemyTypeConfig config)
    {
        if (config.sprite == null)
            return config.scale;

        EnemyTypeConfig dimerConfig = FindConfigByName("二聚体");
        if (dimerConfig == null || dimerConfig.sprite == null)
            return config.scale;

        float dimerWorldHeight = dimerConfig.sprite.bounds.size.y * dimerConfig.scale;

        if (config.displayName == "基本血糖")
            return (dimerWorldHeight * 0.5f) / config.sprite.bounds.size.y;

        return config.scale;
    }

    private EnemyTypeConfig FindConfigByName(string name)
    {
        if (enemyTypes == null)
            return null;

        foreach (EnemyTypeConfig config in enemyTypes)
        {
            if (config.displayName == name)
                return config;
        }

        return null;
    }
}
