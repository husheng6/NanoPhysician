using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 开始界面：点击开始/设置/科普按钮进行跳转或打开页面。
/// 挂载到 Canvas 或主菜单根物体上。
/// </summary>
public class StartMenuController : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button scienceButton;

    [Header("场景跳转")]
    [SerializeField] private string levelSelectionSceneName = "levelselectionScene";

    [Header("设置页面")]
    [SerializeField] private SettingsPanelController settingsPanel;

    [Header("科普页面")]
    [SerializeField] private SciencePanelController sciencePanel;

    private void Awake()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (scienceButton != null)
            scienceButton.onClick.AddListener(OpenScience);
    }

    public void StartGame()
    {
        SceneLoader.Load(levelSelectionSceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("StartMenuController: 未绑定 SettingsPanelController。");
            return;
        }

        settingsPanel.OpenPanel();
    }

    public void OpenScience()
    {
        if (sciencePanel == null)
        {
            Debug.LogWarning("StartMenuController: 未绑定 SciencePanelController。");
            return;
        }

        sciencePanel.OpenPanel();
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(StartGame);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenSettings);

        if (scienceButton != null)
            scienceButton.onClick.RemoveListener(OpenScience);
    }
}
