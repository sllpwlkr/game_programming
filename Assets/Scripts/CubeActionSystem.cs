using System.Collections.Generic;
using UnityEngine;

public class CubeActionSystem : MonoBehaviour
{
    public GameObject cubePrefab;

    private LevelQuery levelQuery;
    private GravitySystem gravitySystem;

    private void Awake()
    {
        levelQuery = GetComponent<LevelQuery>();
        gravitySystem = GetComponent<GravitySystem>();
    }

    public void SplitSelectedCube(List<CubeUnit> cubes, int selectedCubeIndex)
    {
        if (gravitySystem.CurrentMode != SurfaceMode.Floor)
        {
            Debug.Log("Деление разрешено только на полу");
            return;
        }

        if (cubes == null || cubes.Count == 0)
        {
            Debug.Log("Нет кубов для деления");
            return;
        }

        if (selectedCubeIndex < 0 || selectedCubeIndex >= cubes.Count)
        {
            Debug.Log("Выбран некорректный индекс куба для деления");
            return;
        }

        CubeUnit selectedCube = cubes[selectedCubeIndex];

        if (selectedCube == null)
        {
            Debug.Log("Выбранный куб для деления = null");
            return;
        }

        if (!selectedCube.CanSplit())
        {
            Debug.Log("Этот куб нельзя делить");
            return;
        }

        Vector2Int currentCell = levelQuery.GetGridCell(selectedCube.transform.position);

        if (levelQuery.HasLevelBlockAtCell(currentCell))
        {
            Debug.Log("Нельзя делиться внутри особой клетки");
            return;
        }

        if (!TryGetSplitCells(cubes, selectedCube, currentCell, out Vector2Int firstCell, out Vector2Int secondCell))
        {
            Debug.Log("Нет места для деления");
            return;
        }

        Vector3 firstWorld = levelQuery.CellToWorld(firstCell, selectedCube);
        Vector3 secondWorld = levelQuery.CellToWorld(secondCell, selectedCube);

        CubeSize newSize = selectedCube.GetNextSmallerSize();
        (CubeColor firstColor, CubeColor secondColor) = GetSplitColors(selectedCube.cubeColor);

        CreateCube(firstWorld, newSize, firstColor);
        CreateCube(secondWorld, newSize, secondColor);

        Destroy(selectedCube.gameObject);
    }

    private bool TryGetSplitCells(
        List<CubeUnit> cubes,
        CubeUnit selectedCube,
        Vector2Int currentCell,
        out Vector2Int firstCell,
        out Vector2Int secondCell)
    {
        Vector2Int leftCell = currentCell + Vector2Int.left;
        Vector2Int rightCell = currentCell + Vector2Int.right;

        if (CanUseSplitPair(cubes, selectedCube, leftCell, rightCell))
        {
            firstCell = leftCell;
            secondCell = rightCell;
            return true;
        }

        Vector2Int upCell = currentCell + Vector2Int.up;
        Vector2Int downCell = currentCell + Vector2Int.down;

        if (CanUseSplitPair(cubes, selectedCube, upCell, downCell))
        {
            firstCell = upCell;
            secondCell = downCell;
            return true;
        }

        firstCell = currentCell;
        secondCell = currentCell;
        return false;
    }

    private bool CanUseSplitPair(
        List<CubeUnit> cubes,
        CubeUnit selectedCube,
        Vector2Int firstCell,
        Vector2Int secondCell)
    {
        return IsSplitCellFree(cubes, selectedCube, firstCell) &&
               IsSplitCellFree(cubes, selectedCube, secondCell);
    }

    private bool IsSplitCellFree(
        List<CubeUnit> cubes,
        CubeUnit selectedCube,
        Vector2Int cell)
    {
        Vector3 world = levelQuery.CellToWorld(cell, selectedCube);

        bool insideFloorBounds = gravitySystem.IsInsideCurrentBounds(world);
        bool blockedByLevel = levelQuery.IsBlockedForCube(selectedCube, cell);
        bool blockedByCube = levelQuery.IsCellOccupiedByCube(cell, cubes, selectedCube);

        return insideFloorBounds && !blockedByLevel && !blockedByCube;
    }

    public void MergeSelectedCube(List<CubeUnit> cubes, ref int selectedCubeIndex)
    {
        if (gravitySystem.CurrentMode != SurfaceMode.Floor)
        {
            Debug.Log("Объединение разрешено только на полу");
            return;
        }

        if (cubes == null || cubes.Count == 0)
        {
            Debug.Log("Нет кубов для объединения");
            return;
        }

        if (selectedCubeIndex < 0 || selectedCubeIndex >= cubes.Count)
        {
            Debug.Log("Выбран некорректный индекс куба для объединения");
            return;
        }

        CubeUnit selectedCube = cubes[selectedCubeIndex];

        if (selectedCube == null)
        {
            Debug.Log("Выбранный куб для объединения = null");
            return;
        }

        if (selectedCube.cubeSize == CubeSize.Large)
        {
            Debug.Log("Large куб уже нельзя увеличить");
            return;
        }

        Vector2Int selectedCell = levelQuery.GetGridCell(selectedCube.transform.position);

        Debug.Log($"Пытаемся объединить куб: color={selectedCube.cubeColor}, size={selectedCube.cubeSize}, cell={selectedCell}");

        if (levelQuery.HasLevelBlockAtCell(selectedCell))
        {
            Debug.Log("Нельзя объединяться внутри особой клетки (у выбранного куба)");
            return;
        }

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborCell = selectedCell + direction;
            CubeUnit neighborCube = levelQuery.GetCubeAtCell(neighborCell, cubes, selectedCube);

            if (neighborCube == null)
            {
                Debug.Log($"Сосед в направлении {direction} не найден");
                continue;
            }

            Debug.Log($"Найден сосед: color={neighborCube.cubeColor}, size={neighborCube.cubeSize}, cell={neighborCell}");

            if (neighborCube.cubeSize != selectedCube.cubeSize)
            {
                Debug.Log("Размеры кубов не совпадают");
                continue;
            }

            Vector2Int neighborGridCell = levelQuery.GetGridCell(neighborCube.transform.position);

            if (levelQuery.HasLevelBlockAtCell(neighborGridCell))
            {
                Debug.Log("Нельзя объединяться: сосед стоит в особой клетке");
                continue;
            }

            if (!TryGetMergedColor(selectedCube.cubeColor, neighborCube.cubeColor, out CubeColor mergedColor))
            {
                Debug.Log("Цвета не дают допустимое объединение");
                continue;
            }

            Vector3 spawnPosition = selectedCube.transform.position;

            if (!gravitySystem.IsInsideCurrentBounds(spawnPosition))
            {
                Debug.Log("Новый куб появился бы вне допустимых границ");
                continue;
            }

            CubeSize mergedSize = selectedCube.GetNextLargerSize();

            Debug.Log($"Объединение успешно: новый cube color={mergedColor}, size={mergedSize}");

            Destroy(selectedCube.gameObject);
            Destroy(neighborCube.gameObject);

            CubeUnit newCube = CreateCube(spawnPosition, mergedSize, mergedColor);

            CubeUnit[] refreshed = FindObjectsByType<CubeUnit>(FindObjectsSortMode.None);
            List<CubeUnit> refreshedList = new List<CubeUnit>(refreshed);

            int newIndex = refreshedList.IndexOf(newCube);
            if (newIndex >= 0)
                selectedCubeIndex = newIndex;
            else
                selectedCubeIndex = 0;

            return;
        }

        Debug.Log("Подходящего соседа для объединения нет");
    }

    private CubeUnit CreateCube(Vector3 position, CubeSize size, CubeColor color)
    {
        GameObject obj = Instantiate(cubePrefab, position, Quaternion.identity);
        CubeUnit cube = obj.GetComponent<CubeUnit>();

        cube.cubeSize = size;
        cube.cubeColor = color;
        cube.isSelected = false;
        cube.UpdateVisual();

        return cube;
    }

    private (CubeColor, CubeColor) GetSplitColors(CubeColor color)
    {
        switch (color)
        {
            case CubeColor.Red:
                return (CubeColor.Blue, CubeColor.Yellow);

            case CubeColor.Blue:
                return (CubeColor.Red, CubeColor.Yellow);

            case CubeColor.Yellow:
                return (CubeColor.Red, CubeColor.Blue);

            default:
                return (CubeColor.Red, CubeColor.Blue);
        }
    }

    private bool TryGetMergedColor(CubeColor a, CubeColor b, out CubeColor mergedColor)
    {
        if (a == b)
        {
            mergedColor = a;
            return true;
        }

        if ((a == CubeColor.Blue && b == CubeColor.Yellow) ||
            (a == CubeColor.Yellow && b == CubeColor.Blue))
        {
            mergedColor = CubeColor.Red;
            return true;
        }

        if ((a == CubeColor.Red && b == CubeColor.Yellow) ||
            (a == CubeColor.Yellow && b == CubeColor.Red))
        {
            mergedColor = CubeColor.Blue;
            return true;
        }

        if ((a == CubeColor.Red && b == CubeColor.Blue) ||
            (a == CubeColor.Blue && b == CubeColor.Red))
        {
            mergedColor = CubeColor.Yellow;
            return true;
        }

        mergedColor = CubeColor.Red;
        return false;
    }
}