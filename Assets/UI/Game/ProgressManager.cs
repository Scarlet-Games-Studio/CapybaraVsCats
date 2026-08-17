using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProgressManager
{
    // Salva o índice da fase atual
    public static void SaveProgress()
    {
        int currentStage = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("LastStage", currentStage);
        PlayerPrefs.SetInt("HasPlayed", 1);
        PlayerPrefs.Save();
    }

    public static bool HasPlayedBefore() => PlayerPrefs.GetInt("HasPlayed", 0) == 1;

    // Retorna a última fase jogada
    public static int GetLastStage()
    {
        return PlayerPrefs.GetInt("LastStage", 0);
    }

    // 🔹 Salva o score da última fase concluída
    public static void SaveStageScore(int score)
    {
        PlayerPrefs.SetInt("LastStageScore", score);
        PlayerPrefs.SetInt("BestScore", Mathf.Max(score, PlayerPrefs.GetInt("BestScore", 0)));
        SaveLocalRankingScore(score);
        PlayerPrefs.Save();
    }

    static void SaveLocalRankingScore(int score)
    {
        if (score <= 0) return;
        const int size = 10;
        int[] scores = new int[size + 1];
        for (int i = 0; i < size; i++) scores[i] = PlayerPrefs.GetInt($"LocalRank_{i}", 0);
        scores[size] = score;
        System.Array.Sort(scores);
        System.Array.Reverse(scores);
        for (int i = 0; i < size; i++) PlayerPrefs.SetInt($"LocalRank_{i}", scores[i]);
    }

    public static int GetBestScore() => PlayerPrefs.GetInt("BestScore", GetLastStageScore());

    public static int[] GetLocalRanking()
    {
        int[] scores = new int[10];
        for (int i = 0; i < scores.Length; i++) scores[i] = PlayerPrefs.GetInt($"LocalRank_{i}", 0);
        return scores;
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
