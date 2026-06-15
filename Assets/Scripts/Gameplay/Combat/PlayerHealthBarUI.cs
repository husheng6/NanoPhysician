using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕左上角玩家血条与蓝条。优先绑定 Main Camera 上已摆放的 UI，找不到时再动态创建。
/// </summary>
[DisallowMultipleComponent]
public class PlayerHealthBarUI : MonoBehaviour
{
    private const float BarWidth = 220f;
    private const float HealthBarHeight = 28f;
    private const float ManaBarHeight = 22f;
    private const float BarSpacing = 8f;

    [SerializeField] private Health health;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private RectTransform healthFillRect;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Text healthText;
    [SerializeField] private TMP_Text healthTmpText;
    [SerializeField] private RectTransform manaFillRect;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private Text manaText;
    [SerializeField] private TMP_Text manaTmpText;

    private static Sprite whiteSprite;

    public static void TryInitialize(Health playerHealth, PlayerStats stats)
    {
        if (playerHealth == null)
            return;

        PlayerHealthBarUI existing = Object.FindObjectOfType<PlayerHealthBarUI>();
        if (existing != null)
        {
            existing.Bind(playerHealth, stats);
            if (existing.HasBarUI())
                return;

            Object.Destroy(existing);
        }

        Create(playerHealth, stats);
    }

    private void Awake()
    {
        if (health != null)
            EnsureInitialized();
    }

    public static PlayerHealthBarUI Create(Health playerHealth, PlayerStats stats)
    {
        Canvas canvas = CombatUiCanvas.GetOrCreate();

        GameObject root = new GameObject("PlayerStatusBars");
        root.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = new Vector2(24f, -24f);
        rootRect.sizeDelta = new Vector2(BarWidth, HealthBarHeight + BarSpacing + ManaBarHeight);

        CreateBar(
            root.transform,
            "PlayerHealthBar",
            new Vector2(0f, 0f),
            HealthBarHeight,
            new Color(0.25f, 0.95f, 0.35f, 1f),
            out RectTransform healthFill,
            out Image healthFillImg,
            out Text healthLabel);

        CreateBar(
            root.transform,
            "PlayerManaBar",
            new Vector2(0f, -(HealthBarHeight + BarSpacing)),
            ManaBarHeight,
            new Color(0.25f, 0.55f, 0.98f, 1f),
            out RectTransform manaFill,
            out Image manaFillImg,
            out Text manaLabel);

        PlayerHealthBarUI ui = root.AddComponent<PlayerHealthBarUI>();
        ui.healthFillRect = healthFill;
        ui.healthFillImage = healthFillImg;
        ui.healthText = healthLabel;
        ui.manaFillRect = manaFill;
        ui.manaFillImage = manaFillImg;
        ui.manaText = manaLabel;
        ui.Bind(playerHealth, stats);
        return ui;
    }

    private bool HasBarUI()
    {
        return healthFillRect != null && manaFillRect != null;
    }

    private void Bind(Health playerHealth, PlayerStats stats)
    {
        if (health != null)
            health.OnHealthChanged -= RefreshHealth;

        if (playerStats != null)
            playerStats.OnManaChanged -= RefreshMana;

        health = playerHealth;
        playerStats = stats;
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        ResolveReferences();

        if (health == null)
            return;

        health.OnHealthChanged -= RefreshHealth;
        health.OnHealthChanged += RefreshHealth;
        RefreshHealth(health.CurrentHealth, health.MaxHealth);

        if (playerStats != null)
        {
            playerStats.OnManaChanged -= RefreshMana;
            playerStats.OnManaChanged += RefreshMana;
            RefreshMana(playerStats.CurrentMana, playerStats.MaxMana);
        }
    }

    private void ResolveReferences()
    {
        if (healthFillRect == null || healthFillImage == null || (healthText == null && healthTmpText == null))
            TryResolveBar("PlayerHealthBar", "HealthBar", "HP",
                out healthFillRect, out healthFillImage, out healthText, out healthTmpText);

        if (manaFillRect == null || manaFillImage == null || (manaText == null && manaTmpText == null))
            TryResolveBar("PlayerManaBar", "ManaBar", "MP",
                out manaFillRect, out manaFillImage, out manaText, out manaTmpText);
    }

    private void TryResolveBar(
        string primaryName,
        string secondaryName,
        string shortName,
        out RectTransform fillRect,
        out Image fillImage,
        out Text label,
        out TMP_Text tmpLabel)
    {
        fillRect = null;
        fillImage = null;
        label = null;
        tmpLabel = null;

        Transform barRoot = FindChildTransform(transform, primaryName)
            ?? FindChildTransform(transform, secondaryName)
            ?? FindChildTransform(transform, shortName);
        if (barRoot == null)
            return;

        Transform fillTransform = barRoot.Find("Fill");
        if (fillTransform != null)
        {
            fillRect = fillTransform.GetComponent<RectTransform>();
            fillImage = fillTransform.GetComponent<Image>();
        }

        Transform labelTransform = barRoot.Find("Label");
        if (labelTransform != null)
        {
            label = labelTransform.GetComponent<Text>();
            tmpLabel = labelTransform.GetComponent<TMP_Text>();
        }

        if (label == null && tmpLabel == null)
        {
            label = barRoot.GetComponentInChildren<Text>(true);
            tmpLabel = barRoot.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private static Transform FindChildTransform(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (root.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase)
            || root.name.IndexOf(childName, System.StringComparison.OrdinalIgnoreCase) >= 0)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildTransform(root.GetChild(i), childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private static void CreateBar(
        Transform parent,
        string barName,
        Vector2 anchoredPosition,
        float barHeight,
        Color fillColor,
        out RectTransform fillRect,
        out Image fillImage,
        out Text label)
    {
        GameObject barRoot = new GameObject(barName);
        barRoot.transform.SetParent(parent, false);

        RectTransform rootRect = barRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = anchoredPosition;
        rootRect.sizeDelta = new Vector2(BarWidth, barHeight);

        Image background = barRoot.AddComponent<Image>();
        background.sprite = GetWhiteSprite();
        background.type = Image.Type.Simple;
        background.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(barRoot.transform, false);
        fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(3f, 0f);
        fillRect.sizeDelta = new Vector2(BarWidth - 6f, 0f);

        fillImage = fillObject.AddComponent<Image>();
        fillImage.sprite = GetWhiteSprite();
        fillImage.type = Image.Type.Simple;
        fillImage.color = fillColor;

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(barRoot.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        label = textObject.AddComponent<Text>();
        label.font = DialogueFont.Get() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= RefreshHealth;

        if (playerStats != null)
            playerStats.OnManaChanged -= RefreshMana;
    }

    private void RefreshHealth(int current, int max)
    {
        if (healthFillRect == null || max <= 0)
            return;

        float ratio = Mathf.Clamp01((float)current / max);
        float innerWidth = GetBarInnerWidth(healthFillRect);
        healthFillRect.sizeDelta = new Vector2(innerWidth * ratio, healthFillRect.sizeDelta.y);

        if (healthFillImage != null)
        {
            healthFillImage.color = ratio > 0.5f
                ? new Color(0.25f, 0.95f, 0.35f, 1f)
                : ratio > 0.25f
                    ? new Color(0.95f, 0.78f, 0.15f, 1f)
                    : new Color(0.95f, 0.2f, 0.15f, 1f);
        }

        string label = $"HP {current}/{max}";
        if (healthText != null)
            healthText.text = label;
        if (healthTmpText != null)
            healthTmpText.text = label;
    }

    private void RefreshMana(int current, int max)
    {
        if (manaFillRect == null || max <= 0)
            return;

        float ratio = Mathf.Clamp01((float)current / max);
        float innerWidth = GetBarInnerWidth(manaFillRect);
        manaFillRect.sizeDelta = new Vector2(innerWidth * ratio, manaFillRect.sizeDelta.y);

        if (manaFillImage != null)
            manaFillImage.color = new Color(0.25f, 0.55f, 0.98f, 1f);

        string label = $"MP {current}/{max}";
        if (manaText != null)
            manaText.text = label;
        if (manaTmpText != null)
            manaTmpText.text = label;
    }

    private static float GetBarInnerWidth(RectTransform fillRect)
    {
        RectTransform barRoot = fillRect.parent as RectTransform;
        if (barRoot != null && barRoot.sizeDelta.x > 0f)
            return Mathf.Max(0f, barRoot.sizeDelta.x - 6f);

        return BarWidth - 6f;
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
