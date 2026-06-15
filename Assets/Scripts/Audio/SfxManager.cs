using UnityEngine;

/// <summary>
/// 战斗音效：近战、远程攻击等一次性音效。
/// </summary>
public static class SfxManager
{
    private const string MeleeResourcesPath = "Sfx/近战音效";
    private const string RangedResourcesPath = "Sfx/远程音效";
    private const string MeleeEditorPath = "Assets/music/近战音效.mp3";
    private const string RangedEditorPath = "Assets/music/远程音效.mp3";

    private static SfxAudioHost host;
    private static AudioClip meleeClip;
    private static AudioClip rangedClip;

    public static void PlayMelee()
    {
        PlayOneShot(GetMeleeClip());
    }

    public static void PlayRanged()
    {
        PlayOneShot(GetRangedClip());
    }

    private static void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        EnsureHost().Source.PlayOneShot(clip);
    }

    private static SfxAudioHost EnsureHost()
    {
        if (host != null)
            return host;

        GameObject go = new GameObject("SfxManager");
        Object.DontDestroyOnLoad(go);

        AudioSource source = go.AddComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        host = go.AddComponent<SfxAudioHost>();
        host.Source = source;
        return host;
    }

    private static AudioClip GetMeleeClip()
    {
        if (meleeClip == null)
            meleeClip = LoadClip(MeleeResourcesPath, MeleeEditorPath);
        return meleeClip;
    }

    private static AudioClip GetRangedClip()
    {
        if (rangedClip == null)
            rangedClip = LoadClip(RangedResourcesPath, RangedEditorPath);
        return rangedClip;
    }

    private static AudioClip LoadClip(string resourcesPath, string editorPath)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcesPath);
#if UNITY_EDITOR
        if (clip == null)
            clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(editorPath);
#endif
        if (clip == null)
            Debug.LogWarning($"SfxManager: 找不到音频资源 {resourcesPath}。");
        return clip;
    }

    private sealed class SfxAudioHost : MonoBehaviour
    {
        public AudioSource Source;
    }
}
