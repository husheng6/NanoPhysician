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

    [Header("音量调节")]
    [SerializeField] private Slider volumeSlider;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);

        ResolveVolumeSlider();
        BindVolumeSlider();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(AudioSettings.Volume);
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

    private void ResolveVolumeSlider()
    {
        if (volumeSlider != null)
            return;

        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in sliders)
        {
            string name = slider.gameObject.name.ToLowerInvariant();
            if (name.Contains("volume") || name.Contains("音量"))
            {
                volumeSlider = slider;
                return;
            }
        }

        if (sliders.Length > 0)
            volumeSlider = sliders[0];
    }

    private void BindVolumeSlider()
    {
        if (volumeSlider == null)
        {
            Debug.LogWarning("SettingsPanelController: 未找到音量滑条。");
            return;
        }

        volumeSlider.SetValueWithoutNotify(AudioSettings.Volume);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private static void OnVolumeChanged(float value)
    {
        AudioSettings.SetVolume(value);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);

        if (quitGameButton != null)
            quitGameButton.onClick.RemoveListener(QuitGame);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}
