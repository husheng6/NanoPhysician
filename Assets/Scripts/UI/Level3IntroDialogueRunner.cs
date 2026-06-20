using System;
using UnityEngine;

/// <summary>
/// 第三关开场剧情：第一张地图（停止与免疫系统战斗）对话。
/// </summary>
public class Level3IntroDialogueRunner : MonoBehaviour
{
    private Action onComplete;

    public static void Play(Action onComplete)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level3IntroDialogue");
        Level3IntroDialogueRunner runner = host.AddComponent<Level3IntroDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Level3CombatDialogueRunner.Play(new[]
        {
            Level3CombatDialogueRunner.ScriptLine.Doctor("林墨博士：Alpha-01，听我说。你不能再和免疫系统战斗了。这样下去，小雨会有生命危险。"),
            Level3CombatDialogueRunner.ScriptLine.Doctor("林墨博士：我设计了一个新的程序。你可以向免疫系统发送 \"友好\" 信号，告诉它们你不是敌人。"),
            Level3CombatDialogueRunner.ScriptLine.Doctor("林墨博士：但这意味着你不能再主动攻击血糖分子了。你只能在血糖过高的时候，温和地释放少量胰岛素。"),
            Level3CombatDialogueRunner.ScriptLine.Science("科普提示：自身免疫性疾病的本质是免疫系统 \"敌我不分\"，攻击自身组织。治疗这类疾病不能简单地抑制免疫系统，而是要调节免疫系统，让它重新学会区分 \"自己\" 和 \"异己\"。")
        }, FinishIntro);
    }

    private void FinishIntro()
    {
        onComplete?.Invoke();
        onComplete = null;
        Destroy(gameObject);
    }
}
