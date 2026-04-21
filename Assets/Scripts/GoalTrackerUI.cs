using System.Text;
using UnityEngine;
using TMPro;

public class GoalTrackerUI : MonoBehaviour
{
    public TMP_Text trackerText;
    public GameObject panelRoot;

    private LevelUIController levelUIController;

    private void Start()
    {
        levelUIController = FindFirstObjectByType<LevelUIController>();
        UpdateVisibility();
        UpdateTracker();
    }

    private void Update()
    {
        UpdateVisibility();
        UpdateTracker();
    }

    private void UpdateVisibility()
    {
        if (panelRoot == null)
            return;

        if (levelUIController == null)
        {
            panelRoot.SetActive(true);
            return;
        }

        panelRoot.SetActive(!levelUIController.IsIntroOpen);
    }

    private void UpdateTracker()
    {
        if (trackerText == null)
            return;

        if (levelUIController != null && levelUIController.IsIntroOpen)
            return;

        GoalZone[] goals = FindObjectsByType<GoalZone>(FindObjectsSortMode.None);

        if (goals == null || goals.Length == 0)
        {
            trackerText.text = "";
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Цели:");

        foreach (GoalZone goal in goals)
        {
            if (goal == null)
                continue;

            string description = GetGoalDescription(goal);

            if (goal.isCompleted)
            {
                sb.AppendLine($"<color=#4CAF50>[x]</color> {description}");
            }
            else
            {
                sb.AppendLine($"[ ] {description}");
            }
        }

        trackerText.text = sb.ToString();
    }

    private string GetGoalDescription(GoalZone goal)
    {
        string colorPart = "любой цвет";
        string sizePart = "любой размер";
        string surfacePart = "";

        if (goal.requireColor)
        {
            switch (goal.requiredColor)
            {
                case CubeColor.Red:
                    colorPart = "красный";
                    break;
                case CubeColor.Blue:
                    colorPart = "синий";
                    break;
                case CubeColor.Yellow:
                    colorPart = "жёлтый";
                    break;
            }
        }

        if (goal.requireSize)
        {
            switch (goal.requiredSize)
            {
                case CubeSize.Large:
                    sizePart = "большой";
                    break;
                case CubeSize.Medium:
                    sizePart = "средний";
                    break;
                case CubeSize.Small:
                    sizePart = "маленький";
                    break;
            }
        }

        switch (goal.surfaceMode)
        {
            case SurfaceMode.Floor:
                surfacePart = " (пол)";
                break;

            case SurfaceMode.LeftWall:
            case SurfaceMode.RightWall:
                surfacePart = " (стена)";
                break;
        }

        if (!goal.requireColor && !goal.requireSize)
            return "любой куб" + surfacePart;

        if (goal.requireColor && !goal.requireSize)
            return colorPart + " куб" + surfacePart;

        if (!goal.requireColor && goal.requireSize)
            return sizePart + " куб" + surfacePart;

        return colorPart + " " + sizePart + " куб" + surfacePart;
    }
}