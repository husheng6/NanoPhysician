using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 第三关通关并完成商店后：展示「过程剧情3」并播放林墨博士与小雨对话。
/// </summary>
public class Level3VictoryDialogueRunner : MonoBehaviour
{
    private const string Level3SceneName = "level3Scence";
    private const string BackgroundResourcesPath = "Dialogue/过程剧情3";
    private const string BackgroundEditorPath = "Assets/art assets/过程剧情3.png";
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
        return SceneManager.GetActiveScene().name == Level3SceneName;
    }

    public static void Play(Action onComplete)
    {
        GameObject host = new GameObject("Level3VictoryDialogue");
        Level3VictoryDialogueRunner runner = host.AddComponent<Level3VictoryDialogueRunner>();
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
            Debug.LogError("Level3VictoryDialogueRunner: 缺少对话预制体或字体资源。");
            FinishDialogue();
            return;
        }

        Sprite background = StoryDialogueUiHelper.LoadSprite(BackgroundResourcesPath, BackgroundEditorPath);
        backgroundObject = StoryDialogueUiHelper.CreateFullScreenBackground(canvas.transform, background);
        doctorPanel = StoryDialogueUiHelper.InstantiatePanel(doctorPrefab, canvas.transform, "conversation1_level3_victory");
        girlPanel = StoryDialogueUiHelper.InstantiatePanel(girlPrefab, canvas.transform, "conversation2_level3_victory");

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
                content = "林墨博士：Alpha-01，谢谢你。你教会了我一个重要的道理：科技永远不能代替自然，但可以成为自然的好帮手。"
            },
            new DialogueLine
            {
                panel = girlPanel,
                dialogueText = girlText,
                content = "林小雨：爸爸，我饿了。"
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
