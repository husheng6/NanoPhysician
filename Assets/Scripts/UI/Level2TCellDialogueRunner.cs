using System;
using UnityEngine;

/// <summary>
/// 第二关进入第三张地图时的剧情（T 细胞）。
/// </summary>
public class Level2TCellDialogueRunner : MonoBehaviour
{
    private Action onComplete;

    public static void Play(Action onComplete = null)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level2TCellDialogue");
        Level2TCellDialogueRunner runner = host.AddComponent<Level2TCellDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Level2CombatDialogueRunner.Play(new[]
        {
            Level2CombatDialogueRunner.ScriptLine.Doctor("林墨博士：这是 T 细胞，免疫系统的 \"特种部队\"。它们能特异性识别并杀死被病毒感染的细胞和癌细胞。"),
            Level2CombatDialogueRunner.ScriptLine.Doctor("林墨博士：现在，它们把你标记为了需要清除的目标。这是我的错... 我太急于求成了。"),
            Level2CombatDialogueRunner.ScriptLine.Science("科普提示：T 细胞在胸腺中发育成熟，根据功能不同可分为辅助性 T 细胞、细胞毒性 T 细胞和调节性 T 细胞。1 型糖尿病就是由于细胞毒性 T 细胞错误地攻击并破坏了自身的胰岛 β 细胞导致的。")
        }, FinishDialogue);
    }

    private void FinishDialogue()
    {
        onComplete?.Invoke();
        onComplete = null;
        Destroy(gameObject);
    }
}
