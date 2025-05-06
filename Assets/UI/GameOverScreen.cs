using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public GameObject interfacePanel;
    public Button exitButton;

    void Start()
    {
        gameOverUI.SetActive(false);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitInGame);
    }

    public void ShowGameOverScreen()
    {
        gameOverUI.SetActive(true);
        interfacePanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitInGame()
    {
        Time.timeScale = 1f;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
