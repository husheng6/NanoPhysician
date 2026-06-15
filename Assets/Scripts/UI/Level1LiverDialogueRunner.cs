using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 第一关进入肝脏区域（第二张地图）时的剧情对话。
/// </summary>
public class Level1LiverDialogueRunner : MonoBehaviour
{
    private const string DoctorPrefabResourcesPath = "Dialogue/conversation1";
    private const string DoctorPrefabEditorPath = "Assets/perfab/conversation1.prefab";

    private const string LiverLine =
        "我们现在在肝脏。肝脏是人体的 \"糖库\"，当血糖过高时，肝脏会把多余的糖变成糖原储存起来；当血糖过低时，又会把糖原分解成葡萄糖释放到血液中。";

    private const string DiabetesLine =
        "但是糖尿病患者的这个调节系统出了问题。1 型糖尿病患者的胰岛不能分泌胰岛素，2 型糖尿病患者的身体对胰岛素不敏感。";

    [SerializeField] private int dialogueFontSize = 18;

    private Action onComplete;

    public static void Play(Action onComplete = null)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level1LiverDialogue");
        Level1LiverDialogueRunner runner = host.AddComponent<Level1LiverDialogueRunner>();
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
        if (doctorPrefab == null || font == null)
        {
            Debug.LogError("Level1LiverDialogueRunner: 缺少对话预制体或字体资源。");
            FinishDialogue();
            return;
        }

        GameObject doctorPanel = InstantiatePanel(doctorPrefab, canvas.transform, "conversation1_liver");
        Text doctorText = ConversationDialogueHelper.EnsureDialogueText(
            doctorPanel.transform, font, dialogueFontSize, Color.white);

        DialogueLine[] lines =
        {
            new DialogueLine
            {
                panel = doctorPanel,
                dialogueText = doctorText,
                content = LiverLine
            },
            new DialogueLine
            {
                panel = doctorPanel,
                dialogueText = doctorText,
                content = DiabetesLine
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
