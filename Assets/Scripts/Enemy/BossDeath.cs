using UnityEngine;

public class BossDeath : MonoBehaviour
{
    public StageManager stageManager;
    [SerializeField, Min(0)] int scoreValue = 1500;

    bool handled;

    public void NotifyDefeated()
    {
        if (handled) return;
        handled = true;
        ScoreManager.AddScore(scoreValue);

        if (GameManager.instance != null)
        {
            GameManager.instance.CompleteStage();
            return;
        }

        if (stageManager == null)
            stageManager = FindAnyObjectByType<StageManager>(FindObjectsInactive.Include);

        if (stageManager != null) stageManager.OnStageComplete();
        else Debug.LogWarning("StageManager não foi encontrado após derrotar o boss.", this);
    }
}
