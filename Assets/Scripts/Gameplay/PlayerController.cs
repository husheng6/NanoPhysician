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
    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.right;
    private bool faceRight = true;

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

        if (spriteRenderer == null || Mathf.Approximately(facingDirection.x, 0f))
            return;

        if (facingDirection.x > 0f && !faceRight)
        {
            faceRight = true;
            spriteRenderer.flipX = true;
        }
        else if (facingDirection.x < 0f && faceRight)
        {
            faceRight = false;
            spriteRenderer.flipX = false;
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }
    }

    private void Update()
    {
        if (LevelGameFlow.IsLevelEnded)
            return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (moveInput.sqrMagnitude > 0.0001f)
            facingDirection = moveInput.normalized;

        Vector3 nextPosition = transform.position + (Vector3)(moveInput * moveSpeed * Time.deltaTime);
        transform.position = ClampToMap(nextPosition);
        UpdateFacing();
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

    private void UpdateFacing()
    {
        if (spriteRenderer == null || Mathf.Approximately(moveInput.x, 0f))
            return;

        if (moveInput.x > 0f && !faceRight)
        {
            faceRight = true;
            spriteRenderer.flipX = true;
        }
        else if (moveInput.x < 0f && faceRight)
        {
            faceRight = false;
            spriteRenderer.flipX = false;
        }
    }
}
