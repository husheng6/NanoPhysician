using UnityEngine;

/// <summary>
/// 敌人射击弹丸，命中玩家后造成伤害并销毁。
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private float maxTravelDistance = 8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float hitRadius = 0.25f;

    private Vector2 direction;
    private Vector3 startPosition;
    private bool hasHit;
    private SpriteRenderer coreRenderer;

    public void Launch(Vector2 launchDirection, int projectileDamage, float projectileSpeed, float travelDistance)
    {
        direction = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : Vector2.left;
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
        coreRenderer.color = new Color(1f, 0.35f, 0.25f, 1f);
        coreRenderer.sortingOrder = 10;
        transform.localScale = Vector3.one * 0.3f;

        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(transform, false);
        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = CreateCircleSprite(32);
        glowRenderer.color = new Color(1f, 0.5f, 0.2f, 0.4f);
        glowRenderer.sortingOrder = 9;
        glowObject.transform.localScale = Vector3.one * 1.6f;
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

        TryHitPlayer();
    }

    private void TryHitPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        float playerRadius = playerSprite != null
            ? Mathf.Max(playerSprite.bounds.extents.x, playerSprite.bounds.extents.y)
            : 0.35f;

        if (distance > hitRadius + playerRadius)
            return;

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
            playerStats.TakeDamage(damage);
        else
            player.GetComponent<Health>()?.TakeDamage(damage);

        hasHit = true;
        Destroy(gameObject);
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
                texture.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
