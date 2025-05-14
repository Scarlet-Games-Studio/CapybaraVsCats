using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static int score = 0;
    public Text scoreText;

    private void Start()
    {
        score = 0;
        UpdateScoreUI();
    }

    public static void AddScore(int amount)
    {
        score += amount;
        FindObjectOfType<ScoreManager>().UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}