using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 第一关进入胰岛区域（第三张地图）时的剧情对话。
/// </summary>
public class Level1PancreasDialogueRunner : MonoBehaviour
{
    private const string DoctorPrefabResourcesPath = "Dialogue/conversation1";
    private const string GirlPrefabResourcesPath = "Dialogue/conversation2";
    private const string DoctorPrefabEditorPath = "Assets/perfab/conversation1.prefab";
    private const string GirlPrefabEditorPath = "Assets/perfab/conversation2.prefab";

    private const string DoctorLine1 =
        "这里就是胰岛。正常情况下，这里的 β 细胞会分泌胰岛素。但是小雨的 β 细胞已经被自身免疫系统破坏了。";

    private const string GirlLine =
        "这里... 好荒凉啊...";

    private const string DoctorLine2 =
        "别担心，小雨。从今天起，Alpha-01 会代替你的胰岛工作。";

    [SerializeField] private int dialogueFontSize = 18;

    private Action onComplete;

    public static void Play(Action onComplete = null)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level1PancreasDialogue");
        Level1PancreasDialogueRunner runner = host.AddComponent<Level1PancreasDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        LevelGameFlow.SetIntroActive(true);
        Time.timeScale = 0f;

        Canvas canvas = CombatUiCanvas.GetOrCreate(200);
        Font font = DialogueFont.Get();
        GameObject doctorPrefab = LoadPrefab(DoctorPrefabResourcesPath, DoctorPrefabEditorPath);
        GameObject girlPrefab = LoadPrefab(GirlPrefabResourcesPath, GirlPrefabEditorPath);
        if (doctorPrefab == null || girlPrefab == null || font == null)
        {
            Debug.LogError("Level1PancreasDialogueRunner: 缺少对话预制体或字体资源。");
            FinishDialogue();
            return;
        }

        GameObject doctorPanel = InstantiatePanel(doctorPrefab, canvas.transform, "conversation1_pancreas");
        GameObject girlPanel = InstantiatePanel(girlPrefab, canvas.transform, "conversation2_pancreas");
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
                content = DoctorLine1
            },
            new DialogueLine
            {
                panel = girlPanel,
                dialogueText = girlText,
                content = GirlLine
            },
            new DialogueLine
            {
                panel = doctorPanel,
                dialogueText = doctorText,
                content = DoctorLine2
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
        LevelGameFlow.SetIntroActive(false);
        Time.timeScale = 1f;
        onComplete?.Invoke();
        onComplete = null;
    }

    private static GameObject InstantiatePanel(GameObject prefab, Transform parent, string objectName)
    {
        GameObject instance = Instantiate(prefab, parent);
        instance.name = objectName;
        instance.SetActive(false);
        return instance;
    }

    private static GameObject LoadPrefab(string resourcesPath, string editorPath)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcesPath);
#if UNITY_EDITOR
        if (prefab == null)
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(editorPath);
#endif
        return prefab;
    }
}
