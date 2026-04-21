using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowController : MonoBehaviour
{
    [Header("Main Menu")]
    public string firstLevelSceneName = "Level1";

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
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

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}