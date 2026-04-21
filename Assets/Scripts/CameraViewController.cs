using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraViewMode
{
    Auto,
    Floor,
    LeftWall,
    RightWall
}

public class CameraViewController : MonoBehaviour
{
    [Header("References")]
    public GravitySystem gravitySystem;

    [Header("Anchors")]
    public Transform floorAnchor;
    public Transform leftWallAnchor;
    public Transform rightWallAnchor;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotateSpeed = 6f;

    [Header("Mode")]
    public CameraViewMode currentViewMode = CameraViewMode.Auto;

    private void Update()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            currentViewMode = CameraViewMode.Floor;
            Debug.Log("Camera -> Floor");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            currentViewMode = CameraViewMode.LeftWall;
            Debug.Log("Camera -> LeftWall");
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            currentViewMode = CameraViewMode.RightWall;
            Debug.Log("Camera -> RightWall");
        }

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            currentViewMode = CameraViewMode.Auto;
            Debug.Log("Camera -> Auto");
        }
    }

    private void UpdateCameraPosition()
    {
        Transform targetAnchor = GetTargetAnchor();

        if (targetAnchor == null)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetAnchor.position,
            Time.deltaTime * moveSpeed
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetAnchor.rotation,
            Time.deltaTime * rotateSpeed
        );
    }

    private Transform GetTargetAnchor()
    {
        CameraViewMode targetMode = currentViewMode;

        if (currentViewMode == CameraViewMode.Auto)
        {
            if (gravitySystem == null)
                return floorAnchor;

            switch (gravitySystem.CurrentMode)
            {
                case SurfaceMode.Floor:
                    targetMode = CameraViewMode.Floor;
                    break;

                case SurfaceMode.LeftWall:
                    targetMode = CameraViewMode.LeftWall;
                    break;

                case SurfaceMode.RightWall:
                    targetMode = CameraViewMode.RightWall;
                    break;
            }
        }

        switch (targetMode)
        {
            case CameraViewMode.Floor:
                return floorAnchor;

            case CameraViewMode.LeftWall:
                return leftWallAnchor;

            case CameraViewMode.RightWall:
                return rightWallAnchor;

            default:
                return floorAnchor;
        }
    }
}