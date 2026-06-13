using UnityEngine;

/// <summary>
/// 摄像机跟随目标，移动范围限制在地图内；Boss 战时与玩家同步锁定决斗区。
/// 挂载到 Main Camera 上。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform mapRoot;
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Camera cam;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    private bool usingArenaBounds;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (mapRoot == null)
        {
            GameObject backgroundGroup = GameObject.Find("BackgroundGroup");
            if (backgroundGroup != null)
                mapRoot = backgroundGroup.transform;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        usingArenaBounds = PlayerMovementBounds.IsArenaLocked;
        CacheCameraBounds();

        if (target != null)
            transform.position = GetDesiredPosition();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        bool arenaLocked = PlayerMovementBounds.IsArenaLocked;
        if (arenaLocked != usingArenaBounds)
        {
            usingArenaBounds = arenaLocked;
            CacheCameraBounds();
        }

        Vector3 desired = GetDesiredPosition();
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }

    private void CacheCameraBounds()
    {
        if (cam == null)
            return;

        if (!PlayerMovementBounds.TryGetMovementBounds(mapRoot, out Bounds mapBounds)
            && !MapBoundsUtility.TryGetBounds(mapRoot, out mapBounds))
            return;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        minX = mapBounds.min.x + halfWidth;
        maxX = mapBounds.max.x - halfWidth;
        minY = mapBounds.min.y + halfHeight;
        maxY = mapBounds.max.y - halfHeight;

        if (minX > maxX)
        {
            float centerX = mapBounds.center.x;
            minX = maxX = centerX;
        }

        if (minY > maxY)
        {
            float centerY = mapBounds.center.y;
            minY = maxY = centerY;
        }
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 desired = target.position + offset;

        if (mapRoot != null && cam != null)
        {
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        desired.z = offset.z;
        return desired;
    }
}
