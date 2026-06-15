using UnityEngine;

/// <summary>
/// 地图内自由移动，无重力，移动范围限制在背景地图内。
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Transform mapRoot;
    [SerializeField] private float boundsPadding = 0.1f;

    private SpriteRenderer spriteRenderer;
    private PlayerAnimator playerAnimator;
    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.right;

    /// <summary>当前面朝方向（基于最近一次移动输入，默认向右）。</summary>
    public Vector2 FacingDirection => facingDirection;

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.1f, speed);
    }

    public void SetFacingFromDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        facingDirection = direction.normalized;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<PlayerAnimator>();

        // 左右行走使用独立动画，保持默认朝向，避免 flipX 把向右动画镜像成错误姿态。
        if (spriteRenderer != null)
            spriteRenderer.flipX = false;

        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }
    }

    private void Update()
    {
        if (LevelGameFlow.IsLevelEnded || LevelGameFlow.IsIntroActive)
            return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (moveInput.sqrMagnitude > 0.0001f)
            facingDirection = moveInput.normalized;

        Vector3 nextPosition = transform.position + (Vector3)(moveInput * moveSpeed * Time.deltaTime);
        transform.position = ClampToMap(nextPosition);

        if (playerAnimator == null)
            playerAnimator = GetComponent<PlayerAnimator>();

        if (playerAnimator != null)
            playerAnimator.SetMovement(moveInput);
    }

    private Vector3 ClampToMap(Vector3 position)
    {
        if (!PlayerMovementBounds.TryGetMovementBounds(mapRoot, out Bounds bounds)
            && !MapBoundsUtility.TryGetBounds(mapRoot, out bounds))
            return position;

        Vector2 halfSize = GetHalfSize();

        position.x = Mathf.Clamp(position.x,
            bounds.min.x + halfSize.x + boundsPadding,
            bounds.max.x - halfSize.x - boundsPadding);

        position.y = Mathf.Clamp(position.y,
            bounds.min.y + halfSize.y + boundsPadding,
            bounds.max.y - halfSize.y - boundsPadding);

        return position;
    }

    private Vector2 GetHalfSize()
    {
        if (spriteRenderer != null)
            return spriteRenderer.bounds.extents;

        return Vector2.one * 0.25f;
    }
}
