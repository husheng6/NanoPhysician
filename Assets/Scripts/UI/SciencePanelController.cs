using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 科普页面：左侧可滚动主题列表，右侧展示对应内容。
/// </summary>
public class SciencePanelController : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button closeButton;

    [Header("内容区域（可留空，自动查找）")]
    [SerializeField] private TMP_Text contentTmpText;
    [SerializeField] private Text contentText;

    [Header("左侧目录（可留空，自动查找 science column）")]
    [SerializeField] private RectTransform topicListRoot;

    [Header("列表样式")]
    [SerializeField] private float topicButtonHeight = 30f;
    [SerializeField] private float topicButtonSpacing = 3f;
    [SerializeField] private float topicListInset = 8f;
    [SerializeField] private float topicListInnerPadding = 14f;
    [SerializeField] [Range(0.5f, 1f)] private float topicButtonWidthRatio = 0.8f;
    [SerializeField] private float topicFontSize = 12f;

    private readonly List<Button> topicButtons = new List<Button>();
    private GameObject topicButtonPrototype;
    private TMP_FontAsset topicFontAsset;
    private Material topicFontMaterial;
    private Color topicTextColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    private float topicButtonWidth;
    private bool listBuilt;

    private const string FallbackTopicFontPath = "Assets/1_Asset/4_Font/sarasa-gothic-sc-regular SDF.asset";

    private void Awake()
    {
        ResolveReferences();

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        listBuilt = false;
        EnsureTopicListBuilt();
        ShowDefaultTopic();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void ResolveReferences()
    {
        if (contentTmpText == null || contentText == null)
        {
            TMP_Text[] tmpLabels = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text label in tmpLabels)
            {
                if (label == null || IsButtonLabel(label.transform))
                    continue;

                if (contentTmpText == null && IsContentLabel(label.name))
                    contentTmpText = label;
            }
        }

        if (contentText == null)
        {
            Text[] labels = GetComponentsInChildren<Text>(true);
            foreach (Text label in labels)
            {
                if (label == null || IsButtonLabel(label.transform))
                    continue;

                if (IsContentLabel(label.name))
                {
                    contentText = label;
                    break;
                }
            }
        }

        if (topicListRoot == null)
            topicListRoot = FindRectTransformByName("science column");

        ApplyContentFont();
        ConfigureContentArea();
    }

    private void EnsureTopicButtonPrototype()
    {
        if (topicButtonPrototype != null)
            return;

        Button template = FindTopicButtonTemplate();
        if (template == null)
            return;

        topicButtonPrototype = Instantiate(template.gameObject, transform);
        topicButtonPrototype.name = "TopicButtonPrototype";
        topicButtonPrototype.SetActive(false);

        TMP_Text templateText = topicButtonPrototype.GetComponentInChildren<TMP_Text>(true);
        if (templateText != null)
        {
            topicFontAsset = templateText.font;
            topicFontMaterial = templateText.fontSharedMaterial;
            topicTextColor = templateText.color;
        }
    }

    private void EnsureTopicListBuilt()
    {
        if (topicListRoot == null)
            topicListRoot = FindRectTransformByName("science column");

        if (topicListRoot == null)
            return;

        if (listBuilt)
            return;

        BuildScrollableTopicList();
    }

    private void BuildScrollableTopicList()
    {
        if (topicListRoot == null)
            return;

        EnsureTopicButtonPrototype();

        RemoveExistingTopicListUi(topicListRoot);

        RectTransform contentRoot = CreateScrollRoot(topicListRoot);
        CreateTopicButtons(contentRoot);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);

        listBuilt = true;
    }

    private static void RemoveExistingTopicListUi(RectTransform root)
    {
        Transform oldScroll = root.Find("TopicScrollView");
        if (oldScroll != null)
            Destroy(oldScroll.gameObject);

        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (IsCloseButton(button))
                continue;

            Destroy(button.gameObject);
        }
    }

    private RectTransform CreateScrollRoot(RectTransform parent)
    {
        GameObject scrollObject = new GameObject("TopicScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);
        scrollObject.transform.SetAsFirstSibling();

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        float horizontalInset = topicListInset + topicListInnerPadding * 0.5f;
        scrollRectTransform.offsetMin = new Vector2(horizontalInset, topicListInset);
        scrollRectTransform.offsetMax = new Vector2(-horizontalInset, -topicListInset);

        Image background = scrollObject.GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.02f);
        background.raycastTarget = true;

        Mask mask = scrollObject.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        float listInnerWidth = ResolveTopicListInnerWidth(parent);
        topicButtonWidth = listInnerWidth * topicButtonWidthRatio;

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        Mask viewportMask = viewportObject.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("TopicListContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = topicButtonSpacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(0, 0, 2, 2);

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = contentRect;

        return contentRect;
    }

    private float ResolveTopicListInnerWidth(RectTransform scienceColumn)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scienceColumn);

        float columnWidth = scienceColumn.rect.width;
        if (columnWidth <= 1f)
            columnWidth = scienceColumn.sizeDelta.x;

        float horizontalInset = topicListInset + topicListInnerPadding * 0.5f;
        return Mathf.Max(48f, columnWidth - horizontalInset * 2f);
    }

    private void CreateTopicButtons(RectTransform contentRoot)
    {
        topicButtons.Clear();

        foreach (ScienceKnowledgeDatabase.Entry entry in ScienceKnowledgeDatabase.Topics)
        {
            Button button = CreateTopicButton(contentRoot, entry.Title);
            if (button == null)
                continue;

            string content = entry.Content;
            button.onClick.AddListener(() => ShowContent(content));
            topicButtons.Add(button);
        }
    }

    private Button CreateTopicButton(RectTransform parent, string label)
    {
        if (topicButtonPrototype == null)
            return null;

        GameObject buttonObject = Instantiate(topicButtonPrototype, parent);
        buttonObject.name = $"Topic_{label}";
        buttonObject.SetActive(true);

        StripLayoutConflicts(buttonObject);

        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
            button.onClick.RemoveAllListeners();

        TMP_Text tmp = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            ApplyTopicButtonFont(tmp);
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = tmp.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(3f, 1f);
            textRect.offsetMax = new Vector2(-3f, -1f);
        }

        ApplyTopicButtonLayout(buttonObject);
        return button;
    }

    private void ApplyTopicButtonFont(TMP_Text tmp)
    {
        if (topicFontAsset != null)
        {
            tmp.font = topicFontAsset;
            if (topicFontMaterial != null)
                tmp.fontSharedMaterial = topicFontMaterial;
        }
        else
        {
            TMP_FontAsset fallbackFont = LoadFallbackTopicFontAsset();
            if (fallbackFont != null)
            {
                tmp.font = fallbackFont;
                tmp.fontSharedMaterial = fallbackFont.material;
            }
        }

        tmp.fontSize = topicFontSize;
        tmp.enableAutoSizing = false;
        tmp.color = topicTextColor;
    }

    private static void StripLayoutConflicts(GameObject buttonObject)
    {
        ContentSizeFitter fitter = buttonObject.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            Destroy(fitter);
    }

    private static TMP_FontAsset LoadFallbackTopicFontAsset()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackTopicFontPath);
#else
        return null;
#endif
    }

    private void ApplyTopicButtonLayout(GameObject buttonObject)
    {
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(topicButtonWidth, topicButtonHeight);

        Image image = buttonObject.GetComponent<Image>();
        if (image != null)
        {
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
        }

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = buttonObject.AddComponent<LayoutElement>();

        layoutElement.minWidth = topicButtonWidth;
        layoutElement.preferredWidth = topicButtonWidth;
        layoutElement.flexibleWidth = 0f;
        layoutElement.minHeight = topicButtonHeight;
        layoutElement.preferredHeight = topicButtonHeight;
        layoutElement.flexibleHeight = 0f;
    }

    private void ShowDefaultTopic()
    {
        if (ScienceKnowledgeDatabase.Topics.Length > 0)
            ShowContent(ScienceKnowledgeDatabase.Topics[0].Content);
        else
            ShowContent(string.Empty);
    }

    private void ShowContent(string content)
    {
        if (contentTmpText != null)
        {
            contentTmpText.text = content ?? string.Empty;
            contentTmpText.enableWordWrapping = true;
            contentTmpText.overflowMode = TextOverflowModes.Overflow;
        }

        if (contentText != null)
        {
            contentText.text = content ?? string.Empty;
            contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            contentText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    private void ConfigureContentArea()
    {
        if (contentTmpText == null)
            return;

        contentTmpText.enableWordWrapping = true;
        contentTmpText.overflowMode = TextOverflowModes.Overflow;
        contentTmpText.alignment = TextAlignmentOptions.TopLeft;
        contentTmpText.margin = new Vector4(8f, 8f, 8f, 8f);
    }

    private Button FindTopicButtonTemplate()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null || button == closeButton || IsCloseButton(button))
                continue;

            if (GetButtonLabel(button) == "糖尿病" || button.gameObject.name == "diabetes")
                return button;
        }

        foreach (Button button in buttons)
        {
            if (button == null || button == closeButton || IsCloseButton(button))
                continue;

            return button;
        }

        return null;
    }

    private RectTransform FindRectTransformByName(string objectName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            if (child != null && child.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
                return child as RectTransform;
        }

        return null;
    }

    private static bool IsContentLabel(string objectName)
    {
        return objectName.Equals("内容", StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("Content", StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("course content", StringComparison.OrdinalIgnoreCase)
            || objectName.IndexOf("content", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsCloseButton(Button button)
    {
        string name = button.gameObject.name;
        return name.IndexOf("exit", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("关闭", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsButtonLabel(Transform transform)
    {
        return transform.GetComponentInParent<Button>() != null;
    }

    private static string GetButtonLabel(Button button)
    {
        TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null && !string.IsNullOrWhiteSpace(tmp.text))
            return tmp.text.Trim();

        Text label = button.GetComponentInChildren<Text>(true);
        if (label != null && !string.IsNullOrWhiteSpace(label.text))
            return label.text.Trim();

        return button.gameObject.name.Trim();
    }

    private void ApplyContentFont()
    {
        Font font = DialogueFont.Get();
        if (font == null)
            return;

        if (contentText != null)
        {
            contentText.font = font;
            contentText.alignment = TextAnchor.UpperLeft;
            contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            contentText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);

        foreach (Button button in topicButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }
}
