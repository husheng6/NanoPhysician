using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 关卡一剧情：对话结束后显示加载画面并进入第一关。
/// </summary>
public class Level1PlotController : MonoBehaviour
{
    [Header("对话")]
    [SerializeField] private StreamingDialogueController dialogueController;

    [Header("加载")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Text loadingText;
    [SerializeField] private string loadingMessage = "正在加载中...";
    [SerializeField] private string nextSceneName = "level1Scence";
    [SerializeField] private float minimumLoadingSeconds = 1.5f;

    private bool _isLoading;

    public void Configure(
        StreamingDialogueController dialogue,
        GameObject loadingRoot,
        Text loadingLabel,
        string targetSceneName = "level1Scence")
    {
        dialogueController = dialogue;
        loadingPanel = loadingRoot;
        loadingText = loadingLabel;
        nextSceneName = targetSceneName;
    }

    private void Awake()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void Start()
    {
        if (dialogueController == null)
        {
            Debug.LogError("Level1PlotController: 未绑定 StreamingDialogueController。");
            return;
        }

        dialogueController.OnDialogueComplete += HandleDialogueComplete;
        dialogueController.StartDialogue();
    }

    private void OnDestroy()
    {
        if (dialogueController != null)
            dialogueController.OnDialogueComplete -= HandleDialogueComplete;
    }

    private void HandleDialogueComplete()
    {
        if (_isLoading)
            return;

        StartCoroutine(ShowLoadingAndLoadScene());
    }

    private IEnumerator ShowLoadingAndLoadScene()
    {
        _isLoading = true;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingText != null)
            loadingText.text = loadingMessage;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        if (loadOperation == null)
        {
            Debug.LogError($"Level1PlotController: 无法加载场景 {nextSceneName}，请检查 Build Settings。");
            yield break;
        }

        loadOperation.allowSceneActivation = false;

        float elapsed = 0f;
        while (elapsed < minimumLoadingSeconds || loadOperation.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        loadOperation.allowSceneActivation = true;
    }
}
