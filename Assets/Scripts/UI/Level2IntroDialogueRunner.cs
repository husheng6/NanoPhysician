using System;
using UnityEngine;

/// <summary>
/// 第二关开场剧情：第一张地图（中性粒细胞）对话。
/// </summary>
public class Level2IntroDialogueRunner : MonoBehaviour
{
    private Action onComplete;

    public static void Play(Action onComplete)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level2IntroDialogue");
        Level2IntroDialogueRunner runner = host.AddComponent<Level2IntroDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Level2CombatDialogueRunner.Play(new[]
        {
            Level2CombatDialogueRunner.ScriptLine.Doctor("林墨博士：怎么回事？我明明已经给你做了伪装，为什么还是被免疫系统排斥"),
            Level2CombatDialogueRunner.ScriptLine.Doctor("林墨博士：那些白色的细胞是中性粒细胞，它们是人体免疫系统的第一道防线。它们把你当成了细菌或者病毒。"),
            Level2CombatDialogueRunner.ScriptLine.Science("科普提示：中性粒细胞是白细胞的一种，占白细胞总数的 50%-70%。它们可以吞噬和消灭入侵的病原体，是人体最勇敢的 \"士兵\"。")
        }, FinishIntro);
    }

    private void FinishIntro()
    {
        onComplete?.Invoke();
        onComplete = null;
        Destroy(gameObject);
    }
}
