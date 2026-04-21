using UnityEngine;

public enum CubeColor
{
    Red,
    Blue,
    Yellow
}

public enum CubeSize
{
    Large,
    Medium,
    Small
}

public class CubeUnit : MonoBehaviour
{
    public float cellSize = 1f;
    public CubeColor cubeColor = CubeColor.Red;
    public CubeSize cubeSize = CubeSize.Large;

    [Header("Surface Detection")]
    public LayerMask surfaceLayer;

    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool isLockedOnGoal = false;

    private Renderer cubeRenderer;

    private void Awake()
    {
        cubeRenderer = GetComponent<Renderer>();
        UpdateSize();
    }

    private void Start()
    {
        UpdateSize();
        UpdateColorVisual();
    }

    private void Update()
    {
        UpdateColorVisual();
    }

    private void OnValidate()
    {
        cubeRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        UpdateSize();
        UpdateColorVisual();
    }

    private void UpdateColorVisual()
    {
        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();

        if (cubeRenderer == null)
            return;

        Color baseColor = GetBaseColor();

        if (isLockedOnGoal)
        {
            baseColor = Color.green;
        }
        else if (isSelected)
        {
            float t = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
            baseColor = Color.Lerp(baseColor, Color.white, t);
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        cubeRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", baseColor);
        cubeRenderer.SetPropertyBlock(block);
    }

    private Color GetBaseColor()
    {
        switch (cubeColor)
        {
            case CubeColor.Red:
                return Color.red;

            case CubeColor.Blue:
                return Color.blue;

            case CubeColor.Yellow:
                return Color.yellow;
        }

        return Color.white;
    }

    private void UpdateSize()
    {
        float visualScale = GetVisualScale(cubeSize);

        transform.localScale = new Vector3(visualScale, visualScale, visualScale);

        float floorTopY = FindFloorTopY();
        Vector3 pos = transform.position;
        pos.y = floorTopY + visualScale * 0.5f;
        transform.position = pos;
    }

    private float FindFloorTopY()
    {
        float rayStartHeight = 5f;
        Vector3 origin = new Vector3(
            transform.position.x,
            transform.position.y + rayStartHeight,
            transform.position.z
        );

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20f, surfaceLayer))
        {
            return hit.point.y;
        }

        return 0f;
    }

    public float GetVisualScale(CubeSize size)
    {
        switch (size)
        {
            case CubeSize.Large:
                return 1f;

            case CubeSize.Medium:
                return 0.7f;

            case CubeSize.Small:
                return 0.45f;
        }

        return 1f;
    }

    public bool CanSplit()
    {
        return cubeSize != CubeSize.Small && !isLockedOnGoal;
    }

    public CubeSize GetNextSmallerSize()
    {
        switch (cubeSize)
        {
            case CubeSize.Large:
                return CubeSize.Medium;
            case CubeSize.Medium:
                return CubeSize.Small;
            default:
                return CubeSize.Small;
        }
    }

    public CubeSize GetNextLargerSize()
    {
        switch (cubeSize)
        {
            case CubeSize.Small:
                return CubeSize.Medium;
            case CubeSize.Medium:
                return CubeSize.Large;
            default:
                return CubeSize.Large;
        }
    }
}