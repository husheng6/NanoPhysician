using System;
using UnityEngine;

/// <summary>
/// 第三关进入第二张地图时的剧情（与免疫系统合作）。
/// </summary>
public class Level3CooperationDialogueRunner : MonoBehaviour
{
    private Action onComplete;

    public static void Play(Action onComplete = null)
    {
        if (LevelGameFlow.IsIntroActive)
            return;

        GameObject host = new GameObject("Level3CooperationDialogue");
        Level3CooperationDialogueRunner runner = host.AddComponent<Level3CooperationDialogueRunner>();
        runner.Begin(onComplete);
    }

    private void Begin(Action completeCallback)
    {
        onComplete = completeCallback;
        Level3CombatDialogueRunner.Play(new[]
        {
            Level3CombatDialogueRunner.ScriptLine.Doctor("林墨博士：看到了吗？当你不再攻击它们，它们也慢慢停止了攻击你。"),
            Level3CombatDialogueRunner.ScriptLine.Doctor("林墨博士：原来最好的方式不是代替，而是合作。"),
            Level3CombatDialogueRunner.ScriptLine.Girl("林小雨：我感觉好多了... 身体里不再打仗了。"),
            Level3CombatDialogueRunner.ScriptLine.Science("科普提示：目前糖尿病还无法根治，但可以通过合理的治疗和管理得到很好的控制。治疗糖尿病需要 \"五驾马车\"：饮食治疗、运动治疗、药物治疗、血糖监测和健康教育。")
        }, FinishDialogue);
    }

    private void FinishDialogue()
    {
        onComplete?.Invoke();
        onComplete = null;
        Destroy(gameObject);
    }
}
