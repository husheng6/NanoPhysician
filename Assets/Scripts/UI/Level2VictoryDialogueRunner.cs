using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 第二关通关并完成商店后：展示「过程剧情2」并播放林墨博士与小雨对话。
/// </summary>
public class Level2VictoryDialogueRunner : MonoBehaviour
{
    private const string Level2SceneName = "level2Scence";
    private const string BackgroundResourcesPath = "Dialogue/过程剧情2";
    private const string BackgroundEditorPath = "Assets/art assets/过程剧情2.png";
    private const string DoctorPrefabResourcesPath = "Dialogue/conversation1";
    private const string GirlPrefabResourcesPath = "Dialogue/conversation2";
    private const string DoctorPrefabEditorPath = "Assets/perfab/conversation1.prefab";
    private const string GirlPrefabEditorPath = "Assets/perfab/conversation2.prefab";

    [SerializeField] private int dialogueFontSize = 18;

    private Action onComplete;
    private GameObject backgroundObject;
    private GameObject doctorPanel;
    private GameObject girlPanel;

    public static bool ShouldPlayForCurrentScene()
    {
        return SceneManager.GetActiveScene().name == Level2SceneName;
    }

    public static void Play(Action onComplete)
    {
        GameObject host = new GameObject("Level2VictoryDialogue");
        Level2VictoryDialogueRunner runner = host.AddComponent<Level2VictoryDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Time.timeScale = 0f;

        Canvas canvas = CombatUiCanvas.GetOrCreate(280);
        Font font = DialogueFont.Get();
        GameObject doctorPrefab = StoryDialogueUiHelper.LoadPrefab(DoctorPrefabResourcesPath, DoctorPrefabEditorPath);
        GameObject girlPrefab = StoryDialogueUiHelper.LoadPrefab(GirlPrefabResourcesPath, GirlPrefabEditorPath);
        if (doctorPrefab == null || girlPrefab == null || font == null)
        {
            Debug.LogError("Level2VictoryDialogueRunner: 缺少对话预制体或字体资源。");
            FinishDialogue();
            return;
        }

        Sprite background = StoryDialogueUiHelper.LoadSprite(BackgroundResourcesPath, BackgroundEditorPath);
        backgroundObject = StoryDialogueUiHelper.CreateFullScreenBackground(canvas.transform, background);
        doctorPanel = StoryDialogueUiHelper.InstantiatePanel(doctorPrefab, canvas.transform, "conversation1_level2_victory");
        girlPanel = StoryDialogueUiHelper.InstantiatePanel(girlPrefab, canvas.transform, "conversation2_level2_victory");

        Text doctorText = ConversationDialogueHelper.EnsureDialogueText(
            doctorPanel.transform, font, dialogueFontSize, Color.white);
        Text girlText = ConversationDialogueHelper.EnsureDialogueText(
            girlPanel.transform, font, dialogueFontSize, Color.white);

        DialogueLine[] lines =
        {
            new DialogueLine
            {
                panel = doctorPanel,
                dialogueText = doctorText,
                content = "林墨博士：Alpha-01，现在整个免疫系统都在攻击你。我... 我不知道该怎么办了。"
            },
            new DialogueLine
            {
                panel = girlPanel,
                dialogueText = girlText,
                content = "林小雨：爸爸，我感觉身体里在打仗... 好难受..."
            },
            new DialogueLine
            {
                panel = doctorPanel,
                dialogueText = doctorText,
                content = "林墨博士：对不起，小雨。爸爸以为科技可以解决一切问题。但我错了。人体是一个经过亿万年进化的完美系统，不是我们可以随意修改的机器。"
            },
            new DialogueLine
            {
                panel = doctorPanel,
                dialogueText = doctorText,
                content = "林墨博士：Alpha-01，我命令你... 停止战斗。"
            }
        };

        StreamingDialogueController dialogueController = gameObject.AddComponent<StreamingDialogueController>();
        dialogueController.Configure(lines);
        dialogueController.OnDialogueComplete += HandleDialogueComplete;
        dialogueController.StartDialogue();
    }

    private void HandleDialogueComplete()
    {
        FinishDialogue();
        Destroy(gameObject);
    }

    private void FinishDialogue()
    {
        if (backgroundObject != null)
            Destroy(backgroundObject);

        if (doctorPanel != null)
            Destroy(doctorPanel);

        if (girlPanel != null)
            Destroy(girlPanel);

        onComplete?.Invoke();
        onComplete = null;
    }
}
