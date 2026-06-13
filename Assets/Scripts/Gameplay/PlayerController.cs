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
    private bool faceRight = true;

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
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        Vector3 nextPosition = transform.position + (Vector3)(moveInput * moveSpeed * Time.deltaTime);
        transform.position = ClampToMap(nextPosition);
        UpdateFacing();
    }

    private Vector3 ClampToMap(Vector3 position)
    {
        if (!MapBoundsUtility.TryGetBounds(mapRoot, out Bounds mapBounds))
            return position;
        Vector2 halfSize = GetHalfSize();

        position.x = Mathf.Clamp(position.x,
            mapBounds.min.x + halfSize.x + boundsPadding,
            mapBounds.max.x - halfSize.x - boundsPadding);

        position.y = Mathf.Clamp(position.y,
            mapBounds.min.y + halfSize.y + boundsPadding,
            mapBounds.max.y - halfSize.y - boundsPadding);

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
