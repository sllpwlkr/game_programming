using System.Collections.Generic;
using UnityEngine;

public class LevelQuery : MonoBehaviour
{
    public Vector3 overlapHalfExtents = new Vector3(0.45f, 0.55f, 0.45f);
    public float overlapCenterY = 0.5f;

    public bool IsBlockedForCube(CubeUnit cube, Vector2Int targetCell)
    {
        Vector3 worldPosition = new Vector3(targetCell.x, overlapCenterY, targetCell.y);
        return IsBlockedForCubeAtWorld(cube, worldPosition);
    }

    public bool IsBlockedForCubeAtWorld(CubeUnit cube, Vector3 worldPosition)
    {
        Collider[] hits = Physics.OverlapBox(
            worldPosition,
            overlapHalfExtents,
            Quaternion.identity
        );

        foreach (Collider hit in hits)
        {
            LevelBlock block = hit.GetComponent<LevelBlock>();
            if (block != null && block.BlocksCube(cube))
                return true;
        }

        return false;
    }

    public bool HasLevelBlockAtCell(Vector2Int cell)
    {
        Vector3 worldPosition = new Vector3(cell.x, overlapCenterY, cell.y);
        return HasLevelBlockAtWorld(worldPosition);
    }

    public bool HasLevelBlockAtWorld(Vector3 worldPosition)
    {
        Collider[] hits = Physics.OverlapBox(
            worldPosition,
            overlapHalfExtents,
            Quaternion.identity
        );

        foreach (Collider hit in hits)
        {
            LevelBlock block = hit.GetComponent<LevelBlock>();
            if (block != null)
                return true;
        }

        return false;
    }

    public bool IsCellOccupiedByCube(Vector2Int cell, List<CubeUnit> cubes, CubeUnit ignoreCube = null)
    {
        foreach (CubeUnit cube in cubes)
        {
            if (cube == null)
                continue;

            if (cube == ignoreCube)
                continue;

            if (GetGridCell(cube.transform.position) == cell)
                return true;
        }

        return false;
    }

    public CubeUnit GetCubeAtCell(Vector2Int cell, List<CubeUnit> cubes, CubeUnit ignoreCube = null)
    {
        foreach (CubeUnit cube in cubes)
        {
            if (cube == null)
                continue;

            if (cube == ignoreCube)
                continue;

            if (GetGridCell(cube.transform.position) == cell)
                return cube;
        }

        return null;
    }

    public Vector2Int GetGridCell(Vector3 position)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.z)
        );
    }

    public Vector3 CellToWorld(Vector2Int cell, CubeUnit referenceCube)
    {
        Vector3 pos = referenceCube.transform.position;
        pos.x = cell.x;
        pos.z = cell.y;
        return pos;
    }

    public GravitySwitch GetGravitySwitchAtWorld(Vector3 worldPosition, SurfaceMode currentMode)
    {
        Collider[] hits = Physics.OverlapBox(
            worldPosition,
            overlapHalfExtents,
            Quaternion.identity
        );

        foreach (Collider hit in hits)
        {
            GravitySwitch gravitySwitch = hit.GetComponent<GravitySwitch>();
            if (gravitySwitch != null && gravitySwitch.activeOnMode == currentMode)
                return gravitySwitch;
        }

        return null;
    }
}