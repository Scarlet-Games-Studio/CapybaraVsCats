using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverUI;       // Painel de Game Over
    public GameObject interfacePanel;   // UI normal do jogo
    public Button exitButton;

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
    }

    public void ShowGameOverScreen()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        if (interfacePanel != null)
            interfacePanel.SetActive(false); // Esconde a interface normal

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
