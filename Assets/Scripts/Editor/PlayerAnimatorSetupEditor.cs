using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// 编辑器工具：一键生成玩家统一动画控制器。
/// 将 idle、move（上下左右）、attack 动画整合到一个 AnimatorController 中。
/// </summary>
public static class PlayerAnimatorSetupEditor
{
    private const string OutputPath = "Assets/art assets/PlayerAnimator.controller";

    [MenuItem("NanoPhysician/Create Player Animator Controller")]
    public static void CreatePlayerAnimatorController()
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(OutputPath) != null)
            AssetDatabase.DeleteAsset(OutputPath);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(OutputPath);

        // 添加参数
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("AttackType", AnimatorControllerParameterType.Int);

        // 获取默认 Layer 的状态机
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // 加载动画剪辑
        AnimationClip idleClip = LoadClip("Assets/art assets/idle/idle.anim");
        AnimationClip moveRightClip = LoadClip("Assets/art assets/MoveRight/right.anim");
        AnimationClip moveLeftClip = LoadClip("Assets/art assets/MoveLeft/left.anim");
        AnimationClip moveUpClip = LoadClip("Assets/art assets/MoveUp/up.anim");
        AnimationClip moveDownClip = LoadClip("Assets/art assets/MoveDown/down.anim");
        AnimationClip attackClip = LoadClip("Assets/art assets/Attack/attack.anim");

        // 创建状态
        AnimatorState idleState = rootStateMachine.AddState("Idle", new Vector3(0, 0, 0));
        idleState.motion = idleClip;
        rootStateMachine.defaultState = idleState;

        // 创建 Move BlendTree
        AnimatorState moveState = rootStateMachine.AddState("Move", new Vector3(300, 0, 0));
        BlendTree moveBlendTree = new BlendTree();
        moveBlendTree.name = "MoveBlendTree";
        moveBlendTree.blendType = BlendTreeType.SimpleDirectional2D;
        moveBlendTree.blendParameter = "MoveX";
        moveBlendTree.blendParameterY = "MoveY";
        moveBlendTree.useAutomaticThresholds = false;

        if (moveRightClip != null) moveBlendTree.AddChild(moveRightClip, new Vector2(1, 0));
        if (moveLeftClip != null) moveBlendTree.AddChild(moveLeftClip, new Vector2(-1, 0));
        if (moveUpClip != null) moveBlendTree.AddChild(moveUpClip, new Vector2(0, 1));
        if (moveDownClip != null) moveBlendTree.AddChild(moveDownClip, new Vector2(0, -1));

        moveState.motion = moveBlendTree;

        // 创建攻击状态
        AnimatorState attackState = rootStateMachine.AddState("Attack", new Vector3(150, -200, 0));
        attackState.motion = attackClip;

        // Idle -> Move (IsMoving = true)
        AnimatorStateTransition idleToMove = idleState.AddTransition(moveState);
        idleToMove.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        idleToMove.duration = 0.05f;
        idleToMove.hasExitTime = false;

        // Move -> Idle (IsMoving = false)
        AnimatorStateTransition moveToIdle = moveState.AddTransition(idleState);
        moveToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        moveToIdle.duration = 0.05f;
        moveToIdle.hasExitTime = false;

        // Any State -> Attack (Attack trigger)
        AnimatorStateTransition anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.duration = 0.02f;
        anyToAttack.hasExitTime = false;

        // Attack -> Idle (动画播完后回到 Idle)
        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 1f;
        attackToIdle.duration = 0.05f;

        // 保存资源
        AssetDatabase.AddObjectToAsset(moveBlendTree, controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[NanoPhysician] 玩家动画控制器已生成: {OutputPath}");
        Selection.activeObject = controller;
    }

    private static AnimationClip LoadClip(string path)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
            Debug.LogWarning($"[NanoPhysician] 找不到动画剪辑: {path}");
        return clip;
    }
}
