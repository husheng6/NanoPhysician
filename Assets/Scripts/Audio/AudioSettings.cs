using UnityEngine;

/// <summary>
/// 全局音量设置，使用 PlayerPrefs 持久化。
/// </summary>
public static class AudioSettings
{
    private const string VolumePrefKey = "MasterVolume";
    private const float DefaultVolume = 0.8f;

    public static float Volume { get; private set; } = DefaultVolume;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSavedVolume()
    {
        Volume = PlayerPrefs.GetFloat(VolumePrefKey, DefaultVolume);
        ApplyVolume();
    }

    public static void SetVolume(float volume)
    {
        Volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumePrefKey, Volume);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    private static void ApplyVolume()
    {
        AudioListener.volume = Volume;
    }
}
