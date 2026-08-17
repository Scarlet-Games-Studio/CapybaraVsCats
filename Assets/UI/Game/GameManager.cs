using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GameOver()
    {
        // Carrega a cena de Game Over ou reinicia o jogo
        //SceneManager.LoadScene("GameOver");

        // Mostra a tela de Game Over
        GameOverScreen screen = FindFirstObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        if (screen != null)
            screen.ShowGameOverScreen();
        else
            Debug.LogWarning("GameOverScreen não foi encontrado na cena.");
    }

    public void RestartGame()
    {
        // Reinicia a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

