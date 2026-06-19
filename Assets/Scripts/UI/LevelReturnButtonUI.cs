using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 右上角返回按钮：关卡内返回选关页，选关页返回开始菜单。
/// </summary>
public static class LevelReturnButtonUI
{
    private const string StartSceneName = "startScene";
    private const string ButtonSpriteResourcesPath = "Shop/返回按钮";
    private const string ButtonSpriteEditorPath = "Assets/Resources/Shop/返回按钮.png";
    private const float ButtonSize = 52f;
    private const float EdgeMargin = 16f;

    private static GameObject buttonRoot;

    public static void ResetState()
    {
        if (buttonRoot != null)
        {
            Object.Destroy(buttonRoot);
            buttonRoot = null;
        }
    }

    public static void SetupForLevel()
    {
        EnsureEventSystem();
        CreateTopRightButton(
            CombatUiCanvas.GetOrCreate(120).transform,
            LevelGameFlow.RequestReturnToLevelSelection);
    }

    public static void SetupForLevelSelection(Transform canvasTransform)
    {
        if (canvasTransform == null)
            return;

        EnsureEventSystem();
        FixCanvasTransform(canvasTransform);
        CreateTopRightButton(canvasTransform, ReturnToStartScene);
    }

    private static void FixCanvasTransform(Transform canvasTransform)
    {
        RectTransform canvasRect = canvasTransform as RectTransform;
        if (canvasRect == null)
            canvasRect = canvasTransform.GetComponent<RectTransform>();

        if (canvasRect != null && canvasRect.localScale == Vector3.zero)
            canvasRect.localScale = Vector3.one;
    }

    private static void ReturnToStartScene()
    {
        SceneLoader.Load(StartSceneName);
    }

    private static void CreateTopRightButton(Transform parent, UnityEngine.Events.UnityAction onClick)
    {
        ResetState();

        Sprite buttonSprite = LoadButtonSprite();
        if (buttonSprite == null)
        {
            Debug.LogWarning("LevelReturnButtonUI: 找不到返回按钮贴图。");
            return;
        }

        buttonRoot = new GameObject("ReturnButton");
        buttonRoot.transform.SetParent(parent, false);
        buttonRoot.transform.SetAsLastSibling();

        RectTransform rect = buttonRoot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-EdgeMargin, -EdgeMargin);
        rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);

        Image image = buttonRoot.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button = buttonRoot.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
    }

    private static Sprite LoadButtonSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(ButtonSpriteResourcesPath);
#if UNITY_EDITOR
        if (sprite == null)
        {
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpriteEditorPath);
            if (sprite == null)
            {
                Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(ButtonSpriteEditorPath);
                if (texture != null)
                {
                    sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }
            }
        }
#endif
        return sprite;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
}
