using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 返回关卡选择确认弹窗。
/// </summary>
public class LevelReturnConfirmUI : MonoBehaviour
{
    private static LevelReturnConfirmUI instance;

    public static void ResetState()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }

    public static void Show()
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(true);
            return;
        }

        Canvas canvas = CombatUiCanvas.GetOrCreate(260);
        GameObject panelObject = new GameObject("LevelReturnConfirmPanel");
        panelObject.transform.SetParent(canvas.transform, false);
        instance = panelObject.AddComponent<LevelReturnConfirmUI>();
        instance.Build(panelObject);
    }

    public static void Hide()
    {
        if (instance != null)
            instance.gameObject.SetActive(false);
    }

    private void Build(GameObject panelObject)
    {
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image overlay = panelObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject boxObject = CreateChild(panelObject.transform, "Box");
        RectTransform boxRect = boxObject.GetComponent<RectTransform>();
        boxRect.sizeDelta = new Vector2(420f, 260f);
        Image boxImage = boxObject.AddComponent<Image>();
        boxImage.sprite = CreateWhiteSprite();
        boxImage.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        CreateLabel(boxObject.transform, "Message", "是否返回到关卡页面，重新选择关卡？", 20, new Vector2(0f, 45f), 380f);
        CreateButton(boxObject.transform, "YesButton", "是", new Vector2(0f, -25f), ConfirmClicked);
        CreateButton(boxObject.transform, "NoButton", "否", new Vector2(0f, -85f), CancelClicked);
    }

    private static void ConfirmClicked()
    {
        LevelGameFlow.ConfirmReturnToLevelSelection();
    }

    private static void CancelClicked()
    {
        LevelGameFlow.CancelReturnToLevelSelection();
    }

    private static GameObject CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        RectTransform rect = child.AddComponent<RectTransform>();
        child.transform.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        return child;
    }

    private static void CreateLabel(
        Transform parent,
        string name,
        string text,
        int fontSize,
        Vector2 position,
        float width)
    {
        GameObject labelObject = CreateChild(parent, name);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, 72f);

        Text label = labelObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = text;
    }

    private static void CreateButton(
        Transform parent,
        string name,
        string labelText,
        Vector2 position,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateChild(parent, name);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(220f, 44f);

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = CreateWhiteSprite();
        image.color = new Color(0.22f, 0.45f, 0.85f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = labelText;
    }

    private static Sprite whiteSprite;

    private static Sprite CreateWhiteSprite()
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
