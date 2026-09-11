using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Text scoreText;
    public Text healthText;

    private int score = 0;

    void Awake()
    {
        if (instance == null || instance.gameObject.scene != gameObject.scene)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
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
