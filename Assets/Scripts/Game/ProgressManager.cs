using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProgressManager
{
    public static void SaveProgress()
    {
        int currentStage = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("LastStage", currentStage);
        PlayerPrefs.Save();
    }

    public static int GetLastStage()
    {
        return PlayerPrefs.GetInt("LastStage", 0);
    }
}