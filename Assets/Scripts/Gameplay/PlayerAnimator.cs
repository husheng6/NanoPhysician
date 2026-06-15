using UnityEngine;

/// <summary>
/// 驱动玩家动画状态机：待机、移动、攻击。
/// 需要 Animator 组件，以及一个包含以下参数的 AnimatorController：
/// - IsMoving (bool)
/// - MoveX (float)
/// - MoveY (float)
/// - Attack (trigger)
/// - AttackType (int): 0=近战, 1=远程
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private bool isAttacking;
    private bool isConfigured;
    private bool warnedMissingController;

    private static readonly int ParamIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int ParamMoveX = Animator.StringToHash("MoveX");
    private static readonly int ParamMoveY = Animator.StringToHash("MoveY");
    private static readonly int ParamAttack = Animator.StringToHash("Attack");
    private static readonly int ParamAttackType = Animator.StringToHash("AttackType");

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        isConfigured = HasRequiredParameters();
    }

    /// <summary>
    /// 由 PlayerController 每帧调用，更新移动动画。
    /// </summary>
    public void SetMovement(Vector2 moveInput)
    {
        if (!EnsureConfigured())
            return;

        bool moving = moveInput.sqrMagnitude > 0.0001f;
        animator.SetBool(ParamIsMoving, moving);

        if (moving)
        {
            Vector2 blendDirection = GetBlendDirection(moveInput);
            animator.SetFloat(ParamMoveX, blendDirection.x);
            animator.SetFloat(ParamMoveY, blendDirection.y);
        }
    }

    private static Vector2 GetBlendDirection(Vector2 moveInput)
    {
        if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
            return new Vector2(Mathf.Sign(moveInput.x), 0f);

        return new Vector2(0f, Mathf.Sign(moveInput.y));
    }

    /// <summary>
    /// 触发攻击动画。attackType: 0=近战, 1=远程。
    /// </summary>
    public void PlayAttack(int attackType)
    {
        if (!EnsureConfigured())
            return;

        animator.SetInteger(ParamAttackType, attackType);
        animator.SetTrigger(ParamAttack);
        isAttacking = true;
    }

    /// <summary>
    /// 由攻击动画结束事件调用，或由代码在攻击完成后调用。
    /// </summary>
    public void OnAttackFinished()
    {
        isAttacking = false;
    }

    private bool EnsureConfigured()
    {
        if (isConfigured)
            return true;

        isConfigured = HasRequiredParameters();
        if (isConfigured || warnedMissingController)
            return isConfigured;

        warnedMissingController = true;
        Debug.LogWarning(
            "PlayerAnimator: Animator Controller 缺少 IsMoving 等参数。请在 Unity 菜单运行 " +
            "NanoPhysician → Create Player Animator Controller，并确认 Player 的 Animator 已绑定 PlayerAnimator.controller。",
            this);
        return false;
    }

    private bool HasRequiredParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;

        return HasParameter(ParamIsMoving)
            && HasParameter(ParamMoveX)
            && HasParameter(ParamMoveY)
            && HasParameter(ParamAttack)
            && HasParameter(ParamAttackType);
    }

    private bool HasParameter(int nameHash)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == nameHash)
                return true;
        }

        return false;
    }
}
