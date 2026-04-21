using UnityEngine;

public enum BlockType
{
    NormalWall,
    ColorWall,
    SizeGate,
    ColorFloor
}

public enum WallColor
{
    Red,
    Blue,
    Yellow
}

public enum PassageSize
{
    Medium,
    Small
}

public class LevelBlock : MonoBehaviour
{
    public BlockType blockType = BlockType.NormalWall;
    public WallColor wallColor = WallColor.Red;
    public PassageSize passageSize = PassageSize.Medium;

    private Renderer blockRenderer;

    private void Awake()
    {
        blockRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void OnValidate()
    {
        blockRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    public bool BlocksCube(CubeUnit cube)
    {
        if (blockType == BlockType.NormalWall)
            return true;

        if (blockType == BlockType.ColorWall)
        {
            switch (wallColor)
            {
                case WallColor.Red:
                    return cube.cubeColor != CubeColor.Red;

                case WallColor.Blue:
                    return cube.cubeColor != CubeColor.Blue;

                case WallColor.Yellow:
                    return cube.cubeColor != CubeColor.Yellow;
            }
        }

        if (blockType == BlockType.SizeGate)
        {
            switch (passageSize)
            {
                case PassageSize.Medium:
                    return cube.cubeSize == CubeSize.Large;

                case PassageSize.Small:
                    return cube.cubeSize != CubeSize.Small;
            }
        }

        if (blockType == BlockType.ColorFloor)
        {
            switch (wallColor)
            {
                case WallColor.Red:
                    return cube.cubeColor != CubeColor.Red;

                case WallColor.Blue:
                    return cube.cubeColor != CubeColor.Blue;

                case WallColor.Yellow:
                    return cube.cubeColor != CubeColor.Yellow;
            }
        }

        return false;
    }

    public void UpdateVisual()
    {
        if (blockRenderer == null)
            blockRenderer = GetComponent<Renderer>();

        if (blockRenderer == null)
            return;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        blockRenderer.GetPropertyBlock(block);

        if (blockType == BlockType.NormalWall)
        {
            block.SetColor("_BaseColor", Color.gray);
        }
        else if (blockType == BlockType.ColorWall)
        {
            switch (wallColor)
            {
                case WallColor.Red:
                    block.SetColor("_BaseColor", Color.red);
                    break;

                case WallColor.Blue:
                    block.SetColor("_BaseColor", Color.blue);
                    break;

                case WallColor.Yellow:
                    block.SetColor("_BaseColor", Color.yellow);
                    break;
            }
        }
        else if (blockType == BlockType.SizeGate)
        {
            switch (passageSize)
            {
                case PassageSize.Medium:
                    block.SetColor("_BaseColor", new Color(1f, 0.5f, 0f));
                    break;

                case PassageSize.Small:
                    block.SetColor("_BaseColor", new Color(0.6f, 0f, 1f));
                    break;
            }
        }
        else if (blockType == BlockType.ColorFloor)
        {
            switch (wallColor)
            {
                case WallColor.Red:
                    block.SetColor("_BaseColor", new Color(1f, 0.3f, 0.3f));
                    break;

                case WallColor.Blue:
                    block.SetColor("_BaseColor", new Color(0.3f, 0.3f, 1f));
                    break;

                case WallColor.Yellow:
                    block.SetColor("_BaseColor", new Color(1f, 1f, 0.3f));
                    break;
            }
        }

        blockRenderer.SetPropertyBlock(block);
    }
}