using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 流式打字机对话：在 conversation 面板中逐字显示，点击或空格跳过/下一句。
/// </summary>
public class StreamingDialogueController : MonoBehaviour
{
    [Header("对话")]
    [SerializeField] private DialogueLine[] lines;
    [SerializeField] private float charactersPerSecond = 18f;
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;

    public event Action OnDialogueComplete;

    private int _currentLineIndex;
    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private bool _isRunning;
    private readonly StringBuilder _textBuilder = new StringBuilder(256);

    private bool _configuredExternally;

    public void Configure(DialogueLine[] dialogueLines)
    {
        lines = dialogueLines;
        _configuredExternally = true;
    }

    private void Start()
    {
        if (!_configuredExternally && lines != null && lines.Length > 0)
            StartDialogue();
    }

    public void StartDialogue()
    {
        if (_isRunning || lines == null || lines.Length == 0)
            return;

        if (!ValidateLines())
            return;

        _isRunning = true;
        _currentLineIndex = 0;
        ShowLine(_currentLineIndex);
    }

    private bool ValidateLines()
    {
        foreach (DialogueLine line in lines)
        {
            if (line.dialogueText == null)
            {
                Debug.LogError("StreamingDialogueController: 存在未绑定文字的 conversation 面板。");
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(advanceKey))
            return;

        if (_isTyping)
            CompleteCurrentLine();
        else
            AdvanceLine();
    }

    private void ShowLine(int index)
    {
        SetUniquePanelsActive(false);

        DialogueLine line = lines[index];
        if (line.panel != null)
            line.panel.SetActive(true);

        if (line.dialogueText != null)
            ConversationDialogueHelper.ApplyDialogueTextStyle(line.dialogueText, DialogueFont.Get(), line.dialogueText.fontSize, line.dialogueText.color);

        line.dialogueText.text = string.Empty;

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(DialogueLine line)
    {
        _isTyping = true;
        _textBuilder.Clear();
        float interval = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;

        for (int i = 0; i < line.content.Length; i++)
        {
            _textBuilder.Append(line.content[i]);
            line.dialogueText.text = _textBuilder.ToString();

            if (interval > 0f)
                yield return new WaitForSecondsRealtime(interval);
        }

        _isTyping = false;
        _typingCoroutine = null;
    }

    private void CompleteCurrentLine()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        lines[_currentLineIndex].dialogueText.text = lines[_currentLineIndex].content;
        _isTyping = false;
    }

    private void AdvanceLine()
    {
        _currentLineIndex++;

        if (_currentLineIndex >= lines.Length)
        {
            FinishDialogue();
            return;
        }

        ShowLine(_currentLineIndex);
    }

    private void FinishDialogue()
    {
        _isRunning = false;
        SetUniquePanelsActive(false);
        OnDialogueComplete?.Invoke();
    }

    private void SetUniquePanelsActive(bool active)
    {
        HashSet<GameObject> seen = new HashSet<GameObject>();
        foreach (DialogueLine line in lines)
        {
            if (line.panel == null || !seen.Add(line.panel))
                continue;

            line.panel.SetActive(active);
        }
    }
}
