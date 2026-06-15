using UnityEngine;

/// <summary>
/// 玩家远程射击：鼠标右键点击，向点击方向发射子弹。
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private int aimMouseButton = 1;

    private PlayerController playerController;
    private PlayerStats playerStats;
    private Camera mainCamera;
    private float lastFireTime;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (LevelGameFlow.IsLevelEnded || LevelGameFlow.IsIntroActive)
            return;

        if (!Input.GetMouseButtonDown(aimMouseButton))
            return;

        if (Time.time - lastFireTime < playerStats.FireCooldown)
            return;

        if (!TryGetAimDirection(out Vector2 direction))
            return;

        if (!playerStats.TryConsumeMana(playerStats.ManaCostPerShot))
            return;

        Fire(direction);
        lastFireTime = Time.time;
    }

    private bool TryGetAimDirection(out Vector2 direction)
    {
        direction = Vector2.right;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                return false;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 toMouse = (Vector2)mouseWorld - (Vector2)transform.position;
        if (toMouse.sqrMagnitude <= 0.0001f)
        {
            direction = playerController.FacingDirection;
            return direction.sqrMagnitude > 0.0001f;
        }

        direction = toMouse.normalized;
        return true;
    }

    private void Fire(Vector2 direction)
    {
        playerController.SetFacingFromDirection(direction);
        SfxManager.PlayRanged();

        Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffset);

        GameObject projectileObject = new GameObject("PlayerProjectile");
        projectileObject.transform.position = spawnPosition;

        ParticleProjectile projectile = projectileObject.AddComponent<ParticleProjectile>();
        projectile.Launch(
            direction,
            playerStats.AttackDamage,
            playerStats.ProjectileSpeed,
            playerStats.ProjectileRange);
    }
}
