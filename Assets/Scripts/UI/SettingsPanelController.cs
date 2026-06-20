using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置页面：关闭页面返回主菜单、退出游戏、调节音量与音效。
/// 挂载到 SettingsPanel 根物体上，默认隐藏。
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    private const float RowMatchThreshold = 40f;

    [Header("按钮引用")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button quitGameButton;

    [Header("音量调节（可留空，按「音效」「音量」标签自动绑定）")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);

        ResolveVolumeSliders();
        BindVolumeSliders();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        SyncSliders();
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

    private void ResolveVolumeSliders()
    {
        if (TryResolveSlidersByLabel(out Slider sfx, out Slider volume))
        {
            sfxVolumeSlider = sfx;
            volumeSlider = volume;
            return;
        }

        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in sliders)
        {
            if (slider == null)
                continue;

            string name = slider.gameObject.name.ToLowerInvariant();
            if (IsSfxSliderName(name))
            {
                if (sfxVolumeSlider == null)
                    sfxVolumeSlider = slider;
                continue;
            }

            if (IsMasterVolumeSliderName(name) && volumeSlider == null)
                volumeSlider = slider;
        }

        if (volumeSlider == null)
        {
            foreach (Slider slider in sliders)
            {
                if (slider != null && slider != sfxVolumeSlider)
                {
                    volumeSlider = slider;
                    break;
                }
            }
        }
    }

    private bool TryResolveSlidersByLabel(out Slider sfx, out Slider volume)
    {
        sfx = null;
        volume = null;

        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        if (sliders.Length == 0)
            return false;

        float topSfxY = float.NegativeInfinity;

        foreach (TMP_Text label in GetComponentsInChildren<TMP_Text>(true))
            TryMatchLabel(label.text, label.rectTransform, sliders, ref sfx, ref volume, ref topSfxY);

        foreach (Text label in GetComponentsInChildren<Text>(true))
            TryMatchLabel(label.text, label.rectTransform, sliders, ref sfx, ref volume, ref topSfxY);

        return sfx != null && volume != null;
    }

    private static void TryMatchLabel(
        string rawText,
        RectTransform labelRect,
        Slider[] sliders,
        ref Slider sfx,
        ref Slider volume,
        ref float topSfxY)
    {
        if (labelRect == null || string.IsNullOrWhiteSpace(rawText))
            return;

        string text = rawText.Trim();
        float labelY = labelRect.anchoredPosition.y;

        if (text == "音效")
        {
            if (labelY <= topSfxY)
                return;

            Slider matched = FindSliderOnSameRow(sliders, labelY);
            if (matched == null)
                return;

            topSfxY = labelY;
            sfx = matched;
            return;
        }

        if (text == "音量" && volume == null)
            volume = FindSliderOnSameRow(sliders, labelY);
    }

    private static Slider FindSliderOnSameRow(Slider[] sliders, float labelY)
    {
        Slider best = null;
        float bestDistance = float.MaxValue;

        foreach (Slider slider in sliders)
        {
            if (slider == null)
                continue;

            RectTransform rect = slider.transform as RectTransform;
            if (rect == null)
                continue;

            float distance = Mathf.Abs(rect.anchoredPosition.y - labelY);
            if (distance > RowMatchThreshold || distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = slider;
        }

        return best;
    }

    private void BindVolumeSliders()
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(AudioSettings.Volume);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        else
        {
            Debug.LogWarning("SettingsPanelController: 未找到「音量」滑条。");
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(AudioSettings.SfxVolume);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
        else
        {
            Debug.LogWarning("SettingsPanelController: 未找到「音效」滑条。");
        }
    }

    private void SyncSliders()
    {
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(AudioSettings.Volume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(AudioSettings.SfxVolume);
    }

    private static void OnVolumeChanged(float value)
    {
        AudioSettings.SetVolume(value);
    }

    private static void OnSfxVolumeChanged(float value)
    {
        AudioSettings.SetSfxVolume(value);
    }

    private static bool IsMasterVolumeSliderName(string name)
    {
        return name.Contains("volume") || name.Contains("音量");
    }

    private static bool IsSfxSliderName(string name)
    {
        return name.Contains("sfx") || name.Contains("音效");
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);

        if (quitGameButton != null)
            quitGameButton.onClick.RemoveListener(QuitGame);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }
}
