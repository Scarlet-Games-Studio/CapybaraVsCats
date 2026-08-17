using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverUI;       // Painel de Game Over
    public GameObject interfacePanel;   // UI normal do jogo
    public Button exitButton;
    public Button restartButton;
    public TMP_Text finalScoreText;

    void Start()
    {
        // Garantir que o Game Over comece desativado
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        // Garantir que a interface normal esteja ativa no início
        if (interfacePanel != null)
            interfacePanel.SetActive(true);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitInGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    public void ShowGameOverScreen()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        if (interfacePanel != null)
            interfacePanel.SetActive(false); // Esconde a interface normal

        if (finalScoreText != null)
            finalScoreText.text = "SCORE  " + ScoreManager.score.ToString("N0");

        Time.timeScale = 0f; // Pausa o jogo
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Volta o tempo ao normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena
    }

    public void ExitInGame()
    {
        Time.timeScale = 1f;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (interfacePanel != null)
            interfacePanel.SetActive(true); // Garante que a interface normal volte

        SceneManager.LoadScene("MainMenu");
    }
}
