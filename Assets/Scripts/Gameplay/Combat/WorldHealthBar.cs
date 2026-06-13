using UnityEngine;

/// <summary>
/// 世界空间血条，跟随宿主显示在头顶。
/// </summary>
public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private Vector2 barSize = new Vector2(1.2f, 0.12f);

    private Transform fillTransform;
    private Health health;
    private float fullWidth;
    private bool useWorldFollow;

    public void Initialize(Health targetHealth, Vector3 customOffset, bool worldFollow = false)
    {
        health = targetHealth;
        offset = customOffset;
        useWorldFollow = worldFollow;
        BuildBarVisuals();

        if (!worldFollow)
            transform.localPosition = offset;

        health.OnHealthChanged += UpdateBar;
        UpdateBar(health.CurrentHealth, health.MaxHealth);
    }

    private void Awake()
    {
        health = GetComponentInParent<Health>();
        if (health != null && fillTransform == null)
        {
            BuildBarVisuals();
            health.OnHealthChanged += UpdateBar;
            UpdateBar(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void LateUpdate()
    {
        if (health == null || !useWorldFollow)
            return;

        transform.position = health.transform.position + offset;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateBar;
    }

    private void BuildBarVisuals()
    {
        if (fillTransform != null)
            return;

        fullWidth = barSize.x;

        Sprite whiteSprite = CreateWhiteSprite();

        GameObject background = new GameObject("BarBackground");
        background.transform.SetParent(transform, false);
        SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whiteSprite;
        bgRenderer.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        bgRenderer.sortingOrder = 20;
        background.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);

        GameObject fill = new GameObject("BarFill");
        fill.transform.SetParent(transform, false);
        fillTransform = fill.transform;
        SpriteRenderer fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = whiteSprite;
        fillRenderer.color = new Color(0.2f, 0.9f, 0.3f, 1f);
        fillRenderer.sortingOrder = 21;
        fill.transform.localPosition = new Vector3(-fullWidth * 0.5f, 0f, 0f);
        fill.transform.localScale = new Vector3(fullWidth, barSize.y, 1f);
    }

    private void UpdateBar(int current, int max)
    {
        if (fillTransform == null || max <= 0)
            return;

        float ratio = (float)current / max;
        fillTransform.localScale = new Vector3(fullWidth * ratio, barSize.y, 1f);
        fillTransform.localPosition = new Vector3(-fullWidth * 0.5f + fullWidth * ratio * 0.5f, 0f, 0f);

        SpriteRenderer fillRenderer = fillTransform.GetComponent<SpriteRenderer>();
        if (fillRenderer != null)
        {
            fillRenderer.color = ratio > 0.5f
                ? new Color(0.2f, 0.9f, 0.3f, 1f)
                : ratio > 0.25f
                    ? new Color(0.95f, 0.75f, 0.1f, 1f)
                    : new Color(0.95f, 0.2f, 0.15f, 1f);
        }
    }

    private static Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
