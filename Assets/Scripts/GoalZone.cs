using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [Header("Goal Requirements")]
    public bool requireColor = false;
    public CubeColor requiredColor = CubeColor.Red;

    public bool requireSize = false;
    public CubeSize requiredSize = CubeSize.Large;

    [Header("Goal Surface")]
    public SurfaceMode surfaceMode = SurfaceMode.Floor;

    [Header("Visual Size")]
    public float largeGoalScale = 1.15f;
    public float mediumGoalScale = 0.85f;
    public float smallGoalScale = 0.6f;
    public float neutralGoalScale = 0.9f;
    public float goalHeight = 0.1f;

    [Header("Placement")]
    public float floorTopY = 0.1f;
    public float floorOffset = 0.001f;

    public float leftWallX = -10f;
    public float rightWallX = 10f;
    public float wallOffset = 0.001f;

    [Header("Rainbow")]
    public float rainbowSpeed = 0.3f;
    public float rainbowSaturation = 0.5f;
    public float rainbowValue = 0.95f;

    [Header("State")]
    public bool isCompleted = false;
    public CubeUnit lockedCube;

    private Renderer zoneRenderer;
    private CubeUnit previewCube;

    private void Awake()
    {
        zoneRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void Update()
    {
        if (!isCompleted && !requireColor)
        {
            UpdateVisual();
        }
    }

    private void OnValidate()
    {
        zoneRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    public void SetPreviewCube(CubeUnit cube)
    {
        previewCube = cube;
        UpdateVisual();
    }

    public bool CanAcceptCube(CubeUnit cube)
    {
        if (cube == null)
            return false;

        if (isCompleted && lockedCube != cube)
            return false;

        if (requireColor && cube.cubeColor != requiredColor)
            return false;

        if (requireSize && cube.cubeSize != requiredSize)
            return false;

        return true;
    }

    public void CompleteWith(CubeUnit cube)
    {
        if (cube == null)
            return;

        isCompleted = true;
        lockedCube = cube;
        cube.isLockedOnGoal = true;

        UpdateVisual();
    }

    public void ResetGoal()
    {
        isCompleted = false;
        lockedCube = null;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        UpdateGoalScaleAndPosition();

        if (zoneRenderer == null)
            zoneRenderer = GetComponent<Renderer>();

        if (zoneRenderer == null)
            return;

        Color color;

        if (isCompleted)
        {
            color = Color.green;
        }
        else
        {
            color = GetGoalBaseColor();
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        zoneRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        zoneRenderer.SetPropertyBlock(block);
    }

    private void UpdateGoalScaleAndPosition()
    {
        float size = GetVisualGoalScale();

        Vector3 currentPos = transform.position;

        switch (surfaceMode)
        {
            case SurfaceMode.Floor:
                {
                    transform.localScale = new Vector3(size, goalHeight, size);
                    currentPos.y = floorTopY + goalHeight * 0.5f + floorOffset;
                    transform.position = currentPos;
                    break;
                }

            case SurfaceMode.LeftWall:
                {
                    transform.localScale = new Vector3(goalHeight, size, size);
                    currentPos.x = leftWallX + goalHeight * 0.5f + wallOffset;
                    transform.position = currentPos;
                    break;
                }

            case SurfaceMode.RightWall:
                {
                    transform.localScale = new Vector3(goalHeight, size, size);
                    currentPos.x = rightWallX - goalHeight * 0.5f - wallOffset;
                    transform.position = currentPos;
                    break;
                }
        }
    }

    private float GetVisualGoalScale()
    {
        if (requireSize)
            return GetScaleByCubeSize(requiredSize);

        if (previewCube != null)
        {
            if (!requireColor || previewCube.cubeColor == requiredColor)
                return GetScaleByCubeSize(previewCube.cubeSize);
        }

        return neutralGoalScale;
    }

    private float GetScaleByCubeSize(CubeSize size)
    {
        switch (size)
        {
            case CubeSize.Large:
                return largeGoalScale;

            case CubeSize.Medium:
                return mediumGoalScale;

            case CubeSize.Small:
                return smallGoalScale;
        }

        return neutralGoalScale;
    }

    private Color GetGoalBaseColor()
    {
        if (!requireColor)
        {
            float hue = (Time.time * rainbowSpeed) % 1f;
            return Color.HSVToRGB(hue, rainbowSaturation, rainbowValue);
        }

        switch (requiredColor)
        {
            case CubeColor.Red:
                return new Color(1f, 0.4f, 0.4f);

            case CubeColor.Blue:
                return new Color(0.4f, 0.4f, 1f);

            case CubeColor.Yellow:
                return new Color(1f, 1f, 0.4f);
        }

        return new Color(0.9f, 0.9f, 0.85f);
    }
}