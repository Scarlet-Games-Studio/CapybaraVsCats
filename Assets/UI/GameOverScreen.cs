using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Para gerenciar as cenas

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverUI; // Painel de Game Over
    public GameObject interfacePanel; // Interface do jogo
    public UnityEngine.UI.Button exitButton; // Botão para voltar ao MainMenu

    void Start()
    {
        gameOverUI.SetActive(false); // Garante que o painel de Game Over começa oculto

        // Certifique-se de associar o botão no Inspector, senão pode lançar um erro
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitInGame); // Liga o botão à função ExitInGame
        }
        else
        {
            Debug.LogWarning("O botão Exit não foi atribuído no Inspector!");
        }
    }

    // Mostra a tela de Game Over e pausa o jogo
    public void ShowGameOverScreen()
    {
        gameOverUI.SetActive(true);
        interfacePanel.SetActive(false);
        Time.timeScale = 0f; // Pausa o jogo
    }

    // Reinicia o jogo, recarregando a cena atual
    public void RestartGame()
    {
        Time.timeScale = 1f; // Retoma o tempo do jogo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena atual
    }

    // Volta para a cena MainMenu e desativa o GameOverUI
    public void ExitInGame()
    {
        Time.timeScale = 1f; // Retoma o tempo do jogo antes de mudar de cena

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false); // Oculta o painel GameOverUI
        }

        SceneManager.LoadScene("MainMenu"); // Carrega a cena MainMenu
    }
}
