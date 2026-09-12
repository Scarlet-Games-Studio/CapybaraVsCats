using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    static ScoreManager instance;
    public static int score = 0;
    public Text scoreText;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        score = 0;
        UpdateScoreUI();
    }

    public static void AddScore(int amount)
    {
        score += amount;
        if (instance != null) instance.UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
