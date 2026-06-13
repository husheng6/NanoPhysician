using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置页面：关闭页面返回主菜单、退出游戏。
/// 挂载到 SettingsPanel 根物体上，默认隐藏。
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button quitGameButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);

        if (quitGameButton != null)
            quitGameButton.onClick.RemoveListener(QuitGame);
    }
}
