using UnityEngine;

/// <summary>
/// 全局音量与音效音量设置，使用 PlayerPrefs 持久化。
/// </summary>
public static class AudioSettings
{
    private const string VolumePrefKey = "MasterVolume";
    private const string SfxVolumePrefKey = "SfxVolume";
    private const float DefaultVolume = 0.8f;
    private const float DefaultSfxVolume = 1f;

    public static float Volume { get; private set; } = DefaultVolume;
    public static float SfxVolume { get; private set; } = DefaultSfxVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSavedSettings()
    {
        Volume = PlayerPrefs.GetFloat(VolumePrefKey, DefaultVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumePrefKey, DefaultSfxVolume);
        ApplyVolume();
    }

    public static void SetVolume(float volume)
    {
        Volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumePrefKey, Volume);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public static void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, SfxVolume);
        PlayerPrefs.Save();
    }

    private static void ApplyVolume()
    {
        AudioListener.volume = Volume;
    }
}
