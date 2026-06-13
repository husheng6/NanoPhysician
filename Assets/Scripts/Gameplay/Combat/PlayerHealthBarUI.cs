using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕左上角玩家血条，受伤时自动刷新。
/// </summary>
public class PlayerHealthBarUI : MonoBehaviour
{
    private const float BarWidth = 220f;
    private const float BarHeight = 28f;

    [SerializeField] private Health health;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text healthText;

    private static Sprite whiteSprite;

    public static PlayerHealthBarUI Create(Health playerHealth)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("CombatUI");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject barRoot = new GameObject("PlayerHealthBar");
        barRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = barRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = new Vector2(24f, -24f);
        rootRect.sizeDelta = new Vector2(BarWidth, BarHeight);

        Image background = barRoot.AddComponent<Image>();
        background.sprite = GetWhiteSprite();
        background.type = Image.Type.Simple;
        background.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(barRoot.transform, false);
        RectTransform fillTransform = fillObject.AddComponent<RectTransform>();
        fillTransform.anchorMin = new Vector2(0f, 0f);
        fillTransform.anchorMax = new Vector2(0f, 1f);
        fillTransform.pivot = new Vector2(0f, 0.5f);
        fillTransform.anchoredPosition = new Vector2(3f, 0f);
        fillTransform.sizeDelta = new Vector2(BarWidth - 6f, 0f);

        Image fill = fillObject.AddComponent<Image>();
        fill.sprite = GetWhiteSprite();
        fill.type = Image.Type.Simple;
        fill.color = new Color(0.25f, 0.95f, 0.35f, 1f);

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(barRoot.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text label = textObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;

        PlayerHealthBarUI ui = barRoot.AddComponent<PlayerHealthBarUI>();
        ui.health = playerHealth;
        ui.fillRect = fillTransform;
        ui.fillImage = fill;
        ui.healthText = label;
        ui.Refresh(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        playerHealth.OnHealthChanged += ui.Refresh;
        return ui;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        if (fillRect == null || max <= 0)
            return;

        float ratio = Mathf.Clamp01((float)current / max);
        float innerWidth = BarWidth - 6f;
        fillRect.sizeDelta = new Vector2(innerWidth * ratio, fillRect.sizeDelta.y);

        if (fillImage != null)
        {
            fillImage.color = ratio > 0.5f
                ? new Color(0.25f, 0.95f, 0.35f, 1f)
                : ratio > 0.25f
                    ? new Color(0.95f, 0.78f, 0.15f, 1f)
                    : new Color(0.95f, 0.2f, 0.15f, 1f);
        }

        if (healthText != null)
            healthText.text = $"HP {current}/{max}";
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null)
            return whiteSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return whiteSprite;
    }
}
