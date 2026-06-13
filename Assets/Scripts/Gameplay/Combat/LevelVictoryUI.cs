using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 胜利界面：进入下一关或返回开始菜单。
/// </summary>
public class LevelVictoryUI : MonoBehaviour
{
    private static LevelVictoryUI instance;
    private static string pendingNextScene;

    public static void ResetState()
    {
        instance = null;
        pendingNextScene = null;
    }

    public static void Show(string nextSceneName)
    {
        pendingNextScene = nextSceneName;

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

        GameObject panelObject = new GameObject("VictoryPanel");
        panelObject.transform.SetParent(canvas.transform, false);
        instance = panelObject.AddComponent<LevelVictoryUI>();
        instance.Build(panelObject, !string.IsNullOrEmpty(nextSceneName));
    }

    private void Build(GameObject panelObject, bool showNextLevelButton)
    {
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image overlay = panelObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.65f);

        float boxHeight = showNextLevelButton ? 260f : 200f;
        GameObject boxObject = CreateChild(panelObject.transform, "Box");
        RectTransform boxRect = boxObject.GetComponent<RectTransform>();
        boxRect.sizeDelta = new Vector2(380f, boxHeight);
        Image boxImage = boxObject.AddComponent<Image>();
        boxImage.sprite = CreateWhiteSprite();
        boxImage.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        CreateLabel(boxObject.transform, "Title", "挑战成功", 28, new Vector2(0f, showNextLevelButton ? 70f : 40f));

        if (showNextLevelButton)
        {
            CreateLabel(boxObject.transform, "Message", "是否进入下一关？", 18, new Vector2(0f, 25f));
            CreateButton(boxObject.transform, "NextLevelButton", "进入下一关", new Vector2(0f, -25f), NextLevelClicked);
            CreateButton(boxObject.transform, "HomeButton", "返回首页", new Vector2(0f, -85f), HomeClicked);
        }
        else
        {
            CreateButton(boxObject.transform, "HomeButton", "返回首页", new Vector2(0f, -30f), HomeClicked);
        }
    }

    private static void NextLevelClicked()
    {
        if (string.IsNullOrEmpty(pendingNextScene))
            return;

        LevelGameFlow.GoToNextLevel(pendingNextScene);
    }

    private static void HomeClicked()
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
        rect.sizeDelta = new Vector2(340f, 48f);

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
        rect.sizeDelta = new Vector2(240f, 44f);

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
