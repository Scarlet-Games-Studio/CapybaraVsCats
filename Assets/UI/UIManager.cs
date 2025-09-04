using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Text scoreText;
    public Text healthText;

    private int score = 0;

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // mantém o Canvas entre cenas
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Atualiza referência do scoreText ao carregar a cena
        scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();

        // Atualiza o texto do score com o valor atual
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateScore(int points)
    {
        score += points;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateHealth(int health)
    {
        if (healthText != null)
            healthText.text = "  : " + health; // sem alterações
    }

    public void ResetScore()
    {
        score = 0;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}
