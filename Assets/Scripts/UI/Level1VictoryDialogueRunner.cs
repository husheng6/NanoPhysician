using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 第一关通关并完成商店后：展示「游戏背景三」并播放林墨博士庆祝剧情。
/// </summary>
public class Level1VictoryDialogueRunner : MonoBehaviour
{
    private const string Level1SceneName = "level1Scence";
    private const string BackgroundResourcesPath = "Dialogue/游戏背景三";
    private const string BackgroundEditorPath = "Assets/art assets/游戏背景三.png";
    private const string DoctorPrefabResourcesPath = "Dialogue/conversation1";
    private const string DoctorPrefabEditorPath = "Assets/perfab/conversation1.prefab";

    private const string DoctorLine1 =
        "林墨博士：血糖浓度已恢复正常：5.6mmol/L。胰岛功能替代率：100%。";

    private const string DoctorLine2 =
        "林墨博士：成功了！Alpha-01，你做得很好！从现在开始，小雨再也不用打针了！";

    [SerializeField] private int dialogueFontSize = 18;

    private Action onComplete;
    private GameObject backgroundObject;
    private GameObject doctorPanel;

    public static bool ShouldPlayForCurrentScene()
    {
        return SceneManager.GetActiveScene().name == Level1SceneName;
    }

    public static void Play(Action onComplete)
    {
        GameObject host = new GameObject("Level1VictoryDialogue");
        Level1VictoryDialogueRunner runner = host.AddComponent<Level1VictoryDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Time.timeScale = 0f;

        Canvas canvas = CombatUiCanvas.GetOrCreate(280);
        Font font = DialogueFont.Get();
        GameObject doctorPrefab = LoadPrefab(DoctorPrefabResourcesPath, DoctorPrefabEditorPath);
        if (doctorPrefab == null || font == null)
        {
            Debug.LogError("Level1VictoryDialogueRunner: 缺少对话预制体或字体资源。");
            FinishDialogue();
            return;
        }

        backgroundObject = CreateBackgroundImage(canvas.transform);
        doctorPanel = InstantiatePanel(doctorPrefab, canvas.transform, "conversation1_victory");
        Text doctorText = ConversationDialogueHelper.EnsureDialogueText(
            doctorPanel.transform, font, dialogueFontSize, Color.white);

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
        if (backgroundObject != null)
            Destroy(backgroundObject);

        if (doctorPanel != null)
            Destroy(doctorPanel);

        onComplete?.Invoke();
        onComplete = null;
    }

    private static GameObject CreateBackgroundImage(Transform parent)
    {
        GameObject background = new GameObject("VictoryBackground");
        background.transform.SetParent(parent, false);
        background.transform.SetAsFirstSibling();

        RectTransform rect = background.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image image = background.AddComponent<Image>();
        Sprite sprite = LoadBackgroundSprite();
        image.sprite = sprite;
        image.preserveAspect = false;
        image.color = sprite != null ? Color.white : Color.black;
        image.raycastTarget = false;

        return background;
    }

    private static Sprite LoadBackgroundSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(BackgroundResourcesPath);
#if UNITY_EDITOR
        if (sprite == null)
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundEditorPath);
#endif
        if (sprite == null)
            Debug.LogWarning("Level1VictoryDialogueRunner: 找不到游戏背景三资源。");
        return sprite;
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
