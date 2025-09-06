using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProgressManager
{
    // Salva o índice da fase atual
    public static void SaveProgress()
    {
        int currentStage = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("LastStage", currentStage);
        PlayerPrefs.Save();
    }

    // Retorna a última fase jogada
    public static int GetLastStage()
    {
        return PlayerPrefs.GetInt("LastStage", 0);
    }

    // 🔹 Salva o score da última fase concluída
    public static void SaveStageScore(int score)
    {
        PlayerPrefs.SetInt("LastStageScore", score);
        PlayerPrefs.Save();
    }

    // 🔹 Recupera o score da última fase concluída
    public static int GetLastStageScore()
    {
        return PlayerPrefs.GetInt("LastStageScore", 0);
    }

    // Reseta tudo (caso queira começar novo jogo)
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
    }
}
