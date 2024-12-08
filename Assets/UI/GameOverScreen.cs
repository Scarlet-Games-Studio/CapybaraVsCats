using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public GameObject interfacePanel;


    void Start()
    {
        gameOverUI.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        gameOverUI.SetActive(true);
        interfacePanel.SetActive(false);
        Time.timeScale = 0f; // Pausa o jogo
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Retoma o jogo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}


