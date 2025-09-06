using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StageManager : MonoBehaviour
{
    [Header("Stage Complete UI")]
    public GameObject stageCompleteUI;
    public TMP_Text scoreViewText; // texto do score
    public GameObject stars;       // painel de estrelas
    public Button nextButton;
    public Button exitButton;
    public string nextSceneName;

    [Header("Configurações da Fase")]
    public int maxScore = 10000; // score máximo possível da fase

    private void Start()
    {
        stageCompleteUI.SetActive(false);
        nextButton.interactable = false;

        exitButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MainMenu");
        });

        nextButton.onClick.AddListener(() => {
            ProgressManager.SaveProgress();
            ProgressManager.SaveStageScore(ScoreManager.score);
            SceneManager.LoadScene(nextSceneName);
        });
    }

    public void OnStageComplete()
    {
        stageCompleteUI.SetActive(true);

        // Atualiza o score
        if (scoreViewText != null)
            scoreViewText.text = "Score da Fase: " + ScoreManager.score;

        // Atualiza estrelas
        if (stars != null)
            UpdateStars(ScoreManager.score, maxScore);

        ProgressManager.SaveProgress();
        ProgressManager.SaveStageScore(ScoreManager.score);
    }

    public void EnableNextButton()
    {
        nextButton.interactable = true;
    }

    /// <summary>
    /// Preenche as estrelas proporcionalmente ao score em relação ao máximo da fase
    /// </summary>
    /// <param name="score">Score da fase</param>
    /// <param name="maxScore">Score máximo da fase</param>
    private void UpdateStars(int score, int maxScore)
    {
        Image[] starImages = stars.GetComponentsInChildren<Image>();

        // Limpa todas as estrelas
        foreach (var star in starImages)
            star.fillAmount = 0f;

        int starCount = starImages.Length;

        // Calcula o score como porcentagem
        float percent = Mathf.Clamp01((float)score / maxScore);

        // Cada estrela representa uma fração
        float fillPerStar = 1f / starCount;
        float remaining = percent;

        for (int i = 0; i < starCount; i++)
        {
            if (remaining >= fillPerStar)
            {
                starImages[i].fillAmount = 1f; // estrela cheia
                remaining -= fillPerStar;
            }
            else if (remaining > 0f)
            {
                starImages[i].fillAmount = remaining / fillPerStar; // estrela parcial
                remaining = 0f;
            }
            else
            {
                starImages[i].fillAmount = 0f;
            }
        }
    }
}
