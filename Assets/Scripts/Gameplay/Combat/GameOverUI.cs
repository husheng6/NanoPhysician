using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 失败界面：重新开始当前关卡或返回开始菜单。
/// </summary>
public class GameOverUI : MonoBehaviour
{
    private static GameOverUI instance;

    public static void ResetState()
    {
        instance = null;
    }

    public static void Show()
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(true);
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("CombatUI");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panelObject = new GameObject("GameOverPanel");
        panelObject.transform.SetParent(canvas.transform, false);
        instance = panelObject.AddComponent<GameOverUI>();
        instance.Build(panelObject);
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
        boxRect.sizeDelta = new Vector2(360f, 220f);
        Image boxImage = boxObject.AddComponent<Image>();
        boxImage.sprite = CreateWhiteSprite();
        boxImage.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        CreateLabel(boxObject.transform, "Title", "任务失败", 28, new Vector2(0f, 60f));
        CreateButton(boxObject.transform, "RestartButton", "重新开始", new Vector2(0f, 0f), RestartClicked);
        CreateButton(boxObject.transform, "CancelButton", "取消", new Vector2(0f, -60f), CancelClicked);
    }

    private static void RestartClicked()
    {
        LevelGameFlow.RestartCurrentLevel();
    }

    private static void CancelClicked()
    {
        LevelGameFlow.ReturnToStartScene();
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

    private static void CreateLabel(Transform parent, string name, string text, int fontSize, Vector2 position)
    {
        GameObject labelObject = CreateChild(parent, name);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(320f, 48f);

        Text label = labelObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = text;
    }

    private static void CreateButton(Transform parent, string name, string labelText, Vector2 position, UnityEngine.Events.UnityAction onClick)
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
