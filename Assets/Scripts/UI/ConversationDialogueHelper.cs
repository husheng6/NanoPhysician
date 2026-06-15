using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// conversation 面板文字绑定工具。
/// </summary>
public static class ConversationDialogueHelper
{
    public static Text EnsureDialogueText(Transform conversationPanel, Font font, int fontSize, Color color)
    {
        Transform dialogueBox = FindDialogueBoxTransform(conversationPanel);
        Transform existing = dialogueBox.Find("DialogueText");
        if (existing != null)
        {
            Text text = existing.GetComponent<Text>();
            ApplyDialogueTextStyle(text, font, fontSize, color);
            return text;
        }

        Text dialogueText = CreateText(dialogueBox, "DialogueText");
        ApplyDialogueTextStyle(dialogueText, font, fontSize, color);
        return dialogueText;
    }

    public static Transform FindDialogueBoxTransform(Transform conversationPanel)
    {
        Image dialogueBox = null;

        foreach (Image image in conversationPanel.GetComponentsInChildren<Image>(true))
        {
            if (image.transform == conversationPanel)
                continue;

            float area = image.rectTransform.sizeDelta.x * image.rectTransform.sizeDelta.y;
            if (dialogueBox == null || area > dialogueBox.rectTransform.sizeDelta.x * dialogueBox.rectTransform.sizeDelta.y)
                dialogueBox = image;
        }

        return dialogueBox != null ? dialogueBox.transform : conversationPanel;
    }

    public static void ApplyDialogueTextStyle(Text text, Font font, int fontSize, Color color)
    {
        if (text == null)
            return;

        Font dialogueFont = font != null ? font : DialogueFont.Get();
        if (dialogueFont != null)
            text.font = dialogueFont;

        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Normal;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = false;
    }

    private static Text CreateText(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Text text = go.AddComponent<Text>();
        text.text = string.Empty;
        text.font = DialogueFont.Get();

        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(72f, 12f);
        rect.offsetMax = new Vector2(-32f, -12f);
        return text;
    }
}
