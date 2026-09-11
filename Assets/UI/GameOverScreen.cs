using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public GameObject interfacePanel;
    public Button exitButton;
    public Button restartButton;
    public TMP_Text finalScoreText;

    bool shown;

    void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
        SetGameplayInterfaceVisible(true);

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitInGame);
            exitButton.onClick.AddListener(ExitInGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    void OnDestroy()
    {
        if (exitButton != null) exitButton.onClick.RemoveListener(ExitInGame);
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartGame);
    }

    public void ShowGameOverScreen()
    {
        if (shown) return;
        shown = true;
        SetGameplayInterfaceVisible(false);

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            gameOverUI.transform.SetAsLastSibling();
        }

        if (finalScoreText != null)
            finalScoreText.text = "SCORE  " + ScoreManager.score.ToString("N0");

        Time.timeScale = 0f;
    }

    void SetGameplayInterfaceVisible(bool visible)
    {
        // Cenas antigas apontam interfacePanel para o objeto que contém o próprio
        // gameOverUI. Nesse caso, desligá-lo esconderia também o Game Over.
        if (interfacePanel == null || interfacePanel == gameObject || interfacePanel == gameOverUI)
            return;
        if (gameOverUI != null && gameOverUI.transform.IsChildOf(interfacePanel.transform))
            return;
        interfacePanel.SetActive(visible);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitInGame()
    {
        Time.timeScale = 1f;
        shown = false;
        if (gameOverUI != null) gameOverUI.SetActive(false);
        SetGameplayInterfaceVisible(true);

        const string mainMenu = "MainMenu";
        if (Application.CanStreamedLevelBeLoaded(mainMenu)) SceneManager.LoadScene(mainMenu);
        else Debug.LogError($"A cena '{mainMenu}' não está habilitada no Build Settings.");
    }
}
