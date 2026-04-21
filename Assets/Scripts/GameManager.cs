using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private List<CubeUnit> cubes = new List<CubeUnit>();
    private int selectedCubeIndex = 0;

    private LevelQuery levelQuery;
    private CubeActionSystem cubeActionSystem;
    private GravitySystem gravitySystem;
    private LevelUIController levelUIController;

    public bool IsLevelComplete { get; private set; } = false;

    private void Awake()
    {
        levelQuery = GetComponent<LevelQuery>();
        cubeActionSystem = GetComponent<CubeActionSystem>();
        gravitySystem = GetComponent<GravitySystem>();
        levelUIController = FindFirstObjectByType<LevelUIController>();
    }

    private void Start()
    {
        RefreshCubes();
        ClampSelectedIndex();
        UpdateSelectionVisual();
        UpdateGoalPreviews();
        EvaluateGoals();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (levelUIController != null && levelUIController.IsIntroOpen)
            return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            RefreshCubes();
            SelectNextCube();
        }

        Vector2Int moveInput = Vector2Int.zero;

        if (Keyboard.current.wKey.wasPressedThisFrame)
            moveInput = new Vector2Int(0, 1);
        else if (Keyboard.current.sKey.wasPressedThisFrame)
            moveInput = new Vector2Int(0, -1);
        else if (Keyboard.current.aKey.wasPressedThisFrame)
            moveInput = new Vector2Int(-1, 0);
        else if (Keyboard.current.dKey.wasPressedThisFrame)
            moveInput = new Vector2Int(1, 0);

        if (moveInput != Vector2Int.zero)
        {
            RefreshCubes();

            if (gravitySystem.CurrentMode == SurfaceMode.Floor)
                TryMoveAllCubesOnFloor(moveInput);
            else
                TryMoveSingleCubeOnWall(moveInput);

            RefreshCubes();
            TryActivateGravitySwitch();
            EvaluateGoals();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RefreshCubes();

            if (cubes.Count > 0)
            {
                cubeActionSystem.SplitSelectedCube(cubes, selectedCubeIndex);
                RefreshCubes();
                EvaluateGoals();
            }
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            RefreshCubes();

            if (cubes.Count > 0)
            {
                cubeActionSystem.MergeSelectedCube(cubes, ref selectedCubeIndex);
                RefreshCubes();
                EvaluateGoals();
            }
        }
    }

    private void RefreshCubes()
    {
        cubes = new List<CubeUnit>(FindObjectsByType<CubeUnit>(FindObjectsSortMode.None));
        ClampSelectedIndex();
        UpdateSelectionVisual();
        UpdateGoalPreviews();
    }

    private void ClampSelectedIndex()
    {
        if (cubes.Count == 0)
        {
            selectedCubeIndex = 0;
            return;
        }

        if (selectedCubeIndex < 0)
            selectedCubeIndex = 0;

        if (selectedCubeIndex >= cubes.Count)
            selectedCubeIndex = cubes.Count - 1;
    }

    private void SelectNextCube()
    {
        if (cubes.Count == 0)
            return;

        int startIndex = selectedCubeIndex;

        do
        {
            selectedCubeIndex++;
            if (selectedCubeIndex >= cubes.Count)
                selectedCubeIndex = 0;

            if (!cubes[selectedCubeIndex].isLockedOnGoal)
                break;

        } while (selectedCubeIndex != startIndex);

        UpdateSelectionVisual();
        UpdateGoalPreviews();
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < cubes.Count; i++)
        {
            if (cubes[i] == null)
                continue;

            if (cubes[i].isLockedOnGoal)
            {
                cubes[i].isSelected = false;
            }
            else
            {
                cubes[i].isSelected = (i == selectedCubeIndex);
            }
        }
    }

    private void UpdateGoalPreviews()
    {
        GoalZone[] goals = FindObjectsByType<GoalZone>(FindObjectsSortMode.None);

        CubeUnit selectedCube = null;
        if (cubes.Count > 0 && selectedCubeIndex >= 0 && selectedCubeIndex < cubes.Count)
            selectedCube = cubes[selectedCubeIndex];

        foreach (GoalZone goal in goals)
        {
            if (goal == null || goal.isCompleted)
                continue;

            goal.SetPreviewCube(selectedCube);
        }
    }

    private void TryMoveAllCubesOnFloor(Vector2Int direction)
    {
        Dictionary<CubeUnit, Vector2Int> currentCells = new Dictionary<CubeUnit, Vector2Int>();
        Dictionary<CubeUnit, Vector2Int> targetCells = new Dictionary<CubeUnit, Vector2Int>();
        HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        foreach (CubeUnit cube in cubes)
        {
            if (cube == null)
                continue;

            Vector2Int cell = levelQuery.GetGridCell(cube.transform.position);
            currentCells[cube] = cell;
            occupiedCells.Add(cell);
        }

        foreach (CubeUnit cube in cubes)
        {
            if (cube == null)
                continue;

            if (cube.isLockedOnGoal)
            {
                targetCells[cube] = currentCells[cube];
                continue;
            }

            Vector2Int currentCell = currentCells[cube];
            Vector2Int targetCell = currentCell + direction;

            Vector3 targetWorld = levelQuery.CellToWorld(targetCell, cube);

            bool insideFloorBounds = gravitySystem.IsInsideCurrentBounds(targetWorld);
            bool blockedByLevel = levelQuery.IsBlockedForCube(cube, targetCell);
            bool blockedByCube = occupiedCells.Contains(targetCell);

            if (insideFloorBounds && !blockedByLevel && !blockedByCube)
                targetCells[cube] = targetCell;
            else
                targetCells[cube] = currentCell;
        }

        HashSet<Vector2Int> reservedTargets = new HashSet<Vector2Int>();
        List<CubeUnit> cubesToStop = new List<CubeUnit>();

        foreach (var pair in targetCells)
        {
            CubeUnit cube = pair.Key;
            Vector2Int targetCell = pair.Value;
            Vector2Int currentCell = currentCells[cube];

            if (targetCell == currentCell)
                continue;

            if (reservedTargets.Contains(targetCell))
                cubesToStop.Add(cube);
            else
                reservedTargets.Add(targetCell);
        }

        foreach (CubeUnit cube in cubesToStop)
        {
            targetCells[cube] = currentCells[cube];
        }

        foreach (var pair in targetCells)
        {
            MoveCubeOnFloor(pair.Key, pair.Value);
        }
    }

    private void MoveCubeOnFloor(CubeUnit cube, Vector2Int cell)
    {
        if (cube == null)
            return;

        Vector3 pos = cube.transform.position;
        pos.x = cell.x;
        pos.z = cell.y;
        cube.transform.position = pos;
    }

    private void TryMoveSingleCubeOnWall(Vector2Int moveInput)
    {
        CubeUnit cube = GetSingleMovableCube();
        if (cube == null)
            return;

        Vector3 step = GetWallStep(moveInput);
        if (step == Vector3.zero)
            return;

        Vector3 targetPosition = cube.transform.position + step * cube.cellSize;

        if (!gravitySystem.IsInsideCurrentBounds(targetPosition))
            return;

        bool blockedByLevel = levelQuery.IsBlockedForCubeAtWorld(cube, targetPosition);

        if (!blockedByLevel)
        {
            cube.transform.position = targetPosition;
        }
    }

    private Vector3 GetWallStep(Vector2Int moveInput)
    {
        if (gravitySystem.CurrentMode == SurfaceMode.LeftWall)
        {
            if (moveInput.y > 0)
                return Vector3.up;

            if (moveInput.y < 0)
                return Vector3.down;

            if (moveInput.x < 0)
                return Vector3.back;

            if (moveInput.x > 0)
                return Vector3.forward;
        }

        if (gravitySystem.CurrentMode == SurfaceMode.RightWall)
        {
            if (moveInput.y > 0)
                return Vector3.up;

            if (moveInput.y < 0)
                return Vector3.down;

            if (moveInput.x < 0)
                return Vector3.forward;

            if (moveInput.x > 0)
                return Vector3.back;
        }

        return Vector3.zero;
    }

    private void TryActivateGravitySwitch()
    {
        if (gravitySystem.IsSwitchLocked)
            return;

        CubeUnit cube = GetSingleMovableCube();
        if (cube == null)
            return;

        GravitySwitch gravitySwitch = levelQuery.GetGravitySwitchAtWorld(
            cube.transform.position,
            gravitySystem.CurrentMode
        );

        if (gravitySwitch == null)
            return;

        gravitySystem.ApplySwitch(cube, gravitySwitch);
    }

    private void EvaluateGoals()
    {
        GoalZone[] goals = FindObjectsByType<GoalZone>(FindObjectsSortMode.None);

        foreach (GoalZone goal in goals)
        {
            if (goal == null || goal.isCompleted)
                continue;

            foreach (CubeUnit cube in cubes)
            {
                if (cube == null || cube.isLockedOnGoal)
                    continue;

                if (!IsCubeOnGoal(cube, goal))
                    continue;

                if (!goal.CanAcceptCube(cube))
                    continue;

                goal.CompleteWith(cube);
                break;
            }
        }

        CheckWin();
        RefreshCubes();
    }

    private bool IsCubeOnGoal(CubeUnit cube, GoalZone goal)
    {
        Vector3 cubePos = cube.transform.position;
        Vector3 goalPos = goal.transform.position;

        switch (goal.surfaceMode)
        {
            case SurfaceMode.Floor:
                return Mathf.RoundToInt(cubePos.x) == Mathf.RoundToInt(goalPos.x) &&
                       Mathf.RoundToInt(cubePos.z) == Mathf.RoundToInt(goalPos.z);

            case SurfaceMode.LeftWall:
            case SurfaceMode.RightWall:
                return Mathf.RoundToInt(cubePos.y) == Mathf.RoundToInt(goalPos.y) &&
                       Mathf.RoundToInt(cubePos.z) == Mathf.RoundToInt(goalPos.z);
        }

        return false;
    }

    private void CheckWin()
    {
        if (IsLevelComplete)
            return;

        GoalZone[] goals = FindObjectsByType<GoalZone>(FindObjectsSortMode.None);

        if (goals.Length == 0)
            return;

        foreach (GoalZone goal in goals)
        {
            if (goal == null)
                continue;

            if (!goal.isCompleted)
                return;
        }

        IsLevelComplete = true;
        Debug.Log("LEVEL COMPLETE");
    }

    private CubeUnit GetSingleMovableCube()
    {
        CubeUnit movableCube = null;
        int movableCount = 0;

        foreach (CubeUnit cube in cubes)
        {
            if (cube == null)
                continue;

            if (cube.isLockedOnGoal)
                continue;

            movableCube = cube;
            movableCount++;
        }

        if (movableCount == 1)
            return movableCube;

        return null;
    }
}