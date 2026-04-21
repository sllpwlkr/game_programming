using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class LevelUIController : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public GameObject introPanel;
    public GameObject winPanel;

    [Header("Intro UI")]
    public TMP_Text pageText;
    public TMP_Text pageCounterText;
    public Button backButton;
    public Button nextButton;
    public Button startButton;

    [TextArea(3, 10)]
    public string[] introPages;

    [Header("Win UI")]
    public TMP_Text winText;
    public Button restartButton;
    public Button nextLevelButton;
    public Button mainMenuButton;

    private int currentPageIndex = 0;
    private bool winShown = false;

    public bool IsIntroOpen { get; private set; } = true;

    private void Start()
    {
        if (introPanel != null)
            introPanel.SetActive(true);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (backButton != null)
            backButton.onClick.AddListener(PrevPage);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (startButton != null)
            startButton.onClick.AddListener(CloseIntro);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);

        if (winText != null)
            winText.text = "Уровень пройден";

        UpdateIntroPage();
    }

    private void Update()
    {
        HandleHotkeys();

        if (winShown)
            return;

        if (gameManager == null)
            return;

        if (gameManager.IsLevelComplete)
        {
            ShowWinPanel();
        }
    }

    private void HandleHotkeys()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartLevel();
        }
    }

    private void UpdateIntroPage()
    {
        if (introPages == null || introPages.Length == 0)
        {
            if (pageText != null)
                pageText.text = "Инструкции не заданы";

            if (pageCounterText != null)
                pageCounterText.text = "";

            if (backButton != null)
                backButton.interactable = false;

            if (nextButton != null)
                nextButton.interactable = false;

            return;
        }

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, introPages.Length - 1);

        if (pageText != null)
            pageText.text = introPages[currentPageIndex];

        if (pageCounterText != null)
            pageCounterText.text = $"{currentPageIndex + 1} / {introPages.Length}";

        if (backButton != null)
            backButton.interactable = currentPageIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentPageIndex < introPages.Length - 1;
    }

    public void NextPage()
    {
        currentPageIndex++;
        UpdateIntroPage();
    }

    public void PrevPage()
    {
        currentPageIndex--;
        UpdateIntroPage();
    }

    public void CloseIntro()
    {
        IsIntroOpen = false;

        if (introPanel != null)
            introPanel.SetActive(false);
    }

    private void ShowWinPanel()
    {
        winShown = true;

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void LoadNextLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        int nextSceneIndex = currentScene.buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            Debug.Log("Следующего уровня нет");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}