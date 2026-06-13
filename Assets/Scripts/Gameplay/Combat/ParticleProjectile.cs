using UnityEngine;

/// <summary>
/// 玩家弹丸：沿方向飞行，命中一个敌人后消失，飞行一定距离后自动销毁。
/// </summary>
public class ParticleProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float maxTravelDistance = 7f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float hitRadius = 0.28f;

    private Vector2 direction;
    private Vector3 startPosition;
    private bool hasHit;
    private SpriteRenderer coreRenderer;
    private ParticleSystem trailParticles;

    public void Launch(Vector2 launchDirection, int projectileDamage, float projectileSpeed, float travelDistance)
    {
        direction = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : Vector2.right;
        damage = projectileDamage;
        speed = projectileSpeed;
        maxTravelDistance = travelDistance;
        startPosition = transform.position;
        transform.right = direction;

        BuildVisuals();
    }

    private void BuildVisuals()
    {
        coreRenderer = gameObject.AddComponent<SpriteRenderer>();
        coreRenderer.sprite = CreateCircleSprite(32);
        coreRenderer.color = new Color(0.3f, 1f, 1f, 1f);
        coreRenderer.sortingOrder = 10;
        transform.localScale = Vector3.one * 0.35f;

        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(transform, false);
        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = CreateCircleSprite(32);
        glowRenderer.color = new Color(0.5f, 0.95f, 1f, 0.45f);
        glowRenderer.sortingOrder = 9;
        glowObject.transform.localScale = Vector3.one * 1.8f;

        trailParticles = gameObject.AddComponent<ParticleSystem>();
        var main = trailParticles.main;
        main.startLifetime = 0.18f;
        main.startSpeed = 0.2f;
        main.startSize = 0.18f;
        main.maxParticles = 64;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.4f, 0.95f, 1f, 0.9f);

        var emission = trailParticles.emission;
        emission.rateOverTime = 80f;

        var renderer = trailParticles.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 8;
        trailParticles.Play();
    }

    private void Update()
    {
        if (hasHit)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPosition, transform.position) >= maxTravelDistance)
        {
            Destroy(gameObject);
            return;
        }

        TryHitEnemy();
    }

    private void TryHitEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
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
            hasHit = true;
            Destroy(gameObject);
            return;
        }
    }

    private static Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        float center = size * 0.5f;
        float radius = center - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = dist <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
