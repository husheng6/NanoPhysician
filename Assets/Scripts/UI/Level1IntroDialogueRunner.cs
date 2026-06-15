using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡一进入游戏时的引导对话。
/// </summary>
public class Level1IntroDialogueRunner : MonoBehaviour
{
    private const string DoctorPrefabResourcesPath = "Dialogue/conversation1";
    private const string SciencePrefabResourcesPath = "Dialogue/conversation3";
    private const string DoctorPrefabEditorPath = "Assets/perfab/conversation1.prefab";
    private const string SciencePrefabEditorPath = "Assets/perfab/conversation3.prefab";

    private const string DoctorLine =
        "Alpha-01，听得见吗？我是林墨博士。你的任务很简单，看到那些红色的小球了吗？它们就是血糖。当它们数量太多时，就会对人体造成伤害。";

    private const string ScienceLine =
        "胰岛素是唯一能降低血糖的激素。它就像一把钥匙，打开细胞的大门，让葡萄糖进入细胞提供能量。没有胰岛素，葡萄糖只能留在血液里。";

    [SerializeField] private int dialogueFontSize = 18;

    private Action _onComplete;

    public static void Play(Action onComplete)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level1IntroDialogue");
        Level1IntroDialogueRunner runner = host.AddComponent<Level1IntroDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action onComplete)
    {
        _onComplete = onComplete;
        LevelGameFlow.SetIntroActive(true);
        Time.timeScale = 0f;

        Canvas canvas = GetDialogueCanvas();
        Font font = DialogueFont.Get();

        GameObject doctorPrefab = LoadPrefab(DoctorPrefabResourcesPath, DoctorPrefabEditorPath);
        GameObject sciencePrefab = LoadPrefab(SciencePrefabResourcesPath, SciencePrefabEditorPath);
        if (doctorPrefab == null || sciencePrefab == null || font == null)
        {
            Debug.LogError("Level1IntroDialogueRunner: 缺少对话预制体或字体资源。");
            FinishIntro();
            return;
        }

        GameObject doctorPanel = InstantiatePanel(doctorPrefab, canvas.transform, "conversation1");
        GameObject sciencePanel = InstantiatePanel(sciencePrefab, canvas.transform, "conversation3");
        if (sciencePanel.transform.childCount == 0)
        {
            Debug.LogError("Level1IntroDialogueRunner: conversation3 预制体缺少科普对话框，请检查 Assets/perfab/conversation3.prefab。");
            FinishIntro();
            return;
        }

        Text doctorText = ConversationDialogueHelper.EnsureDialogueText(
            doctorPanel.transform, font, dialogueFontSize, Color.white);
        Text scienceText = ConversationDialogueHelper.EnsureDialogueText(
            sciencePanel.transform, font, dialogueFontSize, Color.white);

        DialogueLine[] lines =
        {
            new DialogueLine
            {
                panel = doctorPanel,
                dialogueText = doctorText,
                content = DoctorLine
            },
            new DialogueLine
            {
                panel = sciencePanel,
                dialogueText = scienceText,
                content = ScienceLine
            }
        };

        StreamingDialogueController dialogueController = gameObject.AddComponent<StreamingDialogueController>();
        dialogueController.Configure(lines);
        dialogueController.OnDialogueComplete += HandleDialogueComplete;
        dialogueController.StartDialogue();
    }

    private void HandleDialogueComplete()
    {
        FinishIntro();
        Destroy(gameObject);
    }

    private void FinishIntro()
    {
        LevelGameFlow.SetIntroActive(false);
        Time.timeScale = 1f;
        _onComplete?.Invoke();
        _onComplete = null;
    }

    private static Canvas GetDialogueCanvas()
    {
        return CombatUiCanvas.GetOrCreate(200);
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
        GameObject editorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(editorPath);
        if (editorPrefab != null && !HasDialogueBox(editorPrefab))
            editorPrefab = null;

        if (editorPrefab != null && (prefab == null || !HasDialogueBox(prefab)))
            prefab = editorPrefab;
        else if (prefab == null)
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(editorPath);
#endif
        return prefab;
    }

    private static bool HasDialogueBox(GameObject prefab)
    {
        return prefab != null
            && ConversationDialogueHelper.FindDialogueBoxTransform(prefab.transform) != prefab.transform;
    }
}
