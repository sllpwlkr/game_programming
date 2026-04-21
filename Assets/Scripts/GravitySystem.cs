using UnityEngine;

public enum SurfaceMode
{
    Floor,
    LeftWall,
    RightWall
}

public class GravitySystem : MonoBehaviour
{
    public SurfaceMode currentMode = SurfaceMode.Floor;

    public FloorBounds floorBounds;
    public WallBounds leftWallBounds;
    public WallBounds rightWallBounds;

    [Header("Switch Cooldown")]
    public float switchCooldown = 0.2f;

    private float switchCooldownTimer = 0f;

    public SurfaceMode CurrentMode => currentMode;
    public bool IsSwitchLocked => switchCooldownTimer > 0f;

    private void Awake()
    {
        AutoFindBounds();
    }

    private void OnValidate()
    {
        AutoFindBounds();
    }

    private void Update()
    {
        if (switchCooldownTimer > 0f)
            switchCooldownTimer -= Time.deltaTime;
    }

    public void AutoFindBounds()
    {
        floorBounds = FindFirstObjectByType<FloorBounds>();

        WallBounds[] allBounds = FindObjectsByType<WallBounds>(FindObjectsSortMode.None);

        leftWallBounds = null;
        rightWallBounds = null;

        foreach (WallBounds bounds in allBounds)
        {
            if (bounds == null)
                continue;

            if (bounds.surfaceMode == SurfaceMode.LeftWall && leftWallBounds == null)
                leftWallBounds = bounds;

            if (bounds.surfaceMode == SurfaceMode.RightWall && rightWallBounds == null)
                rightWallBounds = bounds;
        }
    }

    public void ApplySwitch(CubeUnit cube, GravitySwitch gravitySwitch)
    {
        if (cube == null || gravitySwitch == null || gravitySwitch.targetAnchor == null)
            return;

        currentMode = gravitySwitch.targetMode;

        Vector3 targetPos = gravitySwitch.targetAnchor.position;

        if (currentMode == SurfaceMode.Floor)
        {
            targetPos.x = Mathf.Round(targetPos.x);
            targetPos.z = Mathf.Round(targetPos.z);
        }

        cube.transform.position = targetPos;
        cube.UpdateVisual();

        switchCooldownTimer = switchCooldown;
    }

    public bool IsInsideCurrentBounds(Vector3 worldPosition)
    {
        if (currentMode == SurfaceMode.Floor && floorBounds != null)
            return floorBounds.Contains(worldPosition);

        if (currentMode == SurfaceMode.LeftWall && leftWallBounds != null)
            return leftWallBounds.Contains(worldPosition);

        if (currentMode == SurfaceMode.RightWall && rightWallBounds != null)
            return rightWallBounds.Contains(worldPosition);

        return true;
    }
}