using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 跨场景背景音乐管理：主页/选关播放背景 BGM，战斗关卡播放战斗 BGM。
/// </summary>
public static class BgmManager
{
    private const string BackgroundResourcesPath = "Music/背景bgm";
    private const string BattleResourcesPath = "Music/战斗bgm";
    private const string BackgroundEditorPath = "Assets/music/背景bgm.mp3";
    private const string BattleEditorPath = "Assets/music/战斗bgm.mp3";

    private const string StartSceneName = "startScene";
    private const string LevelSelectionSceneName = "levelselectionScene";
    private const string Level1PlotSceneName = "leve1polt";
    private const string Level1SceneName = "level1Scence";
    private const string Level2SceneName = "level2Scence";
    private const string Level3SceneName = "level3Scence";

    private static BgmAudioHost host;
    private static AudioClip backgroundClip;
    private static AudioClip battleClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case StartSceneName:
            case LevelSelectionSceneName:
            case Level1PlotSceneName:
                PlayBackground();
                break;
            case Level2SceneName:
            case Level3SceneName:
                PlayBattle();
                break;
            case Level1SceneName:
                break;
        }
    }

    public static void PlayBackground()
    {
        PlayClip(GetBackgroundClip());
    }

    public static void PlayBattle()
    {
        PlayClip(GetBattleClip());
    }

    private static void PlayClip(AudioClip clip)
    {
        if (clip == null)
            return;

        BgmAudioHost audioHost = EnsureHost();
        if (audioHost.Source.clip == clip && audioHost.Source.isPlaying)
            return;

        audioHost.Source.clip = clip;
        audioHost.Source.Play();
    }

    private static BgmAudioHost EnsureHost()
    {
        if (host != null)
            return host;

        GameObject go = new GameObject("BgmManager");
        Object.DontDestroyOnLoad(go);

        AudioSource source = go.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        host = go.AddComponent<BgmAudioHost>();
        host.Source = source;
        return host;
    }

    private static AudioClip GetBackgroundClip()
    {
        if (backgroundClip == null)
            backgroundClip = LoadClip(BackgroundResourcesPath, BackgroundEditorPath);
        return backgroundClip;
    }

    private static AudioClip GetBattleClip()
    {
        if (battleClip == null)
            battleClip = LoadClip(BattleResourcesPath, BattleEditorPath);
        return battleClip;
    }

    private static AudioClip LoadClip(string resourcesPath, string editorPath)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcesPath);
#if UNITY_EDITOR
        if (clip == null)
            clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(editorPath);
#endif
        if (clip == null)
            Debug.LogWarning($"BgmManager: 找不到音频资源 {resourcesPath}。");
        return clip;
    }

    private sealed class BgmAudioHost : MonoBehaviour
    {
        public AudioSource Source;
    }
}
