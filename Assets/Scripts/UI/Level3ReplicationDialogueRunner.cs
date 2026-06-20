using System;
using UnityEngine;

/// <summary>
/// 第三关进入第三张地图时的剧情（失控复制体）。
/// </summary>
public class Level3ReplicationDialogueRunner : MonoBehaviour
{
    private Action onComplete;

    public static void Play(Action onComplete = null)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level3ReplicationDialogue");
        Level3ReplicationDialogueRunner runner = host.AddComponent<Level3ReplicationDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Level3CombatDialogueRunner.Play(new[]
        {
            Level3CombatDialogueRunner.ScriptLine.Doctor("林墨博士：还有最后一个问题。我之前为了让你能长期工作，给你设计了自我复制功能。但现在看来，这是一个巨大的错误。"),
            Level3CombatDialogueRunner.ScriptLine.Doctor("林墨博士：那个失控的复制体还在疯狂地攻击一切。你必须阻止它们。")
        }, FinishDialogue);
    }

    private void FinishDialogue()
    {
        onComplete?.Invoke();
        onComplete = null;
        Destroy(gameObject);
    }
}
