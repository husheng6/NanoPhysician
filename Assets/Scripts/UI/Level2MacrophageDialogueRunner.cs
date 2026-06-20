using System;
using UnityEngine;

/// <summary>
/// 第二关进入第二张地图时的剧情（巨噬细胞）。
/// </summary>
public class Level2MacrophageDialogueRunner : MonoBehaviour
{
    private Action onComplete;

    public static void Play(Action onComplete = null)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level2MacrophageDialogue");
        Level2MacrophageDialogueRunner runner = host.AddComponent<Level2MacrophageDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Level2CombatDialogueRunner.Play(new[]
        {
            Level2CombatDialogueRunner.ScriptLine.Doctor("林墨博士：糟糕，巨噬细胞也来了！它们比中性粒细胞更强大，能吞噬更大的异物。"),
            Level2CombatDialogueRunner.ScriptLine.Doctor("林墨博士：我设计你的时候，以为只要能控制血糖就万事大吉了。我忽略了人体是一个复杂的整体..."),
            Level2CombatDialogueRunner.ScriptLine.Science("科普提示：巨噬细胞是人体的 \"清道夫\"，不仅能吞噬病原体，还能清除体内衰老、死亡的细胞和细胞碎片。它们还能激活其他免疫细胞，启动特异性免疫反应。")
        }, FinishDialogue);
    }

    private void FinishDialogue()
    {
        onComplete?.Invoke();
        onComplete = null;
        Destroy(gameObject);
    }
}
