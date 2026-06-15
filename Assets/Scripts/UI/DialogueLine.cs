using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单条对话数据，对应一个 conversation 面板。
/// </summary>
[Serializable]
public class DialogueLine
{
    public GameObject panel;
    public Text dialogueText;
    [TextArea(2, 6)]
    public string content;
}
