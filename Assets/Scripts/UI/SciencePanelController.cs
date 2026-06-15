using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 科普页面：左侧主题按钮切换右侧科普内容。
/// </summary>
public class SciencePanelController : MonoBehaviour
{
    [Serializable]
    private class ScienceTopic
    {
        public string buttonLabel;
        [TextArea(3, 12)]
        public string content;
    }

    [Header("按钮引用")]
    [SerializeField] private Button closeButton;

    [Header("内容区域（可留空，自动查找名为「内容」的文本）")]
    [SerializeField] private TMP_Text contentTmpText;
    [SerializeField] private Text contentText;

    [Header("科普条目（按钮文字 → 内容）")]
    [SerializeField] private ScienceTopic[] topics =
    {
        new ScienceTopic
        {
            buttonLabel = "糖尿病",
            content = "糖尿病是一种慢性代谢性疾病，患者体内血糖调节出现问题。"
                + "1 型糖尿病患者的胰岛几乎不能分泌胰岛素；2 型糖尿病患者的身体对胰岛素不够敏感。"
                + "若长期血糖过高，可能损害血管、神经、眼、肾等多个器官。"
        },
        new ScienceTopic
        {
            buttonLabel = "胰岛素",
            content = "胰岛素是由胰岛 β 细胞分泌的激素，也是人体内唯一能降低血糖的物质。"
                + "它就像一把钥匙，能打开细胞的大门，让血液中的葡萄糖进入细胞提供能量。"
                + "当胰岛素不足或作用减弱时，葡萄糖会滞留在血液中，血糖就会升高。"
        }
    };

    private readonly Dictionary<string, string> topicLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly List<Button> topicButtons = new List<Button>();

    private void Awake()
    {
        BuildTopicLookup();
        ResolveReferences();
        BindTopicButtons();

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
        ShowDefaultTopic();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void BuildTopicLookup()
    {
        topicLookup.Clear();

        if (topics == null)
            return;

        foreach (ScienceTopic topic in topics)
        {
            if (topic == null || string.IsNullOrWhiteSpace(topic.buttonLabel))
                continue;

            topicLookup[topic.buttonLabel.Trim()] = topic.content ?? string.Empty;
        }
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

        ApplyContentFont();
    }

    private void BindTopicButtons()
    {
        topicButtons.Clear();
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null || button == closeButton || IsCloseButton(button))
                continue;

            string label = GetButtonLabel(button);
            if (string.IsNullOrWhiteSpace(label) || !topicLookup.ContainsKey(label.Trim()))
                continue;

            string topicContent = topicLookup[label.Trim()];
            topicButtons.Add(button);
            button.onClick.AddListener(() => ShowContent(topicContent));
        }
    }

    private void ShowDefaultTopic()
    {
        if (topics != null && topics.Length > 0 && topics[0] != null)
        {
            ShowContent(topics[0].content);
            return;
        }

        ShowContent(string.Empty);
    }

    private void ShowContent(string content)
    {
        if (contentTmpText != null)
            contentTmpText.text = content ?? string.Empty;

        if (contentText != null)
            contentText.text = content ?? string.Empty;
    }

    private static bool IsContentLabel(string objectName)
    {
        return objectName.Equals("内容", StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("Content", StringComparison.OrdinalIgnoreCase)
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
